using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginScript : MonoBehaviour {

    public Button loginBtn;
    public Text username;

    void Awake() {

        if (GameObject.FindWithTag("MainScript") == null) {

            SceneManager.LoadScene("InitGame");

            return;

        }

    }
    void Start() {

        loginBtn.onClick.AddListener(() => {

            Debug.Log("点击登录");

            if (username.text != "" && username.text != null) {

                Debug.Log("用户名是：" + username.text);

                UserInfo user = new UserInfo(username.text, "");

                PlayerDataScript.USER_NAME = username.text;

                string dataString = JsonUtility.ToJson(user);

                CuteUDPManager.cuteUDP.emitServer("login", dataString);

            } else {

                CuteUDPEvent.showAlertWindow("用户名为空");

            }
        });
    }

    void Update() {
        
    }
}
