
class ServerInfo {

    constructor(serverId, serverName) {

        this.serverId = serverId;

        this.serverName = (serverName === undefined) ? this.serverId.toString() + "区" : serverName;
        
        this.onlineUserCount = 0;

        this.roomJson = {};

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

module.exports = ServerInfo;