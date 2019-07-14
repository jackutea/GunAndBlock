var CuteUDP = require("./CuteUDP/CuteUDP");
var cuteEvent = require("./cuteEvent");
var GD = require("./Datas/GD");
var ServerInfo = require("./Factory/ServerInfo");
var RoomInfo = require("./Factory/RoomInfo");

var cuteUDP = new CuteUDP("127.0.0.1", 9999, 11000);

// console.log(cuteUDP);

cuteUDP.on("login", cuteEvent.login);

// 开服 2 个
for (let i = 0; i < 2; i += 1) {

    let serverInfo = new ServerInfo();

    // 开官方房间 每服务器3个
    for (let o = 0; o < 3; o += 1) {

        let roomInfo = new RoomInfo();

        serverInfo.roomJson[roomInfo.roomId] = roomInfo;

    }

    GD.SERVER_LIST[serverInfo.serverId] = serverInfo;
    
}

// console.log("开服情况" + GD.SERVER_LIST);