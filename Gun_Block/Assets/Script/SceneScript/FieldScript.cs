using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FieldScript : MonoBehaviour {

    public static FieldScript instance;

    // 相机
    public GameObject cameraObject;
    public Camera mainCamera;

    // 战场
    public string meSid;
    public GameObject battlePanel;
    public FieldState fieldState;

    public List<string> leftSidList;
    public List<string> rightSidList;
    public List<string> loseSidList;
    public List<string> winSidList;

    // UI
    public GameObject HUDPanel;
    public GameObject skillPanel;
    Dictionary<int, Slider> skillSliderDic;
    public Text skillCommad;
    public Text liveStateText;
    public Text winStateText;

    void Awake() {

        if (GameObject.FindWithTag("MainScript") == null) {

            SceneManager.LoadScene("InitGame");

            return;

        }
    }

    void Start() {

        if (instance == null) instance = this;

        leftSidList = new List<string>();

        rightSidList = new List<string>();

        winSidList = new List<string>();

        loseSidList = new List<string>();

        winStateText.text = "";

        liveStateText.text = "";

        initBattle();

        initSlider();

    }

    void Update() {

        sliderCheck();

        winStateCheck();

    }

    void FixedUpdate() {

    }

    void LateUpdate() {

        cameraControl();

    }

    // 相机控制
    void cameraControl() {


    }

    void initSlider() {

        skillSliderDic = new Dictionary<int, Slider>();

        if (PlayerDataScript.ROLE_STATE == null) return;

        RoleState rs = PlayerDataScript.ROLE_STATE;

        float xGap = 90;

        for (int i = 0; i < rs.skillList.Length; i += 1) {

            int skillIndex = i;

            SkillState skill = rs.skillList[skillIndex];

            skillSliderDic.Add(skillIndex, Instantiate(PrefabCollection.instance.skillSlider, skillPanel.transform));

            Slider skillSlider = skillSliderDic[skillIndex];

            skillSlider.maxValue = skill.cdOrigin;

            skillSlider.minValue = 0;

            Text sliderName = skillSlider.GetComponentInChildren<Text>();

            sliderName.text = skill.skillName + "\n" + ConfigCollection.SkillSpell[skillIndex];

            Vector2 sliderPo = skillSlider.transform.localPosition;

            skillSlider.transform.localPosition = new Vector2(sliderPo.x + xGap * i, sliderPo.y);

        }
    }

    // 加载战斗
    void initBattle() {

        if (PlayerDataScript.FIELD_STATE == null) return;

        fieldState = PlayerDataScript.FIELD_STATE;

        string modeCode = fieldState.modeCode;

        int marchRoadNum = 0;

        if (modeCode == "0") marchRoadNum = 1;
        else if (modeCode == "1") marchRoadNum = 5;
        else if (modeCode == "2") marchRoadNum = 50;
        else return;

        loadRoad(marchRoadNum);

    }

    // 生成赛道
    void loadRoad(int num) {

        if (fieldState.sidJson == null || fieldState.sidJson.Count <= 0) return;

        Rect battlePanelRect = battlePanel.GetComponent<RectTransform>().rect;

        List<string> sidList = new List<string>(fieldState.sidJson.Keys);

        for (int i = 0; i < fieldState.sidJson.Count; i += 1) {

            if (i % 2 == 0) {

                GameObject roadLine = Instantiate(PrefabCollection.instance.roadLine, battlePanel.transform);

                RectTransform roadLineRect = roadLine.GetComponent<RectTransform>();

                roadLineRect.rect.Set(0, 0, battlePanelRect.width - 200, roadLineRect.rect.height);
            
                float heightGap = roadLineRect.rect.height + 20;

                Vector2 roadPo = roadLine.transform.localPosition;

                roadLine.transform.localPosition = new Vector2(roadPo.x, roadPo.y + heightGap * i / 2);

                string leftSid = sidList[i];

                RoleState leftRole = fieldState.sidJson[leftSid];

                string rightSid = sidList[i + 1];

                RoleState rightRole = fieldState.sidJson[rightSid];

                leftRole.oppoSid = rightRole.sid;

                rightRole.oppoSid = leftRole.sid;

                leftSidList.Add(leftRole.sid);

                rightSidList.Add(rightRole.sid);

                bornRole(i / 2, leftRole, roadLine, roadLineRect.rect, rightSid);

                bornRole(i / 2, rightRole, roadLine, roadLineRect.rect, leftSid);

            }
        }
    }

    // 生成角色 me & other
    void bornRole(int marchIndex, RoleState roleState,GameObject roadLine, Rect roadLineRect, string oppoSid) {

        // 设置名字
        GameObject roleObj = Instantiate(PrefabCollection.instance.rolePrefab, roadLine.transform);

        string sid = roleState.sid;

        roleObj.name = sid;

        // 加载角色属性
        RoleScript roleScript = roleObj.GetComponent<RoleScript>();

        roleScript.roleState = roleState;

        roleScript.oppoSid = oppoSid;

        roleScript.marchIndex = marchIndex;

        if (roleState.username == PlayerDataScript.USER_NAME) {

            roleScript.isMe = true;

            this.meSid = sid;

        }

        // 设置出生地点
        float bornPointX = (roleState.isLeftAlly) ? -roadLineRect.width / 2 + 200: roadLineRect.width / 2 - 200;

        Vector2 rolePo = roleObj.transform.localPosition;

        roleObj.transform.localPosition = new Vector2(bornPointX, rolePo.y);

        // 设置翻转
        bool isRotate = (roleState.isLeftAlly) ? false : true;

        SpriteRenderer roleRender = roleObj.GetComponentInChildren<SpriteRenderer>();

        if (isRotate) roleRender.flipY = isRotate;

    }

    // 技能显示
    void sliderCheck() {

        if (PlayerDataScript.ROLE_STATE == null) return;

        RoleState rs = PlayerDataScript.ROLE_STATE;

        if (rs.skillList.Length <= 0) return;

        for (int i = 0; i < rs.skillList.Length; i += 1) {

            int skillIndex = i;

            SkillState skill = rs.skillList[skillIndex];

            skillSliderDic[skillIndex].value = skill.cd;

        }

        skillCommad.text = rs.currentSpell;

    }

    // 胜负情况显示
    void winStateCheck() {

        int leftLives = leftSidList.Count;

        int rightLives = rightSidList.Count;

        string showDashBoard = leftLives.ToString() + " | " + rightLives.ToString();

        liveStateText.text = showDashBoard;

    }

    // 有人挂菜
    public void loseGame(string deadSid) {

        GameObject whoDead = GameObject.Find(deadSid);

        RoleScript rs = whoDead.GetComponent<RoleScript>();

        rs.roleState.isDead = true;

        loseSidList.Add(deadSid);

        // 计算对线失败玩家情况
        int loseIndex = loseSidList.Count;

        if(rs.roleState.isLeftAlly) {

            leftSidList.Remove(deadSid);

        } else {

            rightSidList.Remove(deadSid);

        }

        string winSid = rs.roleState.oppoSid;

        winSidList.Add(winSid);

        // 计算对线胜利玩家情况
        int rankIndex = winSidList.Count;

        GameObject whoWin = GameObject.Find(winSid);

        RoleScript winRs = whoWin.GetComponent<RoleScript>();

        // 计算全局存活玩家
        int leftLives = leftSidList.Count;

        int rightLives = rightSidList.Count;

        if (leftLives <= 0) {

            // 左输

        } else if (rightLives <= 0) {

            // 右输
            
        }

        if (winSidList.Count > 5) return;

        string roleName = winRs.roleState.roleName;

        string winName = rankIndex.ToString() + " . " + roleName;

        winStateText.text += winName + "\n";

    }

    // 胜负结算
    public void gameOver(int loser) {

        Debug.Log("收到结算请求");

        GameObject meObj = GameObject.Find(meSid);

        RoleScript meRs = meObj.GetComponent<RoleScript>();

        RoleState roleState = meRs.roleState;

        GameObject gameOverWindow = Instantiate(PrefabCollection.instance.gameOverWindow, HUDPanel.transform);

        GameOverScript gos = gameOverWindow.GetComponent<GameOverScript>();

        if (loser == 0) {

            // 左败
            if (roleState.isLeftAlly) {

                gos.resultShowText.text = "您的队伍输了";

                gos.scoreText.text = "积分 -10";

                gos.expText.text = "经验 +0";

            } else {

                gos.resultShowText.text = "您的队伍赢了";

                gos.scoreText.text = "积分 +10";

                gos.expText.text = "经验 +10";

            }

        } else if (loser == 1) {

            // 左胜
            if (!roleState.isLeftAlly) {

                gos.resultShowText.text = "您的队伍输了";

                gos.scoreText.text = "积分 -10";

                gos.expText.text = "经验 +0";

            } else {

                gos.resultShowText.text = "您的队伍赢了";

                gos.scoreText.text = "积分 +10";

                gos.expText.text = "经验 +10";

            }

        } else {

            // 发生错误
            Debug.LogError("发生错误");

        }
    }
}
