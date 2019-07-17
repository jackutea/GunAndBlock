var ClassFactory = require("./Factory/ClassFactory");
var mongoDB = require("./mongoDB/mongoDB");

var GD = require("./Datas/GD");

module.exports = {

    // 注册
    register: function(dataString, sid) {

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

                GD.ONLINE_ACCOUNT[sid] = username; // 添加玩家信息

                mongoDB.insertOne("account", accountState, (err, result) => {

                    let loginSendInfo = new ClassFactory.LoginSendInfo(0, "注册成功，直接登录");

                    this.emitBackTo("LoginRecv", JSON.stringify(loginSendInfo), sid);

                });

            } else {

                let loginSendInfo = new ClassFactory.LoginSendInfo(3, username + "已存在，无法注册");

                this.emitBackTo("LoginRecv", JSON.stringify(loginSendInfo), sid);

            }
        });
    },

    // 传来登录申请 dataString 类型为 UserInfo
    login : function (dataString, sid) {

        let UserInfo = JSON.parse(dataString);

        let username = UserInfo.username;

        let password = UserInfo.password;

        let usernameobj = {username : username};

        mongoDB.findOne("account", usernameobj, (err, result) => {

            if (err) throw err;

            if (!result) {

                console.log(username, "用户名不存在");

                let loginSendInfo = new ClassFactory.LoginSendInfo(2, "用户名不存在");

                this.emitBackTo("LoginRecv", JSON.stringify(loginSendInfo), sid);

            } else {

                if (result.password == password) {

                    // console.log(username, "密码正确，直接登录");

                    GD.ONLINE_ACCOUNT[sid] = username; // 添加玩家信息

                    let loginSendInfo = new ClassFactory.LoginSendInfo(0, "登录成功");

                    this.emitBackTo("LoginRecv", JSON.stringify(loginSendInfo), sid);

                } else {

                    console.log(username, "密码错误");

                    let loginSendInfo = new ClassFactory.LoginSendInfo(1, "密码错误");

                    this.emitBackTo("LoginRecv", JSON.stringify(loginSendInfo), sid);

                }
                
            }
        });
    },

    showServer : function(dataString, sid) {

        console.log(dataString);

        let serverSendInfo = new ClassFactory.ServerSendInfo();

        this.emitBackTo("ShowServerRecv", JSON.stringify(serverSendInfo), sid);

    },

    showRole : function(dataString, sid) {

        let serverId = parseInt(dataString);

        let username = GD.ONLINE_ACCOUNT[sid];

        let findObj = {inServerId : serverId, username : username};

        mongoDB.find("role", findObj, (err, result) => {

            let roleList = new ClassFactory.RoleListSendInfo();

            for (let i = 0; i < result.length; i += 1) {

                let roleState = result[i];

                delete roleState["_id"];

                roleList.roles.push(roleState);

            }

            this.emitBackTo("ShowRoleRecv", JSON.stringify(roleList), sid);

        });
    },

    createRole : function(dataString, sid) {

        let roleInfo = JSON.parse(dataString);

        let roleName = roleInfo.roleName;

        let serverId = roleInfo.serverId;

        let username = GD.ONLINE_ACCOUNT[sid];

        let roleState = new ClassFactory.RoleState();

        roleState.roleName = roleName;

        roleState.username = username;

        roleState.inServerId = serverId;

        let findObj = {roleName : roleName};

        mongoDB.findOne("role", findObj, (err, result) => {

            if (result) {

                this.emitBackTo("CreateRoleFailRecv", "角色名已存在", sid);

            } else {

                mongoDB.insertOne("role", roleState, (err, result) => {

                    this.emitBackTo("CreateRoleRecv", JSON.stringify(roleState), sid);
        
                });
            }
        })
    },

    deleteRole : function(dataString, sid) {

        let roleName = dataString;

        let delObj = {roleName : roleName};

        mongoDB.deleteOne("role", delObj, (err, result) => {

            this.emitBackTo("DeleteRoleRecv", "", sid);

        });

    },

    enterGame : function(dataString, sid) {

        let roleState = JSON.parse(dataString); // TODO : 这里有可能被利用

        let username = GD.ONLINE_ACCOUNT[sid];

        GD.ONLINE_ROLE[username] = roleState;

        this.emitBackTo("EnterGameRecv", "", sid);

    },

    showRoom : function(dataString, sid) {

        let serverId = parseInt(dataString);

        let roomSendInfo = new ClassFactory.RoomSendInfo(serverId);

        this.emitBackTo("ShowRoomRecv", JSON.stringify(roomSendInfo), sid);

    },

    compare : function(dataString, sid) {

        // dataString = int code
        // code 1 : 1V1 / code 5 : 5V5 / code 50 : 50V50
    }
}