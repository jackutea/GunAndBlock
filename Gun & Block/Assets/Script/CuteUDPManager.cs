using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using CuteUDPApp;
using Newtonsoft.Json;

public class CuteUDPManager : MonoBehaviour {
    public static CuteUDPManager instance;
    public static CuteUDP cuteUDP;

    void Start() {

        instance = this;

        DontDestroyOnLoad(this);

        cuteUDP = new CuteUDP("127.0.0.1", 11000, 10000);

        if (SceneManager.GetActiveScene().name == "InitGame") {

            SceneManager.LoadScene("Title");
            
        }

    }

    void initPrivateVoid() {

        cuteUDP.on<string, string, int>("loginCheck", onLoginCheck);

    }

    // 接收登录信息 dataString : LoginInfo {stateCode : stateCode, msg : msg }
    // 0 成功 1用户名不存在 2用户名存在，密码不对 3其他
    void onLoginCheck(string dataString, string remoteIp, int remotePort) {

        LoginInfo li = JsonConvert.DeserializeObject<LoginInfo>(dataString);

        if (li.stateCode == 0) {

            SceneManager.LoadScene("BattleField");

        } else {

            showAlertWindow(li.msg);

        }

    }

    public static void showAlertWindow(string msg) {

        GameObject HUDPanel = GameObject.Find("HUDPanel");

        if (HUDPanel == null) return;

        GameObject alertWindow = Instantiate(PrefabCollection.instance.alertWindow, HUDPanel.transform);

        Text infoMsg = alertWindow.GetComponentInChildren<Text>();

        infoMsg.text = msg;

        Button infoBtn = alertWindow.GetComponentInChildren<Button>();

        infoBtn.onClick.AddListener(() => {

            alertWindow.SetActive(false);

        });
    }

    void Update() {
        
    }

    void OnApplicationQuit() {

        cuteUDP.quitCuteUDP();

        Debug.Log("退");
    }
}
