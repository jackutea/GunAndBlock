﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RoleListScript : MonoBehaviour {

    public GameObject roleListPanel;
    public Button createButton;
    public Button backServerButton;

    public GameObject createRolePanel;
    public Text newRoleName;
    public Button confirmButton;
    public Button cancelButton;

    void Awake() {

        if (GameObject.FindWithTag("MainScript") == null) {

            SceneManager.LoadScene("InitGame");

            return;

        }
    }

    void Start() {

        // 默认隐藏创建角色面板
        createRolePanel.SetActive(false);

        // 当角色数过多时，隐藏创建按钮
        if (PlayerDataScript.ROLES != null && PlayerDataScript.ROLES.Count >= 5) {

            createButton.gameObject.SetActive(false);
            
        }

        // 显示所有角色
        showAllRole();

        // 创建角色按钮
        createButton.onClick.AddListener(() => {

            createRolePanel.SetActive(true);

        });

        // 返回服务器选择按钮
        backServerButton.onClick.AddListener(() => {

            SceneManager.LoadScene("ChooseServer");

        });

        // 确认创建按钮
        confirmButton.onClick.AddListener(() => {

            if (newRoleName.text != "" && newRoleName.text != null) {

                CreateRoleSendInfo roleSendInfo = new CreateRoleSendInfo();

                roleSendInfo.roleName = newRoleName.text;

                roleSendInfo.serverId = ServerDataScript.choosenServerId;

                string dataString = JsonUtility.ToJson(roleSendInfo);

                CuteUDPManager.cuteUDP.emitServer("CreateRole", dataString);

            } else {

                CuteUDPEvent.showAlertWindow("请输入角色名");

            }
        });

        // 取消创建按钮
        cancelButton.onClick.AddListener(() => {

            createRolePanel.SetActive(false);

        });
    }

    void showAllRole() {

        if (PlayerDataScript.ROLES == null) return;

        List<RoleState> roles = PlayerDataScript.ROLES;

        for (int i = 0; i < roles.Count; i += 1) {

            GameObject oneRole = Instantiate(PrefabCollection.instance.oneRolePanelPrefab, roleListPanel.transform);

            Vector3 originPo = oneRole.transform.localPosition;

            oneRole.transform.localPosition = new Vector3(originPo.x, originPo.y - i * 100, originPo.z);

            OneRolePanelScript oneRoleScript = oneRole.GetComponentInChildren<OneRolePanelScript>();

            oneRoleScript.roleIndex = i;
            
            oneRoleScript.roleName.text = roles[i].roleName;

            oneRoleScript.level.text = "Lv " + roles[i].level.ToString();

        }
    }

    void Update() {
        
    }
}