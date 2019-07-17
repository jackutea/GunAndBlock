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

cuteUDP.on("Login", cuteEvent.login); // 登录

cuteUDP.on("Register", cuteEvent.register); // 注册

cuteUDP.on("ShowRole", cuteEvent.showRole); // 显示角色列表

cuteUDP.on("CreateRole", cuteEvent.createRole); // 创建角色

cuteUDP.on("DeleteRole", cuteEvent.deleteRole); // 删除角色

cuteUDP.on("EnterGame", cuteEvent.enterGame); // 选定角色后进入游戏

cuteUDP.on("ShowServer", cuteEvent.showServer); // 显示服务器列表

cuteUDP.on("ShowRoom", cuteEvent.showRoom); // 显示服务器自定义房间

cuteUDP.on("Compare", cuteEvent.compare); // 匹配

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