var DefaultLevel = require("./Config/DefaultLevel");
var ServerInfo = require("../AllClass/ServerInfo");

class HALL_GD {}

// 静态属性
HALL_GD.ONLINE_ACCOUNT = {}; // 在线账号 sid : AccountState
HALL_GD.ONLINE_ROLE = {}; // 在线角色 AccountState.username : RoleState
HALL_GD.SERVER_LIST = {}; // 服务器列表 serverId : ServerInfo

HALL_GD.DEFAULT_LEVEL = DefaultLevel; // 等级:经验 / 天梯:积分 

// 静态方法
HALL_GD.InitServerList = function(count) {

    for (let i = 0; i < count; i += 1) {

        let serverInfo = new ServerInfo(i);

        HALL_GD.SERVER_LIST[serverInfo.serverId] = serverInfo;
        
    }
}

HALL_GD.CreateGD = function(callback) {

    HALL_GD.InitServerList(1);

    callback(0);
    
}

module.exports = HALL_GD;