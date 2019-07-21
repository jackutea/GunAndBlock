var event = require("events");
var cluster = require("cluster");

var CuteUDP = require("./CuteUDP/CuteUDP");

var MoveInfo = require("./AllClass/MoveInfo");

// 主进程，负责 I/O 调度
class SocketApp extends event {

    constructor() {

        super();

        SocketApp.instance = this;

        SocketApp.hallCluster = cluster.workers["1"];

        SocketApp.compareCluster = cluster.workers["2"];

        SocketApp.battleCluster = cluster.workers["3"];

        this.cuteUDP = new CuteUDP("127.0.0.1", 9999, 10000);

        this.initClusterListener();

        this.initCuteUDPListener();

        this.initMainClusterListener();

        this.initGD();

    }

    // 初始化全局变量
    initGD() {

        SocketApp.hallCluster.send({eventName : "InitHallGD", dataString : "", sid : ""});

        this.on("InitCompareGD", (dataString, sid) => {

            SocketApp.compareCluster.send({eventName : "InitCompareGD", dataString : dataString, sid : ""});

        });

        this.on("InitBattleGD", (dataString, sid) => {

            SocketApp.battleCluster.send({eventName : "InitBattleGD", dataString : dataString, sid : ""});

        });
    }

    // 初始化进程监听器
    initClusterListener() {

        cluster.on("message", (worker, msg) => {

            let eventName = msg.eventName;

            let dataString = msg.dataString;

            let sid = msg.sid;

            this.emit(eventName, dataString, sid);

            // console.log("SocketApp Recv : ", msg);

        });

    }

    // 初始化 CuteUDP 监听事件
    initCuteUDPListener() {

        // Hall Cluster 负责处理
        this.cuteUDP.on("Register", this.registerReq); // 注册

        this.cuteUDP.on("Login", this.loginReq); // 登录

        this.cuteUDP.on("ShowRole", this.showRoleReq); // 显示角色列表

        this.cuteUDP.on("CreateRole", this.createRoleReq); // 创建角色

        this.cuteUDP.on("DeleteRole", this.deleteRoleReq); // 删除角色

        this.cuteUDP.on("EnterGame", this.enterGameReq); // 选定角色后进入游戏

        this.cuteUDP.on("ShowServer", this.showServerReq); // 显示服务器列表

        this.cuteUDP.on("ShowRoom", this.showRoomReq); // 显示服务器自定义房间

        // Compare Cluster 负责处理
        this.cuteUDP.on("Compare", this.compareReq); // 匹配申请

        // Battle Cluster 负责处理
        this.cuteUDP.on("BattleMove", this.battleMoveReq); // 移动

    }

    // 初始化主进程监听事件
    initMainClusterListener() {

        this.on("error", (err) => {

            console.log("SocketApp Event Error:", err);

        })

        // Hall Cluster 处理完回传
        this.on("Register", this.registerRes); // 注册

        this.on("Login", this.loginRes); // 登录

        this.on("ShowRole", this.showRoleRes); // 显示角色列表

        this.on("CreateRole", this.createRoleRes); // 创建角色

        this.on("DeleteRole", this.deleteRoleRes); // 删除角色

        this.on("EnterGame", this.enterGameRes); // 选定角色后进入游戏

        this.on("ShowServer", this.showServerRes); // 显示服务器列表

        this.on("ShowRoom", this.showRoomRes); // 显示服务器自定义房间

        // Compare Cluster 处理完回传
        this.on("Compare", this.compareRes); // 匹配申请

        this.on("CompareSuccess", this.compareSucessRes); // 匹配成功

        // Battle Cluster 处理完回传
        this.on("BattleMove", this.battleMoveRes); // 移动数据插入成功

    }

    // 注册 请求
    // dataString = C# class UserSendInfo
    registerReq(dataString, sid) {

        process.nextTick(() => {

            SocketApp.hallCluster.send({eventName : "Register", dataString : dataString, sid : sid});

        })
    }

    // 注册 回应
    // dataString = js class LoginSendInfo
    registerRes(dataString, sid) {

        process.nextTick(() => {

            this.cuteUDP.emitBackTo("LoginRecv", dataString, sid);

        })
    }

    // 登录 请求
    // dataString = C# class UserSendInfo
    loginReq(dataString, sid) {

        process.nextTick(() => {

            console.log("loginReq event:", dataString);

            console.log("发送登录");

            SocketApp.hallCluster.send({ eventName: "Login", dataString: dataString, sid: sid });
            
        })
    }

    // 登录 回应
    // dataString = js class LoginSendInfo
    loginRes(dataString, sid) {

        process.nextTick(() => {

            console.log("回传登录")

            this.cuteUDP.emitBackTo("LoginRecv", dataString, sid);
            
        })
    }

    // 显示服务器 请求
    // dataString = ""
    showServerReq(dataString, sid) {

        process.nextTick(() => {

            SocketApp.hallCluster.send({ eventName: "ShowServer", dataString: dataString, sid: sid })
            
        })
    }

    // 显示服务器 回应
    // dataString = js class ServerSendInfo
    showServerRes(dataString, sid) {

        process.nextTick(() => {

            this.cuteUDP.emitBackTo("ShowServerRecv", dataString, sid);
            
        })
    }

    // 显示角色 请求
    // dataString = int serverId
    showRoleReq(dataString, sid) {

        process.nextTick(() => {

            SocketApp.hallCluster.send({ eventName: "ShowRole", dataString: dataString, sid: sid })
                    
        })
    }

    // 显示角色 回应
    // dataString = js class RoleListSendInfo
    showRoleRes(dataString, sid) {

        process.nextTick(() => {

            this.cuteUDP.emitBackTo("ShowRoleRecv", dataString, sid);
            
        })
    }

    // 创建角色 请求
    // dataString = C# class CreateRoleSendInfo
    createRoleReq(dataString, sid) {

        process.nextTick(() => {

            SocketApp.hallCluster.send({ eventName: "CreateRole", dataString: dataString, sid: sid })
                    
        })
    }

    // 创建角色 回应
    // dataString = "" 创建失败
    // dataString = class RoleState 创建成功
    createRoleRes(dataString, sid) {

        process.nextTick(() => {

            if (dataString.length < 2) {

                this.cuteUDP.emitBackTo("CreateRoleFailRecv", dataString, sid);

            } else {

                this.cuteUDP.emitBackTo("CreateRoleRecv", dataString, sid);

            }
                    
        })
    }

    // 删除角色 请求
    // dataString = roleName
    deleteRoleReq(dataString, sid) {

        process.nextTick(() => {

            SocketApp.hallCluster.send({ eventName: "DeleteRole", dataString: dataString, sid: sid })
            
        })
    }

    // 删除角色 回应
    // dataString = ""
    deleteRoleRes(dataString, sid) {

        process.nextTick(() => {

            this.cuteUDP.emitBackTo("DeleteRoleRecv", dataString, sid);
            
        })
    }

    // 进入游戏 请求
    // dataString = class RoleState
    enterGameReq(dataString, sid) {

        process.nextTick(() => {

            SocketApp.hallCluster.send({ eventName: "EnterGame", dataString: dataString, sid: sid })
            
        })
    }

    // 进入游戏 回应
    // dataString = ""
    enterGameRes(dataString, sid) {

        process.nextTick(() => {

            this.cuteUDP.emitBackTo("EnterGameRecv", dataString, sid);
            
        })
    }

    // TODO : 显示自定义房间 请求 
    showRoomReq(dataString, sid) {

        process.nextTick(() => {

            let serverId = parseInt(dataString);

            let roomSendInfo = new Factory.RoomSendInfo(serverId);

            this.cuteUDP.emitBackTo("ShowRoomRecv", JSON.stringify(roomSendInfo), sid);
            
        })
    }

    // TODO : 显示自定义房间 回应
    showRoomRes(dataString, sid) {

    }

    // 开始匹配 请求
    // dataString = int modeCode
    // 只负责添加到匹配列表，之后由服务器匹配心跳去处理
    compareReq(dataString, sid) {

        process.nextTick(() => {

            let modeCode = dataString;

            SocketApp.hallCluster.send({ eventName: "RequestHallGD", dataString: modeCode, sid: sid});

            SocketApp.instance.on("RequestHallGD", (data, sid) => {
                
                let roleState = data.roleState;

                SocketApp.compareCluster.send({ eventName: "PreCompare", dataString: {roleState: roleState, dataString : dataString}, sid: sid })

            });
        })
    }

    // 开始匹配 回应
    // dataString = modeCode
    compareRes(dataString, sid) {

        process.nextTick(() => {

            this.cuteUDP.emitBackTo("CompareWaitRecv", dataString, sid);
            
        })
    }

    // 匹配成功 回应
    // 向HALL请求玩家数据，成功后向BATTLE注入战斗数据，注入成功后，推送给客户端
    compareSucessRes(dataString, sid) {

        process.nextTick(() => {

            let sidArray = dataString; // 匹配成功的玩家数组 array[sid...]

            SocketApp.hallCluster.send({ eventName: "RequestRoleState", dataString: sidArray, sid: ""});

            this.on("RequestRoleState", (_roleArray, nothing) => {

                SocketApp.battleCluster.send({ eventName: "BattleLoadField", dataString: _roleArray, sid: ""});

            })

            this.on("BattleLoadField", (_fieldInfo, nothing) => {

                // console.log("收到 fieldInfo:", _fieldInfo);

                let fieldInfo = _fieldInfo;

                SocketApp.instance.cuteUDP.emitBrocast("CompareSuccessRecv", JSON.stringify(fieldInfo), sidArray, true);

            });
        })
    }

    // 玩家移动 请求
    // dataString = MoveInfo
    battleMoveReq(dataString, sid) {

        process.nextTick(() => {

            let moveInfo = dataString;

            SocketApp.battleCluster.send({ eventName: "BattleMove", dataString: moveInfo, sid: sid});
                    
        })
    }

    // 玩家移动 回应
    // dataString = fieldInfo
    battleMoveRes(dataString, sid) {

        process.nextTick(() => {

            let fieldInfo = JSON.parse(dataString);

            let roleArray = fieldInfo.roleArray;

            let sidArray = [];

            let moveInfo = new MoveInfo();

            for (let i in roleArray) {

                let role = roleArray[i];

                let _sid = role.sid;

                sidArray.push(_sid);

                if (_sid == sid) {

                    moveInfo.d = _sid;

                    moveInfo.v = role.vecArray;

                    moveInfo.i = role.inRoleArrayIndex;
                    
                }
            }

            // console.log("广播移动");

            // console.log("服务器Sid", CuteUDP.socketId);

            SocketApp.instance.cuteUDP.emitBrocast("BattleMoveRecv", JSON.stringify(moveInfo), sidArray, sid);
                            
        })
    }
}

module.exports = SocketApp;