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

        process.on("message", (msg) => {

            let eventName = msg.eventName;

            let dataString = msg.dataString;

            let sid = msg.sid;

            this.emit(eventName, dataString, sid);

            // console.log("Hall App Recv : ", msg);

        });

    }

    // 初始化全局变量
    initHallGD(dataString, sid) {

        HALL_GD.CreateGD(() => {

            process.send({ eventName: "InitCompareGD", dataString: HALL_GD.SERVER_LIST, sid: sid});

            // process.send({ eventName: "InitBattleGD", dataString: HALL_GD, sid: sid});

        });

    }

    requireHallGD(dataString, sid) {

        process.send({ eventName: "RequireHallGD", dataString: {ONLINE_ACCOUNT: HALL_GD.ONLINE_ACCOUNT, ONLINE_ROLE: HALL_GD.ONLINE_ROLE}, sid: sid });

    }

    // 初始化进程监听事件
    initHallClusterListener() {

        // 处理非UDP
        this.on("InitHallGD", this.initHallGD);

        this.on("RequireHallGD", this.requireHallGD);

        // 处理UDP
        this.on("Register", this.register); // 注册

        this.on("Login", this.login); // 登录

        this.on("ShowServer", this.showServer); // 显示服务器列表

        this.on("ShowRole", this.showRole); // 显示角色列表

        this.on("CreateRole", this.createRole); // 创建角色

        this.on("DeleteRole", this.deleteRole); // 删除角色

        this.on("EnterGame", this.enterGame); // 选定角色后进入游戏

        this.on("ShowRoom", this.showRoom); // 显示服务器自定义房间

    }

    // 注册
    register(dataString, sid) {

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
    }

    // 登录
    login(dataString, sid) {

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
    }

    // 显示服务器列表
    showServer(dataString, sid) {

        let serverSendInfo = new Factory.ServerSendInfo();

        process.send({ eventName: "ShowServer", dataString: JSON.stringify(serverSendInfo), sid: sid});

    }

    // 显示角色
    showRole(dataString, sid) {

        let serverId = parseInt(dataString);

        let username = HALL_GD.ONLINE_ACCOUNT[sid];

        let findObj = {inServerId : serverId, username : username};

        mongoDB.find("role", findObj, (err, result) => {

            let roleList = new Factory.RoleListSendInfo();

            for (let i = 0; i < result.length; i += 1) {

                let roleState = result[i];

                delete roleState["_id"];

                roleList.roles.push(roleState);

            }

            process.send({ eventName: "ShowRole", dataString: JSON.stringify(roleList), sid: sid});

        });
    }

    // 创建角色
    createRole(dataString, sid) {

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
    }

    // 删除角色
    deleteRole(dataString, sid) {

        let roleName = dataString;

        let delObj = {roleName : roleName};

        mongoDB.deleteOne("role", delObj, (err, result) => {

            process.send({ eventName: "DeleteRole", dataString: "", sid: sid});

        });

    }

    // 进入游戏
    enterGame(dataString, sid) {

        let roleState = JSON.parse(dataString); // TODO : 这里有可能被利用

        let username = HALL_GD.ONLINE_ACCOUNT[sid];

        HALL_GD.ONLINE_ROLE[username] = roleState;

        process.send({ eventName: "EnterGame", dataString: "", sid: sid});

    }

    // TODO : 显示自定义房间
    showRoom(dataString, sid) {

    }
}

module.exports = HallApp;