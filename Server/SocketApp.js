var event = require("events");
var cluster = require("cluster");

var CuteUDP = require("./CuteUDP/CuteUDP");

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

        // Hall Cluster 处理 UDP
        this.cuteUDP.on("Register", this.registerHallSend); // 注册

        this.cuteUDP.on("Login", this.loginHallSend); // 登录

        this.cuteUDP.on("ShowRole", this.showRoleHallSend); // 显示角色列表

        this.cuteUDP.on("CreateRole", this.createRoleHallSend); // 创建角色

        this.cuteUDP.on("DeleteRole", this.deleteRoleHallSend); // 删除角色

        this.cuteUDP.on("EnterGame", this.enterGameHallSend); // 选定角色后进入游戏

        this.cuteUDP.on("ShowServer", this.showServerHallSend); // 显示服务器列表

        this.cuteUDP.on("ShowRoom", this.showRoomHallSend); // 显示服务器自定义房间

        // Compare Cluster 负责处理        
        this.cuteUDP.on("Compare", this.compareCompareSend); // 匹配

    }

    // 初始化主进程监听事件
    initMainClusterListener() {

        // Hall Cluster 处理 UDP 完回传
        this.on("Register", this.registerHallBack); // 注册

        this.on("Login", this.loginHallBack); // 登录

        this.on("ShowRole", this.showRoleHallBack); // 显示角色列表

        this.on("CreateRole", this.createRoleHallBack); // 创建角色

        this.on("DeleteRole", this.deleteRoleHallBack); // 删除角色

        this.on("EnterGame", this.enterGameHallBack); // 选定角色后进入游戏

        this.on("ShowServer", this.showServerHallBack); // 显示服务器列表

        this.on("ShowRoom", this.showRoomHallBack); // 显示服务器自定义房间

        // Compare Cluster 处理完回传
        this.on("Compare", this.compareCompareBack); //匹配

    }

    // 注册 —— hall send
    // dataString = C# class UserSendInfo
    registerHallSend(dataString, sid) {

        SocketApp.hallCluster.send({eventName : "Register", dataString : dataString, sid : sid});

    }

    // 注册 —— hall back
    // dataString = js class LoginSendInfo
    registerHallBack(dataString, sid) {

        this.cuteUDP.emitBackTo("LoginRecv", dataString, sid);
        
    }

    // 登录 —— hall send
    // dataString = C# class UserSendInfo
    loginHallSend(dataString, sid) {

        console.log("发送登录")

        SocketApp.hallCluster.send({ eventName: "Login", dataString: dataString, sid: sid });

    }

    // 登录 —— hall back
    // dataString = js class LoginSendInfo
    loginHallBack(dataString, sid) {

        console.log("回传登录")

        this.cuteUDP.emitBackTo("LoginRecv", dataString, sid);

    }

    // 显示服务器 —— hall send
    // dataString = ""
    showServerHallSend(dataString, sid) {

        SocketApp.hallCluster.send({ eventName: "ShowServer", dataString: dataString, sid: sid })

    }

    // 显示服务器 —— hall back
    // dataString = js class ServerSendInfo
    showServerHallBack(dataString, sid) {

        this.cuteUDP.emitBackTo("ShowServerRecv", dataString, sid);

    }

    // 显示角色 —— hall send
    // dataString = int serverId
    showRoleHallSend(dataString, sid) {

        SocketApp.hallCluster.send({ eventName: "ShowRole", dataString: dataString, sid: sid })
        
    }

    // 显示角色 —— hall back
    // dataString = js class RoleListSendInfo
    showRoleHallBack(dataString, sid) {

        this.cuteUDP.emitBackTo("ShowRoleRecv", dataString, sid);

    }

    // 创建角色 —— hall send
    // dataString = C# class CreateRoleSendInfo
    createRoleHallSend(dataString, sid) {

        SocketApp.hallCluster.send({ eventName: "CreateRole", dataString: dataString, sid: sid })
        
    }

    // 创建角色 —— hall back
    // dataString = "" 创建失败
    // dataString = class RoleState 创建成功
    createRoleHallBack(dataString, sid) {

        if (dataString.length < 2) {

            this.cuteUDP.emitBackTo("CreateRoleFailRecv", dataString, sid);

        } else {

            this.cuteUDP.emitBackTo("CreateRoleRecv", dataString, sid);

        }
    }

    // 删除角色 —— hall send
    // dataString = roleName
    deleteRoleHallSend(dataString, sid) {

        SocketApp.hallCluster.send({ eventName: "DeleteRole", dataString: dataString, sid: sid })

    }

    // 删除角色 —— hall back
    // dataString = ""
    deleteRoleHallBack(dataString, sid) {

        this.cuteUDP.emitBackTo("DeleteRoleRecv", dataString, sid);

    }

    // 进入游戏 —— hall send
    // dataString = class RoleState
    enterGameHallSend(dataString, sid) {

        SocketApp.hallCluster.send({ eventName: "EnterGame", dataString: dataString, sid: sid })

    }

    // 进入游戏 —— hall back
    // dataString = ""
    enterGameHallBack(dataString, sid) {

        this.cuteUDP.emitBackTo("EnterGameRecv", dataString, sid);

    }

    // TODO : 显示自定义房间 —— hall send 
    showRoomHallSend(dataString, sid) {

        let serverId = parseInt(dataString);

        let roomSendInfo = new Factory.RoomSendInfo(serverId);

        this.cuteUDP.emitBackTo("ShowRoomRecv", JSON.stringify(roomSendInfo), sid);

    }

    // TODO : 显示自定义房间 —— hall back
    showRoomHallBack(dataString, sid) {

    }

    // 开始匹配 —— compare send
    // dataString = int modeCode
    // 只负责添加到匹配列表，之后由服务器匹配心跳去处理
    compareCompareSend(dataString, sid) {

        SocketApp.hallCluster.send({ eventName: "RequireHallGD", dataString: "", sid: sid});

        SocketApp.instance.on("RequireHallGD", (data, sid) => {
            
            let ONLINE_ACCOUNT = data.ONLINE_ACCOUNT;

            let ONLINE_ROLE = data.ONLINE_ROLE;

            SocketApp.compareCluster.send({ eventName: "PreCompare", dataString: {ONLINE_ACCOUNT: ONLINE_ACCOUNT, ONLINE_ROLE: ONLINE_ROLE, dataString : dataString}, sid: sid })

        });
    }

    // 开始匹配 —— compare back
    // dataString = ""
    compareCompareBack(dataString, sid) {

        this.cuteUDP.emitBackTo("CompareWaitRecv", dataString, sid);

    }
}

module.exports = SocketApp;