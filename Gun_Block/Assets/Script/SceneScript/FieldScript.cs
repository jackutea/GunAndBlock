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
    public Slider skill1;
    public Slider skill2;
    public Slider skill3;
    public Slider skill4;
    public Slider skill5;
    public Slider skill6;
    public Slider skill7;
    public Slider skill8;
    public Text skillCommad;

    void Awake() {

        if (GameObject.FindWithTag("MainScript") == null) {

            SceneManager.LoadScene("InitGame");

            return;

        }
    }

    void Start() {

        initBattle();

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

    // 加载战斗
    void initBattle() {

        if (PlayerDataScript.FIELD_INFO == null) return;

        fieldInfo = PlayerDataScript.FIELD_INFO;

        string modeCode = fieldInfo.modeCode;

        Debug.Log(fieldInfo.modeCode);

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
        GameObject towerObj = Instantiate(PrefabCollection.instance.tower, transform.parent.transform);

        towerObj.transform.localPosition = (roleState.isLeftAlly) ? new Vector2(rolePo.x - 120, rolePo.y) : new Vector2(rolePo.x + 120, rolePo.y);

    }

    // 技能显示
    void skillCheck() {

        if (PlayerDataScript.ROLE_STATE == null) return;

        RoleState rs = PlayerDataScript.ROLE_STATE;

        skill1 = skillCDCount(skill1, rs.blockSkill);

        skill2 = skillCDCount(skill2, null, rs.normalBullet);

        skill3 = skillCDCount(skill3, null, rs.slowBullet);
        
        skill4 = skillCDCount(skill4, null, rs.fastBullet);
        
        skill5 = skillCDCount(skill5, null, rs.rayBullet);
        
        skill6 = skillCDCount(skill6, rs.blockWall);
        
        skill7 = skillCDCount(skill7, rs.shadow);
        
        skill8 = skillCDCount(skill8, rs.shield);

        skillCommad.text = rs.currentSpell;

    }

    Slider skillCDCount(Slider slider, BuffSkill buffSkill = null, BulletSkill bulletSkill = null) {

        float cdOrigin = (buffSkill == null) ? bulletSkill.cdOrigin : buffSkill.cdOrigin;

        float cd = (buffSkill == null) ? bulletSkill.cd : buffSkill.cd;

        slider.maxValue = cdOrigin;

        slider.minValue = 0;

        slider.value = cd;

        return slider;
        
    }
}
