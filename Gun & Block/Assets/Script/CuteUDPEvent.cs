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

        LoginInfo loginInfo = JsonUtility.FromJson<LoginInfo>(dataString);

        Debug.Log(loginInfo.msg);

        if (loginInfo.stateCode == 0) {

            showAlertWindow(loginInfo.msg);

            ServerDataScript.serverIdList = loginInfo.serverIdList;

            ServerDataScript.serverUserCountList = loginInfo.serverUserCountList;

            // Debug.LogWarning(loginInfo.serverUserCountList.Length);

            SceneManager.LoadScene("ChooseServer");

            // SceneManager.LoadScene("BattleField");

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