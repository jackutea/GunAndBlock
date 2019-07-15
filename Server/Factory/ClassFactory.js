var GD = require("../Datas/GD");

class LoginSendInfo {

    constructor(stateCode, msg) {

        // 0 登录正确 1 登录密码错 2 登录用户名错 3 注册用户名被占用
        this.stateCode = stateCode;

        this.msg = msg;

        this.serverIdList = [];

        this.serverUserCountList = [];

        this.loadServerList(stateCode);

    }

    loadServerList(stateCode) {

        if (stateCode !== 0) return;

        for (let serverId in GD.SERVER_LIST) {

            let serverInfo = GD.SERVER_LIST[serverId];

            this.serverIdList.push(serverInfo.serverId);

            this.serverUserCountList.push(serverInfo.getOnlineUserCount());

        }
    }
}

class RoomInfo {

    constructor(roomName) {

        this.roomId = RoomInfo.prototype.count;

        this.roomName = (roomName === undefined) ? this.roomId.toString() + "号房间" : roomName;

        this.isPrivate = false;

        this.leftAllyList = [];

        this.rightAllyList = [];

        RoomInfo.prototype.count += 1;
    }
}

RoomInfo.prototype.count = 0;

class RoomSendInfo {

    constructor(serverId) {

        this.roomIdList = [];

        this.roomNameList = [];

        this.roomIsPrivateList = [];

        this.leftAllyCountList = [];

        this.rightAllyCountList = [];

        this.loadServerRoom(serverId);

    }

    loadServerRoom(serverId) {

        if (serverId === undefined) return;

        let serverInfo = GD.SERVER_LIST[serverId];

        for (let key in serverInfo.roomJson) {

            let roomInfo = serverInfo.roomJson[key];

            this.roomIdList.push(roomInfo.roomId);

            this.roomNameList.push(roomInfo.roomName);

            this.roomIsPrivateList.push(roomInfo.isPrivate);

            this.leftAllyCountList.push(roomInfo.leftAllyList.length);

            this.rightAllyCountList.push(roomInfo.rightAllyList.length);

        }
    }
}



class ServerInfo {

    constructor(serverName) {

        this.serverId = ServerInfo.prototype.count;

        this.serverName = (serverName === undefined) ? this.serverId.toString() + "区" : serverName;
        
        this.onlineUserCount = 0;

        this.roomJson = {};

        ServerInfo.prototype.count += 1;
    }

    getOnlineUserCount() {

        let num = 0;

        for (let i in this.roomJson) {

            num += this.roomJson[i].leftAllyList.length;

            num += this.roomJson[i].rightAllyList.length;

        }

        this.onlineUserCount = num;

        return num; // 该服务器所有人数
    }
}

ServerInfo.prototype.count = 0;

class UserState {

    constructor() {

        this.username;

        this.ip;

        this.port;

    }
}

class RoleState extends UserState {
    
    constructor() {

        super();

        this.roleName = "";

        this.level = 1;

        this.exp = 0;

        this.score = 0;

        this.isLeftAlly = true;
        
        this.life = 5.0;
        
        this.blockLife = 3.0;
        
        this.damage = 1.0;
        
        this.shootGapOrigin = 1.0;
        
        this.shootGap = 0.0;
        
        this.blockGapOrigin = 0.5;
        
        this.blockGap = 0.0;
        
        this.perfectBlockGapOrigin = 0.2;
        
        this.perfectBlockGap = 0.0;
        
        this.moveSpeedOrigin = 2.0;
        
        this.moveSpeed = 2.0;
        
        this.shootSpeed = 4.0;
        
        this.isBlocking = false;
        
        this.isPerfectBlocking = false;
        
        this.isMoving = false;
        
        this.isReloading = false;
        
        this.isDead = false;

    }
}


// Exports
exports.LoginSendInfo = LoginSendInfo;

exports.RoomInfo = RoomInfo;

exports.RoomSendInfo = RoomSendInfo;

exports.ServerInfo = ServerInfo;

exports.UserState = UserState;

exports.RoleState = RoleState;
