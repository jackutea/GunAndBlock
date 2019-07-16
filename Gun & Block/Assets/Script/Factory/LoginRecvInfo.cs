using System;
using System.Collections.Generic;
using System.Collections;

[Serializable]
class LoginRecvInfo {

    // 0 正确 1密码错 2用户名错
    public int stateCode;
    public string msg;

    public LoginRecvInfo(int stateCode, string msg) {

        this.stateCode = stateCode;

        this.msg = msg;

    }
}