using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FieldScript : MonoBehaviour {

    public static FieldScript instance;
    public Camera mainCamera;
    public GameObject battlePanel;
    RectTransform battlePanelRect;
    public float scaleSpeed;

    public FieldInfo fieldInfo;
    public int fieldId;
    public int modeCode;
    public Dictionary<string, RoleState> sidJson;
    public static Dictionary<string, GameObject> roleInstanceDic;

    void Awake() {

        if (GameObject.FindWithTag("MainScript") == null) {

            SceneManager.LoadScene("InitGame");

        }
    }

    void Start() {

        if (instance == null) instance = this;

        fieldInfo = PlayerDataScript.FIELD_INFO;

        fieldId = fieldInfo.fieldId;

        modeCode = fieldInfo.modeCode;

        sidJson = fieldInfo.sidJson;

        roleInstanceDic = new Dictionary<string, GameObject>();

        battlePanelRect = GetComponent<RectTransform>();

        scaleSpeed = 0.5f;

        born();

    }

    void Update() {

    }

    // 生成角色 me & other
    void born() {

        List<string> sidList = new List<string>(sidJson.Keys);

        for (int i = 0; i < sidJson.Count; i += 1) {
            
            string sid = sidList[i];

            RoleState role = sidJson[sid];

            GameObject roleObj = Instantiate(PrefabCollection.instance.rolePrefab, battlePanel.transform);

            roleObj.name = role.sid;

            RoleScript roleScript = roleObj.GetComponentInChildren<RoleScript>();

            roleScript.roleState = role;

            roleScript.isMe = (role.roleName == PlayerDataScript.ROLE_STATE.roleName) ? true : false;

            roleScript.roleInstance = roleObj;

            roleScript.battlePanel = battlePanel;

            roleScript.battlePanelRect = battlePanelRect;

            SpriteRenderer roleRenderer = roleScript.roleInstance.GetComponentInChildren<SpriteRenderer>();

            if(roleScript.roleState.isLeftAlly) {

                roleRenderer.flipY = false;

                roleScript.roleInstance.transform.localPosition = new Vector3(-battlePanelRect.rect.width / 2 + 100, 0, 0);

            } else {

                roleRenderer.flipY = true;

                roleScript.roleInstance.transform.localPosition = new Vector3(battlePanelRect.rect.width / 2 - 100, 0, 0);

            }
        }
    }

    // 他人移动
    public static void BattleMove(string sid, int[] vecArray) {

        Vector2 po = new Vector2(vecArray[0], vecArray[1]);

        GameObject go = GameObject.Find(sid);

        RoleScript roleScript = go.GetComponent<RoleScript>();

        roleScript.roleState.isMoving = true;

        if (go == null) {

            Debug.LogWarning("找不到对象 :" + sid);
            
            return;

        }

        go.transform.localPosition = po;

    }

    // 他人取消移动
    public static void CancelMove(string sid) {

        GameObject go = GameObject.Find(sid);

        RoleScript roleScript = go.GetComponent<RoleScript>();

        roleScript.roleState.isMoving = false;

        Debug.Log(sid + " : 取消了移动");

    }
    
}
