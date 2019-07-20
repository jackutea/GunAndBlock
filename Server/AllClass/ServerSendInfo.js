var HALL_GD = require("../GlobalData/HALL_GD");

class ServerSendInfo {

    constructor() {

        this.serverIdList = [];

        this.serverUserCountList = [];

        this.loadServerList();

    }

    loadServerList() {

        for (let serverId in HALL_GD.SERVER_LIST) {

            let serverInfo = HALL_GD.SERVER_LIST[serverId];

            this.serverIdList.push(serverInfo.serverId);

            this.serverUserCountList.push(serverInfo.getOnlineUserCount());

        }
    }

}

module.exports = ServerSendInfo;