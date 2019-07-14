var LoginInfo = require("./Factory/LoginInfo");
var RoleState = require("./Factory/RoleState");
var mongoDB = require("./mongoDB/mongoDB");

var GD = require("./Datas/GD");

module.exports = {

    // 传来登录申请 dataString 类型为 UserInfo
    login : function (dataString, remoteIpString, remotePort) {

        let UserInfo = JSON.parse(dataString);

        let username = UserInfo.username;

        let password = UserInfo.password;

        let usernameobj = {username : username};

        console.log("login");

        mongoDB.findOne("account", usernameobj, (err, result) => {

            if (err) throw err;

            if (!result) {

                console.log(username, "用户名不存在");

                let loginInfo = new LoginInfo(2, "用户名不存在");

                this.emitTo("loginCheck", JSON.stringify(loginInfo), remoteIpString, remotePort);

                // let obj = {username : username, password : password};

                // mongoDB.insertOne("account", obj, (err, result) => {

                //     let loginInfo = new LoginInfo(0, "注册成功，直接登录");

                //     this.emitTo("loginCheck", JSON.stringify(loginInfo), remoteIpString, remotePort);

                // });

            } else {

                if (result.password == password) {

                    // console.log(username, "已存在，直接登录");

                    let roleState = new RoleState();

                    roleState.username = username;

                    roleState.ip = remoteIpString;

                    roleState.port = remotePort;

                    GD.ONLINE_USERS[remoteIpString] = roleState; // 添加玩家信息

                    let loginInfo = new LoginInfo(0, "登录成功");

                    for (let serverId in GD.SERVER_LIST) {

                        let serverInfo = GD.SERVER_LIST[serverId];

                        loginInfo.serverIdList.push(serverInfo.serverId);

                        loginInfo.serverUserCountList.push(serverInfo.getOnlineUserCount());

                    }

                    this.emitTo("loginCheck", JSON.stringify(loginInfo), remoteIpString, remotePort);

                } else {

                    console.log(username, "密码错误");

                    let loginInfo = new LoginInfo(1, "密码错误");

                    this.emitTo("loginCheck", JSON.stringify(loginInfo), remoteIpString, remotePort);

                }
                
            }
        });
    }
}