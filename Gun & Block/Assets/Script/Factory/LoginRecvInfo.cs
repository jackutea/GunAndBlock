using System;
using System.Collections.Generic;
using System.Collections;

[Serializable]
class LoginRecvInfo {

    // 0 正确 1密码错 2用户名错
    public int stateCode;
    public string msg;
    public int[] serverIdList = new int[0];
    public int[] serverUserCountList = new int[0];

    ..用多重请求，不要把数据混在一个类里

    public LoginRecvInfo(int stateCode, string msg) {

        this.stateCode = stateCode;

        this.msg = msg;

        // this.serverIdList = new int[100];

        // this.serverUserCount = new int[100];

    }
}