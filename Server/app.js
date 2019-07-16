var CuteUDP = require("./CuteUDP/CuteUDP");
var cuteEvent = require("./cuteEvent");
var GD = require("./Datas/GD");
var ClassFactory = require("./Factory/ClassFactory");

var cuteUDP = new CuteUDP("127.0.0.1", 9999, 10000);

// TODO :
// 1. 匹配系统
// 2. 人物反馈（血条反馈、攻击反馈、格挡反馈、完美格挡反馈）
// 3. Ping功能
// 4. 水晶

cuteUDP.on("Login", cuteEvent.login);

cuteUDP.on("Register", cuteEvent.register);

cuteUDP.on("ShowRole", cuteEvent.showRole);

cuteUDP.on("CreateRole", cuteEvent.createRole);

cuteUDP.on("DeleteRole", cuteEvent.deleteRole);

cuteUDP.on("EnterGame", cuteEvent.enterGame);

cuteUDP.on("ShowServer", cuteEvent.showServer);

cuteUDP.on("ShowRoom", cuteEvent.showRoom);



// 开服 1 个
for (let i = 0; i < 1; i += 1) {

    let serverInfo = new ClassFactory.ServerInfo();

    // 开官方房间 每服务器0个
    for (let o = 0; o < 0; o += 1) {

        let roomInfo = new ClassFactory.RoomInfo();

        serverInfo.roomJson[roomInfo.roomId] = roomInfo;

    }

    GD.SERVER_LIST[serverInfo.serverId] = serverInfo;
    
}

// console.log("开服情况" + GD.SERVER_LIST);