var LoginInfo = require("./Factory/LoginInfo");
var mongoDB = require("./mongoDB/mongoDB");

module.exports = {

    // 传来登录申请 dataString 类型为 UserInfo
    login : function (dataString, remoteIpString, remotePort) {

        let UserInfo = JSON.parse(dataString);

        let username = UserInfo.username;

        let password = UserInfo.password;

        let usernameobj = {username : username};

        let obj = {username : username, password : password};

        console.log("login");

        mongoDB.findOne("account", usernameobj, (err, result) => {

            if (err) throw err;

            if (!result) {

                // console.log(username, "用户名不存在");

                // let loginInfo = new LoginInfo(2, "用户名不存在");

                // this.emitTo("loginCheck", JSON.stringify(loginInfo), remoteIpString, remotePort);

                mongoDB.insertOne("account", obj, (err, result) => {

                    let loginInfo = new LoginInfo(0, "注册成功，直接登录");

                    this.emitTo("loginCheck", JSON.stringify(loginInfo), remoteIpString, remotePort);

                });

            } else {

                if (result.password == password) {

                    console.log(username, "已存在，直接登录");

                    let loginInfo = new LoginInfo(0, "登录成功");

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