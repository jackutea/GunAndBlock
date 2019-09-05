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

        LoginRecv loginRecvInfo = JsonUtility.FromJson<LoginRecv>(dataString);

        if (loginRecvInfo.stateCode == 0) {

            CuteUDPManager.cuteUDP.emitServer(HallEventEnum.ShowServer.ToString(), "请求服务器列表");

        } else {

            showAlertWindow(loginRecvInfo.msg);

        }
    }

    // 接收服务器回传
    public static void onShowServer(string dataString) {

        Debug.Log("服务器信息" + dataString);

        ServerRecv serverRecvInfo = JsonConvert.DeserializeObject<ServerRecv>(dataString);

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

        RoleListRecv roleListInfo = JsonConvert.DeserializeObject<RoleListRecv>(dataString);

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

        // showAlertWindow("正在匹配中"); // TODO ： 换成其他显示方式

    }

    // 取消匹配回传
    public static void onCancelCompare(string dataString) {

        PlayerDataScript.ROLE_STATE.waitMode = -1;

    }

    // 匹配成功回传
    public static void onCompareSuccess(string dataString) {

        // dataString = class FieldInfo
        Debug.LogWarning("匹配成功，回传战场数据 :" + dataString);
        FieldState fieldInfo = JsonConvert.DeserializeObject<FieldState>(dataString);

        PlayerDataScript.FIELD_STATE = fieldInfo;

        SceneManager.LoadScene("Field");

    }

    // 接收服务器房间信息回传
    public static void onShowRoom(string dataString) {


    }

    // 有人施放技能
    public static void onCastSkill(string dataString) {

        CastSkillRecv castSkillRecv = JsonConvert.DeserializeObject<CastSkillRecv>(dataString);

        int skillEnum = castSkillRecv.skillEnum;

        string sid = castSkillRecv.sid;

        GameObject whoCast = GameObject.Find(sid);

        if (whoCast == null) return;

        RoleScript rs = whoCast.GetComponent<RoleScript>();

        rs.castSkill(skillEnum, castSkillRecv.timeSample);

    }

    // 有人反射子弹
    public static void onReflectBullet(string dataString) {

        BulletRecv bulletRecv = JsonConvert.DeserializeObject<BulletRecv>(dataString);

        string bid = bulletRecv.bid;

        GameObject whichBullet = GameObject.Find(bid);

        if (whichBullet == null) return;

        SkillScript bs = whichBullet.GetComponent<SkillScript>();

        bs.onCol(bulletRecv.sid, BattleEventEnum.ReflectBullet);

    }

    // 有人免疫子弹
    public static void onImmuneBullet(string dataString) {

        BulletRecv bulletRecv = JsonConvert.DeserializeObject<BulletRecv>(dataString);

        string bid = bulletRecv.bid;

        GameObject whichBullet = GameObject.Find(bid);

        if (whichBullet == null) return;

        SkillScript bs = whichBullet.GetComponent<SkillScript>();

        bs.onCol(bulletRecv.sid, BattleEventEnum.ImmuneBullet);

    }

    // 有人消灭子弹
    public static void onKillBullet(string dataString) {

        BulletRecv bulletRecv = JsonConvert.DeserializeObject<BulletRecv>(dataString);

        string bid = bulletRecv.bid;

        GameObject whichBullet = GameObject.Find(bid);

        if (whichBullet == null) return;

        SkillScript bs = whichBullet.GetComponent<SkillScript>();

        bs.onCol(bulletRecv.sid, BattleEventEnum.KillBullet);

    }

    // 有人格挡子弹
    public static void onBlockBullet(string dataString) {

        BulletRecv bulletRecv = JsonConvert.DeserializeObject<BulletRecv>(dataString);

        string bid = bulletRecv.bid;

        GameObject whichBullet = GameObject.Find(bid);

        if (whichBullet == null) return;

        SkillScript bs = whichBullet.GetComponent<SkillScript>();

        bs.onCol(bulletRecv.sid, BattleEventEnum.BlockBullet);

    }

    // 有人被直接击中
    public static void onBeAttacked(string dataString) {

        BulletRecv bulletRecv = JsonConvert.DeserializeObject<BulletRecv>(dataString);

        string bid = bulletRecv.bid;

        GameObject whichBullet = GameObject.Find(bid);

        if (whichBullet == null) return;

        SkillScript bs = whichBullet.GetComponent<SkillScript>();

        bs.onCol(bulletRecv.sid, BattleEventEnum.BeAttacked);

    }

    // 有人挂菜
    public static void onDead(string dataString) {

        string deadSid = dataString;

        FieldScript.instance.loseGame(deadSid);

    }

    // 游戏结束
    public static void onGameOver(string dataString) {

        // dataString = int loser(0 为左边玩家败)
        int loser = int.Parse(dataString);

        FieldScript.instance.gameOver(loser);

    }

    // 游戏结算后，接收单角色回传，并跳转至主页
    public static void onRequestRoleState(string dataString) {

        RoleState rs = JsonConvert.DeserializeObject<RoleState>(dataString);

        PlayerDataScript.ROLE_STATE = rs;

        SceneManager.LoadScene("Home");

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