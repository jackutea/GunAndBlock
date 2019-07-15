using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HomeScript : MonoBehaviour {

    // 匹配面板
    public Button soloCompareBtn;
    public Button teamCompareBtn;
    public Button raidCompareBtn;
    public Button customRoomBtn;

    // 角色信息面板
    public Text username;
    public Text roleName;
    public Text level;
    public Text exp;
    public Text score;
    public Text life;
    public Text blockLife;
    public Text damage;
    public Text shootGap;
    public Text blockGap;
    public Text perfectBlockLast;
    public Text moveSpeed;
    public Text bulletSpeed;

    void Awake() {

        if (GameObject.FindWithTag("MainScript") == null) {

            SceneManager.LoadScene("InitGame");

            return;

        }
    }

    void Start() {
        
    }

    void Update() {
        
    }


}
