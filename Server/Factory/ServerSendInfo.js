var GD = require("../Datas/GD");

class ServerSendInfo {

    constructor() {

        this.serverIdList = [];

        this.serverUserCountList = [];

        this.loadServerList();

    }

    loadServerList() {

        for (let serverId in GD.SERVER_LIST) {

            let serverInfo = GD.SERVER_LIST[serverId];

            this.serverIdList.push(serverInfo.serverId);

            this.serverUserCountList.push(serverInfo.getOnlineUserCount());

        }
    }

}

module.exports = ServerSendInfo;