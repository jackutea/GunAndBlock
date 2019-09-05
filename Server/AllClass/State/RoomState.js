class RoomState {

    constructor(roomName) {

        this.roomId = RoomState.count;

        this.roomName = (roomName === undefined) ? this.roomId.toString() + "号房间" : roomName;

        this.isPrivate = false;

        this.leftAllyList = [];

        this.rightAllyList = [];

        RoomState.count += 1;
    }
}

RoomState.count = 0;

module.exports = RoomState;