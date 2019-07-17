var ClassFactory = require("./Factory/ClassFactory");
var mongoDB = require("./mongoDB/mongoDB");

var GD = require("./Datas/GD");

module.exports = {

    // 注册
    register: function(dataString, remoteIpString, remotePort) {

        let UserInfo = JSON.parse(dataString);

        let username = UserInfo.username;

        let password = UserInfo.password;

        let usernameobj = {username : username};

        mongoDB.findOne("account", usernameobj, (err, result) => {

            if (err) throw err;

            if (!result) {

                console.log(username, "用户名不存在，可注册");

                let accountState = new ClassFactory.AccountState();

                accountState.username = username;

                accountState.password = password;

                GD.ONLINE_ACCOUNT[remoteIpString] = username; // 添加玩家信息

                mongoDB.insertOne("account", accountState, (err, result) => {

                    let loginSendInfo = new ClassFactory.LoginSendInfo(0, "注册成功，直接登录");

                    this.emitTo("LoginRecv", JSON.stringify(loginSendInfo), remoteIpString, remotePort);

                });

            } else {

                let loginSendInfo = new ClassFactory.LoginSendInfo(3, username + "已存在，无法注册");

                this.emitTo("LoginRecv", JSON.stringify(loginSendInfo), remoteIpString);

            }
        });
    },

    // 传来登录申请 dataString 类型为 UserInfo
    login : function (dataString, remoteIpString, remotePort) {

        let UserInfo = JSON.parse(dataString);

        let username = UserInfo.username;

        let password = UserInfo.password;

        let usernameobj = {username : username};

        mongoDB.findOne("account", usernameobj, (err, result) => {

            if (err) throw err;

            if (!result) {

                console.log(username, "用户名不存在");

                let loginSendInfo = new ClassFactory.LoginSendInfo(2, "用户名不存在");

                this.emitTo("LoginRecv", JSON.stringify(loginSendInfo), remoteIpString);

            } else {

                if (result.password == password) {

                    // console.log(username, "密码正确，直接登录");

                    GD.ONLINE_ACCOUNT[remoteIpString] = username; // 添加玩家信息

                    let loginSendInfo = new ClassFactory.LoginSendInfo(0, "登录成功");

                    this.emitTo("LoginRecv", JSON.stringify(loginSendInfo), remoteIpString);

                } else {

                    console.log(username, "密码错误");

                    let loginSendInfo = new ClassFactory.LoginSendInfo(1, "密码错误");

                    this.emitTo("LoginRecv", JSON.stringify(loginSendInfo), remoteIpString);

                }
                
            }
        });
    },

    showServer : function(dataString, remoteIpString, remotePort) {

        console.log(dataString);

        let serverSendInfo = new ClassFactory.ServerSendInfo();

        this.emitTo("ShowServerRecv", JSON.stringify(serverSendInfo), remoteIpString);

    },

    showRole : function(dataString, remoteIpString, remotePort) {

        let serverId = parseInt(dataString);

        let username = GD.ONLINE_ACCOUNT[remoteIpString];

        let findObj = {inServerId : serverId, username : username};

        mongoDB.find("role", findObj, (err, result) => {

            let roleList = new ClassFactory.RoleListSendInfo();

            for (let i = 0; i < result.length; i += 1) {

                let roleState = result[i];

                delete roleState["_id"];

                roleList.roles.push(roleState);

            }

            this.emitTo("ShowRoleRecv", JSON.stringify(roleList), remoteIpString);

        });
    },

    createRole : function(dataString, remoteIpString, remotePort) {

        let roleInfo = JSON.parse(dataString);

        let roleName = roleInfo.roleName;

        let serverId = roleInfo.serverId;

        let username = GD.ONLINE_ACCOUNT[remoteIpString];

        let roleState = new ClassFactory.RoleState();

        roleState.roleName = roleName;

        roleState.username = username;

        roleState.inServerId = serverId;

        let findObj = {roleName : roleName};

        mongoDB.findOne("role", findObj, (err, result) => {

            if (result) {

                this.emitTo("CreateRoleFailRecv", "角色名已存在", remoteIpString);

            } else {

                mongoDB.insertOne("role", roleState, (err, result) => {

                    this.emitTo("CreateRoleRecv", JSON.stringify(roleState), remoteIpString);
        
                });
            }
        })
    },

    deleteRole : function(dataString, remoteIpString, remotePort) {

        let roleName = dataString;

        let delObj = {roleName : roleName};

        mongoDB.deleteOne("role", delObj, (err, result) => {

            this.emitTo("DeleteRoleRecv", "", remoteIpString);

        });

    },

    enterGame : function(dataString, remoteIpString, remotePort) {

        let roleState = JSON.parse(dataString); // TODO : 这里有可能被利用

        let username = GD.ONLINE_ACCOUNT[remoteIpString];

        GD.ONLINE_ROLE[username] = roleState;

        this.emitTo("EnterGameRecv", "", remoteIpString);

    },

    showRoom : function(dataString, remoteIpString, remotePort) {

        let serverId = parseInt(dataString);

        let roomSendInfo = new ClassFactory.RoomSendInfo(serverId);

        this.emitTo("ShowRoomRecv", JSON.stringify(roomSendInfo), remoteIpString);

    },

    compare : function(dataString, remoteIpString, remotePort) {

        // dataString = int code
        // code 1 : 1V1 / code 5 : 5V5 / code 50 : 50V50
    }
}