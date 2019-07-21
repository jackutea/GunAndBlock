var event = require("events");
var cluster = require("cluster");

var mongoDB = require("./mongoDB/mongoDB");

var Factory = require("./AllClass/Factory");
var HALL_GD = require("./GlobalData/HALL_GD");

class HallApp extends event {

    constructor() {

        super();

        HallApp.instance = this;

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

    // 初始化全局变量
    initHallGD(dataString, sid) {

        HALL_GD.CreateGD(() => {

            process.send({ eventName: "InitCompareGD", dataString: HALL_GD.SERVER_LIST, sid: sid});

            process.send({ eventName: "InitBattleGD", dataString: "", sid: sid});

        });

    }

    // 初始化进程监听事件
    initHallClusterListener() {

        this.on("error", (err) => {

            console.log("HallApp Event Error:", err);
            
        })

        // 处理主线程请求
        this.on("InitHallGD", this.initHallGD);

        // 处理客户端请求
        this.on("Register", this.register); // 注册

        this.on("Login", this.login); // 登录

        this.on("ShowServer", this.showServer); // 显示服务器列表

        this.on("ShowRole", this.showRole); // 显示角色列表

        this.on("CreateRole", this.createRole); // 创建角色

        this.on("DeleteRole", this.deleteRole); // 删除角色

        this.on("EnterGame", this.enterGame); // 选定角色后进入游戏

        this.on("ShowRoom", this.showRoom); // 显示服务器自定义房间

        this.on("RequestHallGD", this.requestHallGD);

        this.on("RequestRoleState", this.requestRoleState);

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

                    let accountState = new Factory.AccountState();

                    accountState.username = username;

                    accountState.password = password;

                    HALL_GD.ONLINE_ACCOUNT[sid] = username; // 添加玩家信息

                    mongoDB.insertOne("account", accountState, (err, result) => {

                        let loginSendInfo = new Factory.LoginSendInfo(0, "注册成功，直接登录");

                        process.send({ eventName: "Register", dataString: JSON.stringify(loginSendInfo), sid: sid })

                    });

                } else {

                    let loginSendInfo = new Factory.LoginSendInfo(3, username + "已存在，无法注册");

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

                    let loginSendInfo = new Factory.LoginSendInfo(2, "用户名不存在");

                    process.send({ eventName: "Login", dataString: JSON.stringify(loginSendInfo), sid: sid});

                } else {

                    if (result.password == password) {

                        // console.log(username, "密码正确，直接登录");

                        HALL_GD.ONLINE_ACCOUNT[sid] = username; // 添加玩家信息

                        let loginSendInfo = new Factory.LoginSendInfo(0, "登录成功");

                        process.send({ eventName: "Login", dataString: JSON.stringify(loginSendInfo), sid: sid});

                    } else {

                        console.log(username, "密码错误");

                        let loginSendInfo = new Factory.LoginSendInfo(1, "密码错误");

                        process.send({ eventName: "Login", dataString: JSON.stringify(loginSendInfo), sid: sid});

                    }
                    
                }
            });
        })
    }

    // 显示服务器列表
    showServer(dataString, sid) {

        process.nextTick(() => {

            let serverSendInfo = new Factory.ServerSendInfo();

            process.send({ eventName: "ShowServer", dataString: JSON.stringify(serverSendInfo), sid: sid});
                    
        })
    }

    // 显示角色
    showRole(dataString, sid) {

        process.nextTick(() => {

            let serverId = parseInt(dataString);

            let username = HALL_GD.ONLINE_ACCOUNT[sid];

            let findObj = {inServerId : serverId, username : username};

            mongoDB.find("role", findObj, (err, result) => {

                let roleListSendInfo = new Factory.RoleListSendInfo();

                roleListSendInfo.roleJson = {};

                for (let i = 0; i < result.length; i += 1) {

                    let roleState = result[i];

                    delete roleState["_id"];

                    let roleName = roleState.roleName;

                    roleListSendInfo.roleJson[roleName] = roleState;

                }

                process.send({ eventName: "ShowRole", dataString: roleListSendInfo, sid: sid});

            });
        })
    }

    // 创建角色
    createRole(dataString, sid) {

        process.nextTick(() => {

            let roleInfo = JSON.parse(dataString);

            let roleName = roleInfo.roleName;

            let serverId = roleInfo.serverId;

            let username = HALL_GD.ONLINE_ACCOUNT[sid];

            let roleState = new Factory.RoleState();

            roleState.roleName = roleName;

            roleState.username = username;

            roleState.inServerId = serverId;

            let findObj = {roleName : roleName};

            mongoDB.findOne("role", findObj, (err, result) => {

                if (result) {

                    process.send({ eventName: "CreateRole", dataString: "", sid: sid});

                } else {

                    mongoDB.insertOne("role", roleState, (err, result) => {

                        process.send({ eventName: "CreateRole", dataString: JSON.stringify(roleState), sid: sid});
            
                    });
                }
            })
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

            let username = HALL_GD.ONLINE_ACCOUNT[sid];

            HALL_GD.ONLINE_ROLE[username] = roleState;

            process.send({ eventName: "EnterGame", dataString: "", sid: sid});
                    
        })
    }

    // TODO : 显示自定义房间
    showRoom(dataString, sid) {

    }

    // 开始匹配 申请GD
    requestHallGD(dataString, sid) {

        let modeCode = dataString;

        let username = HALL_GD.ONLINE_ACCOUNT[sid];

        HALL_GD.ONLINE_ROLE[username].inComparingMode = parseInt(modeCode); // 角色状态设为正在匹配中

        process.send({ eventName: "RequestHallGD", dataString: {roleState: HALL_GD.ONLINE_ROLE[username]}, sid: sid });

    }

    // 匹配成功 主线程请求角色数组
    // dataString = sidJson 键值对为 sid : {}
    requestRoleState(dataString, sid) {

        let sidJson = dataString;

        for (let sid in sidJson) {

            let username = HALL_GD.ONLINE_ACCOUNT[sid];

            let role = HALL_GD.ONLINE_ROLE[username];

            role.sid = sid;

            sidJson[sid] = role;

        }

        process.send({ eventName: "RequestRoleState", dataString: sidJson, sid: "" });

    }
}

module.exports = HallApp;