
class LoginInfo {

    constructor(stateCode, msg) {

        // 0 正确 1 密码错 2 用户名错
        this.stateCode = stateCode;

        this.msg = msg;
    }
}

module.exports = LoginInfo;