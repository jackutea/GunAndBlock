using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HomeScript : MonoBehaviour {

    public static HomeScript instance;

    public GameObject HUDPanel;

    // TODO : 匹配面板
    public Button soloCompareBtn;
    public Button teamCompareBtn;
    public Button raidCompareBtn;
    public Button customRoomBtn;

    public Button backToTitleBtn;

    // 角色信息面板
    public Text username;
    public Text roleName;
    public Text level;
    public Text exp;
    public Text rank;
    public Text score;
    public Text life;
    public Text blockLife;
    public Text damage;
    public Text shootGap;
    public Text blockGap;
    public Text perfectBlockLast;
    public Text moveSpeed;
    public Text bulletSpeed;

    void Awake() {

        if (GameObject.FindWithTag("MainScript") == null) {

            SceneManager.LoadScene("InitGame");

            return;

        }
    }

    void Start() {

        if (instance == null) instance = this;

        showRoleInfo();

        soloCompareBtn.onClick.AddListener(() => {

            compareSend("0");

        });

        teamCompareBtn.onClick.AddListener(() => {

            compareSend("1");

        });

        raidCompareBtn.onClick.AddListener(() => {

            compareSend("2");

        });

        customRoomBtn.onClick.AddListener(() => {

            CuteUDPEvent.showAlertWindow("暂不支持自定义房间模式");

        });

        backToTitleBtn.onClick.AddListener(() => {

            SceneManager.LoadScene("Login");

        });
        
    }

    void compareSend(string modeCode) {

        CuteUDPManager.cuteUDP.emitServer(CompareEventEnum.Compare.ToString(), modeCode);

        showCompareWindow(modeCode);
    }

    void Update() {
        
    }

    // 加载角色信息
    void showRoleInfo() {

        if (PlayerDataScript.ROLE_STATE == null) return;

        RoleState rs = PlayerDataScript.ROLE_STATE;

        username.text = "用户名 : " + rs.username;

        roleName.text = "角色名 : " + rs.roleName;

        level.text = "Lv : " + rs.level.ToString();
        
        exp.text = "经验值 : " + rs.exp.ToString();

        rank.text = "段位 : " + rs.rank.ToString();
        
        score.text = "积分 : " + rs.score.ToString();
        
        life.text = "生命 : " + rs.life.ToString();
        
        blockLife.text = "盾强度 : " + rs.blockLife.ToString();
        
    }

    // 显示匹配窗
    public void showCompareWindow(string modeCode) {

        GameObject compareWindowObj = Instantiate(PrefabCollection.instance.compareWindow, HUDPanel.transform);

        CompareWindowScript cws = compareWindowObj.GetComponent<CompareWindowScript>();

        cws.modeCode = modeCode;

    }
}
