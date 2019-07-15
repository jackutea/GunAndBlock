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

                let obj = {username : username, password : password};

                mongoDB.insertOne("account", obj, (err, result) => {

                    let roleState = new ClassFactory.RoleState();

                    roleState.username = username;

                    roleState.ip = remoteIpString;

                    roleState.port = remotePort;

                    GD.ONLINE_USERS[remoteIpString] = roleState; // 添加玩家信息

                    mongoDB.insertOne("role", roleState, (err, result) => {

                        let loginSendInfo = new ClassFactory.LoginSendInfo(0, "注册成功，直接登录");

                        this.emitTo("LoginRecv", JSON.stringify(loginSendInfo), remoteIpString, remotePort);

                    });
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

                    mongoDB.findOne("role", usernameobj, (err, result) => {

                        console.log(result);

                        let roleState = result;

                        GD.ONLINE_USERS[remoteIpString] = roleState; // 添加玩家信息

                        let loginSendInfo = new ClassFactory.LoginSendInfo(0, "登录成功");

                        this.emitTo("LoginRecv", JSON.stringify(loginSendInfo), remoteIpString);

                    });

                } else {

                    console.log(username, "密码错误");

                    let loginSendInfo = new ClassFactory.LoginSendInfo(1, "密码错误");

                    this.emitTo("LoginRecv", JSON.stringify(loginSendInfo), remoteIpString);

                }
                
            }
        });
    },

    showRoom : function(dataString, remoteIpString, remotePort) {

        let serverId = parseInt(dataString);

        let roomSendInfo = new ClassFactory.RoomSendInfo(serverId);

        this.emitTo("ShowRoomRecv", JSON.stringify(roomSendInfo), remoteIpString);

    }
}