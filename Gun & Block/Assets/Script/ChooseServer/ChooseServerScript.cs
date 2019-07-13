using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseServerScript : MonoBehaviour {

    void Awake() {

        if (GameObject.FindWithTag("MainScript") == null) {

            SceneManager.LoadScene("InitGame");

            return;

        }
    }

    void Start() {

        LoginInfo loginInfo = ServerDataScript.LOGIN_INFO;

        for (int i = 0; i < loginInfo.serverIdList.Length; i += 1) {

            Debug.Log(loginInfo.serverIdList[i]);

        }
        
    }

    void Update() {
        
    }

    // 显示服务器列表
    void showServers(int num) {

        

    }

    // 显示房间列表
    void showRooms(int num) {

    }
}
