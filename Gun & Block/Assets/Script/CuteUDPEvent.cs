using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

class CuteUDPEvent : MonoBehaviour {

    
    // 接收登录信息 dataString : LoginInfo {stateCode : stateCode, msg : msg }
    // 0 成功 1密码错 2用户名错 3其他
    public static void onLoginRecv(string dataString, string remoteIp) {

        LoginRecvInfo loginRecvInfo = JsonUtility.FromJson<LoginRecvInfo>(dataString);

        if (loginRecvInfo.stateCode == 0) {

            showAlertWindow(loginRecvInfo.msg);

            ServerDataScript.serverIdList = loginRecvInfo.serverIdList;

            ServerDataScript.serverUserCountList = loginRecvInfo.serverUserCountList;

            SceneManager.LoadScene("ChooseServer");

            // SceneManager.LoadScene("Field");

        } else {

            showAlertWindow(loginRecvInfo.msg);

        }
    }

    // 接收服务器房间信息回传
    public static void onShowRoomRecv(string dataString, string remoteIp) {


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