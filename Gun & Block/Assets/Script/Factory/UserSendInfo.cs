using System;

[Serializable]
class UserSendInfo {

    public string username;
    public string password;

    public UserSendInfo(string username, string password) {

        this.username = username;

        this.password = password;
        
    }
}