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
    public RoleState[] roleStateArray;
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

        roleStateArray = fieldInfo.roleArray;

        roleInstanceDic = new Dictionary<string, GameObject>();

        battlePanelRect = GetComponent<RectTransform>();

        scaleSpeed = 0.5f;

        born();

    }

    void Update() {

    }

    // 生成角色 me & other
    void born() {

        for (int i = 0; i < roleStateArray.Length; i += 1) {

            RoleState role = roleStateArray[i];

            GameObject roleObj = Instantiate(PrefabCollection.instance.rolePrefab, battlePanel.transform);

            roleObj.name = role.sid;

            RoleScript roleScript = roleObj.GetComponentInChildren<RoleScript>();

            roleScript.index = i;

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

    // 触发移动更新
    public static void BattleMove(string sid, float[] vecArray) {

        Vector3 po = new Vector3(vecArray[0], vecArray[1], vecArray[2]);

        GameObject go = GameObject.Find(sid);

        if (go == null) {

            Debug.LogWarning("找不到对象 :" + sid);
            
            Debug.LogWarning("将要移动到的坐标是 :" + po);

            return;

        }

        go.transform.localPosition = po;

        Debug.LogWarning("触发了移动");

    }
    
}
