using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChooseRoomScript : MonoBehaviour {

    void Awake() {

        if (GameObject.FindWithTag("MainScript") == null) {

            SceneManager.LoadScene("InitGame");

            return;

        }

    }

    void Start() {

        CuteUDPManager.cuteUDP.emitServer("ShowRoom", ServerDataScript.choosenServerId.ToString());
        
    }

    void Update() {
        
    }
}
