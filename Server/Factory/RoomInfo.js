
class RoomInfo {

    constructor(roomName) {

        this.roomName = (roomName === undefined) ? this.roomId.toString() + "号房间" : roomName;

        this.leftAllyList = [];

        this.rightAllyList = [];

        RoomInfo.prototype.roomId += 1;
    }
}

RoomInfo.prototype.roomId = 0;

module.exports = RoomInfo;