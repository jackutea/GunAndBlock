
class GD {}

// 静态属性
GD.ONLINE_ACCOUNT = {}; // IP : AccountState
GD.ONLINE_ROLE = {}; // AccountState.username : RoleState
GD.SERVER_LIST = {}; // serverId : ServerInfo

// 静态方法
GD.showAll = function() {
    
    for (let element in GD) {

        if (typeof(GD[element]) == "function") continue;

        console.log(GD[element]);

    }
}

module.exports = GD;