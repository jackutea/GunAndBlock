class LoginSendInfo {

    constructor(stateCode, msg) {

        // 0 登录正确 1 登录密码错 2 登录用户名错 3 注册用户名被占用
        this.stateCode = stateCode;

        this.msg = msg;

    }
}

module.exports = LoginSendInfo;