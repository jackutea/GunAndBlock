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

module.exports = RoomInfo;