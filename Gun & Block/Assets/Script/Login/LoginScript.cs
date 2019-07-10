using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class LoginScript : MonoBehaviour {

    public Button loginBtn;
    public Text username;
    void Start() {

        loginBtn.onClick.AddListener(() => {

            if (username.text != "" && username.text != null) {

                UserInfo user = new UserInfo(username.text, "");

                string dataString = JsonConvert.SerializeObject(user);

                CuteUDPManager.cuteUDP.emitServer("login", dataString);

            }
        });
    }

    void Update() {
        
    }
}
