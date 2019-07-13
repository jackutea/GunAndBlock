using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

class CuteUDPEvent : MonoBehaviour {

    
    // 接收登录信息 dataString : LoginInfo {stateCode : stateCode, msg : msg }
    // 0 成功 1密码错 2用户名错 3其他
    public static void onLoginCheck(string dataString, string remoteIp) {

        ServerDataScript.LOGIN_INFO = JsonUtility.FromJson<LoginInfo>(dataString);

        LoginInfo loginInfo = ServerDataScript.LOGIN_INFO;

        if (loginInfo.stateCode == 0) {

            showAlertWindow(loginInfo.msg);

            for (int i = 0; i < loginInfo.serverIdList.Length; i += 1) {

                int serverName = loginInfo.serverIdList[i];

                int serverUserCount = loginInfo.serverUserCount[i];

                Debug.Log(serverName);

                Debug.Log(serverUserCount);

            }

            // SceneManager.LoadScene("ChooseServer");

            SceneManager.LoadScene("BattleField");

        } else {

            showAlertWindow(loginInfo.msg);

        }
    }

    // 弹窗信息
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
    
}