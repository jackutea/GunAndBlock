using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChooseServerScript : MonoBehaviour {

    public GameObject HUDPanel;

    public Button enterServerBtn;

    public Button backLoginBtn;

    void Awake() {

        if (GameObject.FindWithTag("MainScript") == null) {

            SceneManager.LoadScene("InitGame");

            return;

        }
    }

    void Start() {

        // Debug.LogWarning("Start");

        showServers();

        backLoginBtn.onClick.AddListener(() => {

            PlayerDataScript.USER_NAME = "";

            SceneManager.LoadScene("Login");

        });
        
    }

    void Update() {
        
    }

    // 显示服务器列表
    void showServers() {

        int[] serverIdList = ServerDataScript.serverIdList;

        int[] severUserCountList = ServerDataScript.serverUserCountList;

        int xLine = 0;

        int yLine = 0;

        float xDis = 300f;
        
        float yDis = - 160f;

        for (int i = 0; i < severUserCountList.Length; i += 1) {

            GameObject oneServer = Instantiate(PrefabCollection.instance.serverPanelPrefab, HUDPanel.transform);

            Text[] serverTxt = oneServer.GetComponentsInChildren<Text>();

            Text serverId = serverTxt[0];

            serverId.text = i.ToString() + "区";

            Text serverUserCount = serverTxt[1];

            serverUserCount.text = severUserCountList[i].ToString() + "人";

            Button chooseServerBtn = oneServer.GetComponentInChildren<Button>();

            Text chooseServerId = chooseServerBtn.GetComponentInChildren<Text>();

            chooseServerId.text = i.ToString();

            chooseServerBtn.onClick.AddListener(() => {

                ServerDataScript.choosenServerId = int.Parse(chooseServerId.text);
                
                Debug.Log("已选中服务器 :" + chooseServerId.text);

                Debug.Log(ServerDataScript.choosenServerId);

            });

            Vector3 originPo = oneServer.transform.localPosition;

            oneServer.transform.localPosition = new Vector3(originPo.x + yLine * xDis, originPo.y + xLine * yDis, originPo.z);

            if (yLine < 2) {

                yLine += 1;

            } else {

                yLine = 0;

                xLine += 1;
            }

        }

        enterServerBtn.onClick.AddListener(() => {

            if (ServerDataScript.choosenServerId == -1) {

                Debug.Log("未选中服务器");

            } else {

                SceneManager.LoadScene("Home");
                
            }
        });
        
    }
}
