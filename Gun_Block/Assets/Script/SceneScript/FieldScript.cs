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

    void Awake() {

        if (GameObject.FindWithTag("MainScript") == null) {

            SceneManager.LoadScene("InitGame");

        }
    }

    void Start() {

        initBattle();

    }

    void Update() {

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

        GameObject roadLinePrefab = PrefabCollection.instance.roadLine;

        Rect roadLineRect = roadLinePrefab.GetComponent<RectTransform>().rect;

        roadLineRect.width = battlePanelRect.width - 200;
        
        float heightGap = roadLineRect.height + 20;

        int i = 0;

        foreach (var kv in fieldInfo.sidJson) {

            string sid = kv.Key;

            RoleState roleState = kv.Value;

            GameObject roadLine = Instantiate(PrefabCollection.instance.roadLine, battlePanel.transform);

            Vector2 roadPo = roadLine.transform.localPosition;

            roadLine.transform.localPosition = new Vector2(roadPo.x, roadPo.y + heightGap * i);

            bornRole(i, roleState, roadLine, roadLineRect);

            i += 1;

        }

    }

    // 生成角色 me & other
    void bornRole(int i, RoleState roleState, GameObject roadLine, Rect roadLineRect) {

        GameObject roleObj = Instantiate(PrefabCollection.instance.rolePrefab, roadLine.transform);

        string sid = roleState.sid;

        roleObj.name = sid;

        float bornPointX = (roleState.isLeftAlly) ? -roadLineRect.width : roadLineRect.width;

        Vector2 rolePo = roleObj.transform.localPosition;

        roleObj.transform.localPosition = new Vector2(bornPointX, rolePo.y);

        ..as图像翻转，塔，相机

    }
    
}
