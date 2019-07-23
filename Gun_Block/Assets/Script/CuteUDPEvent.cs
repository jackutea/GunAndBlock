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

        ServerDataScript.serverUserCountList = serverRecvInfo.serverUserCountList;

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

        PlayerDataScript.ROLES = roleJson;

        if (roleJson.Count > 0) {

            Debug.Log(roleJson.Keys.First());

        }

        SceneManager.LoadScene("RoleList");

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

        PlayerDataScript.ROLE_STATE.inComparingMode = modeCode;

        showAlertWindow("正在匹配中"); // TODO ： 换成其他显示方式

    }

    // 取消匹配回传
    public static void onCompareCancel(string dataString) {

        PlayerDataScript.ROLE_STATE.inComparingMode = -1;

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

    // 其他玩家移动
    public static void onMove(string dataString) {

        MoveInfo moveInfo = JsonUtility.FromJson<MoveInfo>(dataString);

        string targetSid = moveInfo.d;

        PlayerDataScript.FIELD_INFO.sidJson[targetSid].vecArray = moveInfo.v;

        FieldScript.Move(targetSid, moveInfo.v);
        
    }

    // 其他玩家取消移动
    // dataString = 取消移动的玩家 sid 
    public static void onCancelMove(string dataString) {

        string targetSid = dataString;

        PlayerDataScript.FIELD_INFO.sidJson[targetSid].isMoving = false;

        FieldScript.CancelMove(targetSid);
    }

    // 其他玩家格挡
    public static void onBlock(string dataString) {

        string targetSid = dataString;

        FieldScript.Block(targetSid);

    }

    // 其他玩家取消格挡
    public static void onCancelBlock(string dataString) {

        string targetSid = dataString;

        FieldScript.CancelBlock(targetSid);

    }

    // 其他玩家完美格挡
    public static void onPerfectBlock(string dataString) {

        string targetSid = dataString;

        FieldScript.PerfectBlock(targetSid);

    }

    // 其他玩家取消完美格挡
    public static void onCancelPerfectBlock(string dataString) {

        string targetSid = dataString;

        FieldScript.CancelPerfectBlock(targetSid);

    }

    // 其他玩家射击
    public static void onShoot(string dataString) {

        BulletInfo bulletInfo = JsonConvert.DeserializeObject<BulletInfo>(dataString);

        if (!PlayerDataScript.FIELD_INFO.bidJson.ContainsKey(bulletInfo.bid)) {

            PlayerDataScript.FIELD_INFO.bidJson.Add(bulletInfo.bid, bulletInfo);

        }

        FieldScript.Shoot(bulletInfo);

    }

    // 其他玩家完美格挡了子弹
    public static void onPerfectBlockBullet(string dataString) {

        Debug.Log(dataString);

        BulletInfo bulletInfo = JsonConvert.DeserializeObject<BulletInfo>(dataString);

        if (PlayerDataScript.FIELD_INFO.bidJson.ContainsKey(bulletInfo.bid)) {

            PlayerDataScript.FIELD_INFO.bidJson.Remove(bulletInfo.bid);

        }

        FieldScript.PerfectBlockBullet(bulletInfo);
        
    }

    // 其他玩家普通格挡了子弹
    public static void onBlockBullet(string dataString) {

        BulletInfo bulletInfo = JsonConvert.DeserializeObject<BulletInfo>(dataString);

        if (PlayerDataScript.FIELD_INFO.bidJson.ContainsKey(bulletInfo.bid)) {

            PlayerDataScript.FIELD_INFO.bidJson.Remove(bulletInfo.bid);

        }

        FieldScript.BlockBullet(bulletInfo);
        
    }

    // 其他玩家被子弹直接击中
    public static void onBeAttacked(string dataString) {

        BulletInfo bulletInfo = JsonConvert.DeserializeObject<BulletInfo>(dataString);

        if (PlayerDataScript.FIELD_INFO.bidJson.ContainsKey(bulletInfo.bid)) {

            PlayerDataScript.FIELD_INFO.bidJson.Remove(bulletInfo.bid);

        }

        FieldScript.BeAttacked(bulletInfo);
        
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