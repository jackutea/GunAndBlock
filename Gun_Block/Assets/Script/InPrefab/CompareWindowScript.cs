using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompareWindowScript : MonoBehaviour {

    public string modeCode;
    public Button cancelCompareBtn;

    void Start() {

        cancelCompareBtn.onClick.AddListener(() => {

            CuteUDPManager.cuteUDP.emitServer("CancelCompare", modeCode);

            Destroy(gameObject);

        });  
    }

}
