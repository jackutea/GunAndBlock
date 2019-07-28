using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FieldScript : MonoBehaviour {

    // 相机
    public GameObject cameraObject;
    public Camera mainCamera;

    // 战场
    public GameObject battlePanel;
    public FieldInfo fieldInfo;

    // UI
    public GameObject HUDPanel;
    public GameObject skillPanel;
    Dictionary<int, Slider> skillSliderDic;
    public Text skillCommad;

    void Awake() {

        if (GameObject.FindWithTag("MainScript") == null) {

            SceneManager.LoadScene("InitGame");

            return;

        }
    }

    void Start() {

        initBattle();

        initSlider();

    }

    void Update() {

        skillCheck();

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

            Skill skill = rs.skillList[skillIndex];

            skillSliderDic.Add(skillIndex, Instantiate(PrefabCollection.instance.skillSlider, skillPanel.transform));

            Slider skillSlider = skillSliderDic[skillIndex];

            skillSlider.maxValue = skill.cdOrigin;

            skillSlider.minValue = 0;

            Text sliderName = skillSlider.GetComponentInChildren<Text>();

            sliderName.text = ConfigCollection.SkillSpell[skillIndex];

            Vector2 sliderPo = skillSlider.transform.localPosition;

            skillSlider.transform.localPosition = new Vector2(sliderPo.x + xGap * i, sliderPo.y);

        }
    }

    // 加载战斗
    void initBattle() {

        if (PlayerDataScript.FIELD_INFO == null) return;

        fieldInfo = PlayerDataScript.FIELD_INFO;

        string modeCode = fieldInfo.modeCode;

        int marchRoadNum = 0;

        if (modeCode == "0") marchRoadNum = 1;
        else if (modeCode == "1") marchRoadNum = 5;
        else if (modeCode == "2") marchRoadNum = 50;
        else return;

        loadRoad(marchRoadNum);

    }

    // 生成赛道
    void loadRoad(int num) {

        if (fieldInfo.sidJson == null || fieldInfo.sidJson.Count <= 0) return;

        Rect battlePanelRect = battlePanel.GetComponent<RectTransform>().rect;

        List<string> sidList = new List<string>(fieldInfo.sidJson.Keys);

        for (int i = 0; i < fieldInfo.sidJson.Count; i += 1) {

            if (i % 2 == 0) {

                GameObject roadLine = Instantiate(PrefabCollection.instance.roadLine, battlePanel.transform);

                RectTransform roadLineRect = roadLine.GetComponent<RectTransform>();

                roadLineRect.rect.Set(0, 0, battlePanelRect.width - 200, roadLineRect.rect.height);
            
                float heightGap = roadLineRect.rect.height + 20;

                Vector2 roadPo = roadLine.transform.localPosition;

                roadLine.transform.localPosition = new Vector2(roadPo.x, roadPo.y + heightGap * i / 2);

                string leftSid = sidList[i];

                RoleState leftRole = fieldInfo.sidJson[leftSid];

                string rightSid = sidList[i + 1];

                RoleState rightRole = fieldInfo.sidJson[rightSid];

                bornRole(leftRole, roadLine, roadLineRect.rect);

                bornRole(rightRole, roadLine, roadLineRect.rect);

            }
        }
    }

    // 生成角色 me & other
    void bornRole(RoleState roleState, GameObject roadLine, Rect roadLineRect) {

        // 设置名字
        GameObject roleObj = Instantiate(PrefabCollection.instance.rolePrefab, roadLine.transform);

        string sid = roleState.sid;

        roleObj.name = sid;

        // 加载角色属性
        RoleScript roleScript = roleObj.GetComponent<RoleScript>();

        roleScript.roleState = roleState;

        if (roleState.username == PlayerDataScript.USER_NAME) {

            roleScript.isMe = true;

        }

        // 设置出生地点
        float bornPointX = (roleState.isLeftAlly) ? -roadLineRect.width / 2 + 200: roadLineRect.width / 2 - 200;

        Vector2 rolePo = roleObj.transform.localPosition;

        roleObj.transform.localPosition = new Vector2(bornPointX, rolePo.y);

        // 设置翻转
        bool isRotate = (roleState.isLeftAlly) ? false : true;

        SpriteRenderer roleRender = roleObj.GetComponentInChildren<SpriteRenderer>();

        if (isRotate) roleRender.flipY = isRotate;

        // 生成塔
        GameObject towerObj = Instantiate(PrefabCollection.instance.tower, roadLine.transform);

        towerObj.transform.localPosition = (roleState.isLeftAlly) ? new Vector2(roleObj.transform.localPosition.x - 120, rolePo.y) : new Vector2(roleObj.transform.localPosition.x + 120, rolePo.y);

    }

    // 技能显示
    void skillCheck() {

        if (PlayerDataScript.ROLE_STATE == null) return;

        RoleState rs = PlayerDataScript.ROLE_STATE;

        if (rs.skillList.Length <= 0) return;

        for (int i = 0; i < rs.skillList.Length; i += 1) {

            int skillIndex = i;

            Skill skill = rs.skillList[skillIndex];

            skillSliderDic[skillIndex].value = skill.cd;

        }

        skillCommad.text = rs.currentSpell;

    }
}
