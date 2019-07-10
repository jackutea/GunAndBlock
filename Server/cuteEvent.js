var LoginInfo = require("./Factory/LoginInfo");
var mongoDB = require("./mongoDB/mongoDB");

module.exports = {

    // 传来登录申请 dataString 类型为 UserInfo
    login : function (dataString, remoteIpString, remotePort) {

        let UserInfo = JSON.parse(dataString);

        let username = UserInfo.username;

        let password = UserInfo.password;

        let findObj = {username : username, password : password};

        console.log("login");

        mongoDB.find("account", findObj, (err, result) => {

            if (err) throw err;

            console.log(result);

        });
        
    }
}