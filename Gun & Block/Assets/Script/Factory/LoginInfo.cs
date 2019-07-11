
class LoginInfo {

    // 0 正确 1密码错 2用户名错
    public int stateCode;

    public string msg;

    public LoginInfo(int stateCode, string msg) {

        this.stateCode = stateCode;

        this.msg = msg;
        
    }
}