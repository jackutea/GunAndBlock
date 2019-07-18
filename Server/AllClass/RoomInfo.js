class RoomInfo {

    constructor(roomName) {

        this.roomId = RoomInfo.count;

        this.roomName = (roomName === undefined) ? this.roomId.toString() + "号房间" : roomName;

        this.isPrivate = false;

        this.leftAllyList = [];

        this.rightAllyList = [];

        RoomInfo.count += 1;
    }
}

RoomInfo.count = 0;

module.exports = RoomInfo;