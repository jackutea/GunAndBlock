using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginScript : MonoBehaviour {

    public GameObject loginPanel;
    public Button loginBtn;
    public Button registerBtn;
    public Text username;
    public Text password;
    public Button quitGameBtn;

    public GameObject registerPanel;
    public Text newUsername;
    public Text newPassword;
    public Text newRePassword;
    public Button confirmRegisterBtn;
    public Button backLoginBtn;

    void Awake() {

        if (GameObject.FindWithTag("MainScript") == null) {

            SceneManager.LoadScene("InitGame");

            return;

        }
    }
    
    void Start() {

        // 默认隐藏注册页
        registerPanel.SetActive(false);

        // 点击登录
        loginBtn.onClick.AddListener(() => {

            if (username.text != "" && username.text != null) {

                UserSendInfo user = new UserSendInfo(username.text, password.text);

                PlayerDataScript.USER_NAME = username.text;

                string dataString = JsonUtility.ToJson(user);

                CuteUDPManager.cuteUDP.emitServer("Login", dataString);

            } else {

                CuteUDPEvent.showAlertWindow("用户名为空");

            }
        });

        // 点击注册，进入注册页
        registerBtn.onClick.AddListener(() => {

            registerPanel.SetActive(true);

            loginPanel.SetActive(false);

        });

        // 点击退出游戏
        quitGameBtn.onClick.AddListener(() => {

            Application.Quit();

        });

        // 确认注册
        confirmRegisterBtn.onClick.AddListener(() => {

            if (newUsername.text != "" || newUsername.text != null) {

                if (newPassword.text == newRePassword.text && (newPassword.text != "" || newPassword.text != null)) {

                    UserSendInfo user = new UserSendInfo(newUsername.text, newPassword.text);

                    PlayerDataScript.USER_NAME = username.text;

                    string dataString = JsonUtility.ToJson(user);

                    CuteUDPManager.cuteUDP.emitServer("Register", dataString);

                } else {

                    CuteUDPEvent.showAlertWindow("两次输入的密码不一致");

                }

            } else {

                CuteUDPEvent.showAlertWindow("用户名不能为空");

            }
        });

        // 在注册页点击返回，返回登录页
        backLoginBtn.onClick.AddListener(() => {

            registerPanel.SetActive(false);

            loginPanel.SetActive(true);

        });

    }

    void Update() {


    }
}
