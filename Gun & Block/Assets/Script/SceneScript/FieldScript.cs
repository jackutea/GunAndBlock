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

    void Awake() {

        if (GameObject.FindWithTag("MainScript") == null) {

            SceneManager.LoadScene("InitGame");

        }
    }

    void Start() {

        if (instance == null) instance = this;

        battlePanelRect = GetComponent<RectTransform>();

        scaleSpeed = 0.5f;

        born();
    }

    void Update() {

    }

    void FixedUpdate() {

        scaleCircle();

    }

    void OnGUI() {
        
        // inputKeyboardCheck();
        
    }

    // 出生
    void born() {

        GameObject roleInstance = Instantiate(PrefabCollection.instance.rolePrefab, battlePanel.transform);

        RoleScript roleScript = roleInstance.GetComponentInChildren<RoleScript>();

        roleScript.roleInstance = roleInstance;

        roleScript.battlePanel = battlePanel;

        roleScript.battlePanelRect = battlePanelRect;

        SpriteRenderer roleRenderer = roleInstance.GetComponentInChildren<SpriteRenderer>();

        Debug.Log(roleRenderer);

        if(roleScript.roleState.isLeftAlly) {

            roleRenderer.flipY = false;

            roleInstance.transform.localPosition = new Vector3(-battlePanelRect.rect.width / 2 + 100, 0, 0);

        } else {

            roleRenderer.flipY = true;

            roleInstance.transform.localPosition = new Vector3(battlePanelRect.rect.width / 2 - 100, 0, 0);

        }
    }

    // 缩圈
    void scaleCircle() {

        if (battlePanelRect.rect.width > 400) {

            battlePanelRect.sizeDelta = new Vector2(battlePanelRect.rect.width - scaleSpeed, battlePanelRect.rect.height);

        }
        
        if (battlePanelRect.rect.height > 70) {

            battlePanelRect.sizeDelta = new Vector2(battlePanelRect.rect.width, battlePanelRect.rect.height - scaleSpeed);

        }
    }

}
