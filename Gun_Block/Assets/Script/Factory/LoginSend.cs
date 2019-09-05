using System;

[Serializable]
public class LoginSend {

    public string username;
    public string password;

    public LoginSend(string username, string password) {

        this.username = username;

        this.password = password;
        
    }
}