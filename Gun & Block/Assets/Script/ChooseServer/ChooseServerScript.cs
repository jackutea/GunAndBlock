using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseServerScript : MonoBehaviour {

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
