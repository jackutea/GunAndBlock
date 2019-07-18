using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HomeScript : MonoBehaviour {

    // TODO : 匹配面板
    public Button soloCompareBtn;
    public Button teamCompareBtn;
    public Button raidCompareBtn;
    public Button customRoomBtn;

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
        
    }

    void compareSend(string modeCode) {

        if (PlayerDataScript.ROLE_STATE.isComparing) {

            CuteUDPEvent.showAlertWindow("正在匹配中");

        } else {

            CuteUDPManager.cuteUDP.emitServer("Compare", modeCode);

        }
    }

    void Update() {
        
    }

    // 加载角色信息
    void showRoleInfo() {

        RoleState rs = PlayerDataScript.ROLE_STATE;

        username.text = "用户名 : " + rs.username;

        roleName.text = "角色名 : " + rs.roleName;

        level.text = "Lv : " + rs.level.ToString();
        
        exp.text = "经验值 : " + rs.exp.ToString();

        rank.text = "段位 : " + rs.rank.ToString();
        
        score.text = "排位积分 : " + rs.score.ToString();
        
        life.text = "生命 : " + rs.life.ToString();
        
        blockLife.text = "盾强度 : " + rs.blockLife.ToString();
        
        damage.text = "伤害 : " + rs.damage.ToString();
        
        shootGap.text = "射击时间间隔 : " + rs.shootGap.ToString();
        
        blockGap.text = "格挡时间间隔 : " + rs.blockGap.ToString();
        
        perfectBlockLast.text = "完美格挡持续时间 : " + rs.perfectBlockGap.ToString();
        
        moveSpeed.text = "移动速度 : " + rs.moveSpeed.ToString();
        
        bulletSpeed.text = "子弹速度 : " + rs.shootSpeed.ToString();

    }
}
