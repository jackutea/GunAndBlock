using System;
using System.Threading;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class CuteUDPEvent : MonoBehaviour {
    
    // 接收登录信息 dataString : LoginInfo {stateCode : stateCode, msg : msg }
    // 0 成功 1密码错 2用户名错 3其他
    public static void onLogin(string dataString) {

        LoginRecvInfo loginRecvInfo = JsonUtility.FromJson<LoginRecvInfo>(dataString);

        if (loginRecvInfo.stateCode == 0) {

            CuteUDPManager.cuteUDP.emitServer("ShowServer", "请求服务器列表");

        } else {

            showAlertWindow(loginRecvInfo.msg);

        }
    }

    // 接收服务器回传
    public static void onShowServer(string dataString) {

        ServerRecvInfo serverRecvInfo = JsonUtility.FromJson<ServerRecvInfo>(dataString);

        ServerDataScript.serverIdList = serverRecvInfo.serverIdList;

        ServerDataScript.serverNameList = serverRecvInfo.serverNameList;

        if (ServerDataScript.serverIdList.Length < 0) {

            showAlertWindow("服务器未开放");

        } else {

            SceneManager.LoadScene("ChooseServer");

        }
    }

    // 接收角色回传
    public static void onShowRoles(string dataString) {

        RoleListRecvInfo roleListInfo = JsonConvert.DeserializeObject<RoleListRecvInfo>(dataString);

        Dictionary<string, RoleState> roleJson = roleListInfo.roleJson;

        if (roleJson != null) {

            PlayerDataScript.ROLES = roleJson;

            SceneManager.LoadScene("RoleList");

        }
    }

    // 删除角色回传
    public static void onDeleteRole(string dataString) {

        Debug.Log("删除角色回传");

        string roleName = dataString;

        PlayerDataScript.ROLES.Remove(roleName);

        SceneManager.LoadScene("RoleList");

    }

    // 接收创建角色成功回传
    public static void onCreateRole(string dataString) {

        RoleState oneRole = JsonUtility.FromJson<RoleState>(dataString);

        PlayerDataScript.ROLES.Add(oneRole.roleName, oneRole);

        SceneManager.LoadScene("RoleList");

    }

    // 创建角色失败回传
    public static void onCreateRoleFail(string dataString) {

        showAlertWindow("角色名已存在");

    }

    // 进入游戏回传
    public static void onEnterGame(string dataString) {

        // Debug.Log("进入HOME");

        SceneManager.LoadScene("Home");

    }

    // 等待匹配回传
    public static void onCompareWait(string dataString) {

        int modeCode = int.Parse(dataString);

        PlayerDataScript.ROLE_STATE.waitMode = modeCode;

        showAlertWindow("正在匹配中"); // TODO ： 换成其他显示方式

    }

    // 取消匹配回传
    public static void onCompareCancel(string dataString) {

        PlayerDataScript.ROLE_STATE.waitMode = -1;

    }

    // 匹配成功回传
    public static void onCompareSuccess(string dataString) {

        // dataString = class FieldInfo
        Debug.LogWarning("匹配成功，回传战场数据 :" + dataString);
        FieldInfo fieldInfo = JsonConvert.DeserializeObject<FieldInfo>(dataString);

        PlayerDataScript.FIELD_INFO = fieldInfo;

        SceneManager.LoadScene("Field");

    }

    // 接收服务器房间信息回传
    public static void onShowRoom(string dataString) {


    }


    // TODO 退出登录
    // public static

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