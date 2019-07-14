using System.Collections.Generic;
using System.Collections;

class LoginInfo {

    // 0 正确 1密码错 2用户名错
    public int stateCode;
    public string msg;
    public int[] serverIdList;
    public int[] serverUserCountList;

    public LoginInfo(int stateCode, string msg) {

        this.stateCode = stateCode;

        this.msg = msg;

        // this.serverIdList = new int[100];

        // this.serverUserCount = new int[100];

    }
}