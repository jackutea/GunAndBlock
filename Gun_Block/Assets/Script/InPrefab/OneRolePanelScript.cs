using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OneRolePanelScript : MonoBehaviour {

    public GameObject rolePanel;
    public Button roleButton;
    public Text roleName;
    public Text level;
    public Button enterGameButton;
    public Button deleteRoleButton;

    void Start() {

        enterGameButton.gameObject.SetActive(false);

        deleteRoleButton.gameObject.SetActive(false);

        // 选中角色
        roleButton.onClick.AddListener(() => {

            PlayerDataScript.ROLE_NAME = roleName.text;

            PlayerDataScript.ROLE_STATE = PlayerDataScript.ROLES[roleName.text];

            // 选中一个角色，取消其他角色的“进入游戏选项”
            GameObject[] btns = GameObject.FindGameObjectsWithTag("RoleSiblingsButton");

            for (int i = 0; i < btns.Length; i += 1) {

                btns[i].SetActive(false);

            }

            enterGameButton.gameObject.SetActive(true);

            deleteRoleButton.gameObject.SetActive(true);

        });

        // 进入游戏
        enterGameButton.onClick.AddListener(() => {

            RoleState roleState = PlayerDataScript.ROLE_STATE;

            string dataString = JsonUtility.ToJson(roleState);

            CuteUDPManager.cuteUDP.emitServer("EnterGame", dataString);

        });

        // 删除角色
        deleteRoleButton.onClick.AddListener(() => {

            string roleName = PlayerDataScript.ROLE_STATE.roleName;

            CuteUDPManager.cuteUDP.emitServer("DeleteRole", roleName);

        });
        
    }

    void Update() {
        
    }
}
