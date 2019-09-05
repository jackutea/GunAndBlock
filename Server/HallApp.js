var event = require("events");
var cluster = require("cluster");
var Redis = require("redis");

var mongoDB = require("./mongoDB/mongoDB");

var LoginSend = require("./AllClass/LoginSend");
var RoleListSend = require("./AllClass/RoleListSend");
var ServerSend = require("./AllClass/ServerSend");
var AccountState = require("./AllClass/State/AccountState");
var RoleState = require("./AllClass/State/RoleState");

class HallApp extends event {

    constructor(redisPort, redisIp) {

        super();

        HallApp.instance = this;

        this.rds = Redis.createClient(redisPort, redisIp);

        this.initClusterListener();

        this.initHallClusterListener();

    }

    // 初始化进程监听器
    initClusterListener() {

        cluster.worker.on("message", (msg) => {

            let eventName = msg.eventName;

            let dataString = msg.dataString;

            let sid = msg.sid;

            this.emit(eventName, dataString, sid);

            // console.log("Hall App Recv : ", msg);

        });

        cluster.worker.on("error", (err) => {

            console.log("HallApp Error :", err);

        })

    }

    // 初始化进程监听事件
    initHallClusterListener() {

        this.on("error", (err) => {

            console.log("HallApp Event Error:", err);
            
        })

        // 处理客户端请求
        this.on("Register", this.register); // 注册

        this.on("Login", this.login); // 登录

        this.on("ShowServer", this.showServer); // 显示服务器列表

        this.on("ShowRoles", this.showRoles); // 显示角色列表

        this.on("CreateRole", this.createRole); // 创建角色

        this.on("DeleteRole", this.deleteRole); // 删除角色

        this.on("EnterGame", this.enterGame); // 选定角色后进入游戏

        this.on("RequestRoleState", this.RequestRoleState); // 重新请求角色属性

        this.on("ShowRoom", this.showRoom); // 显示服务器自定义房间

    }

    // 注册
    register(dataString, sid) {

        process.nextTick(() => {

            let UserInfo = JSON.parse(dataString);

            let username = UserInfo.username;

            let password = UserInfo.password;

            let usernameobj = {username : username};

            mongoDB.findOne("account", usernameobj, (err, result) => {

                if (err) throw err;

                if (!result) {

                    console.log(username, "用户名不存在，可注册");

                    let accountState = new AccountState();

                    accountState.username = username;

                    accountState.password = password;

                    this.rds.hset(sid, "username", username);

                    this.rds.hset(username, "sid", sid);

                    mongoDB.insertOne("account", accountState, (err3, result) => {

                        let loginSendInfo = new LoginSend(0, "注册成功，直接登录");

                        process.send({ eventName: "Register", dataString: JSON.stringify(loginSendInfo), sid: sid })

                    });

                } else {

                    let loginSendInfo = new LoginSend(3, username + "已存在，无法注册");

                    process.send({ eventName: "Register", dataString: JSON.stringify(loginSendInfo), sid: sid })

                }
            });
        })
    }

    // 登录
    login(dataString, sid) {

        process.nextTick(() => {

            let UserInfo = JSON.parse(dataString);

            let username = UserInfo.username;

            let password = UserInfo.password;

            let usernameobj = {username : username};

            mongoDB.findOne("account", usernameobj, (err, result) => {

                if (err) throw err;

                if (!result) {

                    console.log(username, "用户名不存在");

                    let loginSendInfo = new LoginSend(2, "用户名不存在");

                    process.send({ eventName: "Login", dataString: JSON.stringify(loginSendInfo), sid: sid});

                } else {

                    if (result.password == password) {

                        // console.log(username, "密码正确，直接登录");

                        this.rds.hget(username, "sid", (err, _sid) => {

                            // TODO 删除SID USERNAME对
                            // TODO 清除排队信息
                            // TODO 退出战场

                            if (_sid) {

                                this.rds.hset(username, "sid", sid);

                                this.rds.hdel(_sid, "username");

                                this.rds.hset(sid, "username", username);

                            } else {

                                this.rds.hset(sid, "username", username);

                                this.rds.hset(username, "sid", sid);

                            }
                        })

                        let loginSendInfo = new LoginSend(0, "登录成功");

                        process.send({ eventName: "Login", dataString: JSON.stringify(loginSendInfo), sid: sid});

                    } else {

                        console.log(username, "密码错误");

                        let loginSendInfo = new LoginSend(1, "密码错误");

                        process.send({ eventName: "Login", dataString: JSON.stringify(loginSendInfo), sid: sid});

                    }
                }
            });
        })
    }

    // 显示服务器列表
    showServer(dataString, sid) {

        process.nextTick(() => {

            let serverSendInfo = new ServerSend();

            this.rds.hkeys("serverList", (err0, data0) => {

                serverSendInfo.serverIdList = data0;

                this.rds.hvals("serverList", (err1, data1) => {

                    serverSendInfo.serverNameList = data1;
                    
                    console.log(data0,data1);

                    process.send({ eventName: "ShowServer", dataString: JSON.stringify(serverSendInfo), sid: sid});

                })
            })
        })
    }

    // 显示角色
    showRoles(dataString, sid) {

        process.nextTick(() => {

            let serverId = parseInt(dataString);

            this.rds.hget(sid, "username", (err0, data) => {

                let username = data;

                let findObj = {inServerId : serverId, username : username};

                mongoDB.find("role", findObj, (err1, result) => {

                    let roleListSendInfo = new RoleListSend();

                    roleListSendInfo.roleJson = {};

                    for (let i = 0; i < result.length; i += 1) {

                        let roleState = result[i];

                        delete roleState["_id"];

                        let roleName = roleState.roleName;

                        roleListSendInfo.roleJson[roleName] = roleState;

                    }

                    process.send({ eventName: "ShowRoles", dataString: JSON.stringify(roleListSendInfo), sid: sid});

                });
            });
        })
    }

    // 创建角色
    createRole(dataString, sid) {

        process.nextTick(() => {

            let roleInfo = JSON.parse(dataString);

            let roleName = roleInfo.roleName;

            let serverId = roleInfo.serverId;

            this.rds.hget(sid, "username", (err0, data) => {

                let username = data;

                let roleState = new RoleState();

                roleState.roleName = roleName;

                roleState.username = username;

                roleState.inServerId = serverId;

                let findObj = {roleName : roleName};

                mongoDB.findOne("role", findObj, (err1, result) => {

                    if (result) {

                        process.send({ eventName: "CreateRoleFail", dataString: "", sid: sid});

                    } else {

                        mongoDB.insertOne("role", roleState, (err2, result) => {

                            process.send({ eventName: "CreateRole", dataString: JSON.stringify(roleState), sid: sid});
                
                        });
                    }
                })
            });
        })
    }

    // 删除角色
    deleteRole(dataString, sid) {

        process.nextTick(() => {

            let roleName = dataString;

            let delObj = {roleName : roleName};

            mongoDB.deleteOne("role", delObj, (err, result) => {

                process.send({ eventName: "DeleteRole", dataString: roleName, sid: sid});

            });
        })
    }

    // 进入游戏
    enterGame(dataString, sid) {

        process.nextTick(() => {

            let roleState = JSON.parse(dataString); // TODO : 这里有可能被利用

            this.rds.hget(sid, "username", (err0, data) => {

                let username = data;

                this.rds.hset(username, "roleState", JSON.stringify(roleState))

                process.send({ eventName: "EnterGame", dataString: "", sid: sid});

            });
        })
    }

    // 重新请求角色属性
    RequestRoleState(dataString, sid) {

        process.nextTick(() => {

            let roleName = dataString;

            mongoDB.findOne("role", { roleName: roleName }, (err, result) => {

                console.log("请求角色", result);

                process.send({ eventName: "RequestRoleState", dataString: JSON.stringify(result), sid: sid});

            });
        })
    }

    // TODO : 显示自定义房间
    showRoom(dataString, sid) {

    }
}

module.exports = HallApp;