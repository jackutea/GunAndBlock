var GD = require("../GlobalData/HALL_GD");

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

module.exports = RoomSendInfo;