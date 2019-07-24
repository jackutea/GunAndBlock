
class ServerInfo {

    constructor(serverId, serverName) {

        this.serverId = serverId;

        this.serverName = (serverName === undefined) ? this.serverId.toString() + "区" : serverName;
        
        this.onlineUserCount = 0;

    }
}

module.exports = ServerInfo;