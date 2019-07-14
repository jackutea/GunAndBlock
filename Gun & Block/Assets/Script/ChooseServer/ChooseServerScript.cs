using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChooseServerScript : MonoBehaviour {

    public GameObject HUDPanel;

    void Awake() {

        if (GameObject.FindWithTag("MainScript") == null) {

            SceneManager.LoadScene("InitGame");

            return;

        }
    }

    void Start() {

        Debug.LogWarning("Start");

        int[] serverIdList = ServerDataScript.serverIdList;

        int[] severUserCountList = ServerDataScript.serverUserCountList;

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

            if (i < 3) oneServer.transform.localPosition = new Vector3(originPo.x + i * xDis, originPo.y, originPo.z);

            if (3 <= i && i < 6) oneServer.transform.localPosition = new Vector3(originPo.x + (i - 3) * xDis, originPo.y + yDis, originPo.z);
            
            if (6 <= i && i < 9) oneServer.transform.localPosition = new Vector3(originPo.x + (i - 6) * xDis, originPo.y + yDis * 2, originPo.z);

        }

        .明天写生成房间
        
    }

    void Update() {
        
    }

    // 显示服务器列表
    void showServers(int num) {


    }

    // 显示房间列表
    void showRooms(int num) {

    }
}
