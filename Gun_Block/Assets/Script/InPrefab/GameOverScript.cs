using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScript : MonoBehaviour {

    public Text resultShowText;

    public Text scoreText;

    public Text expText;

    public Button backButton;

    void Awake() {

        backButton.onClick.AddListener(() => {

            PlayerDataScript.reqState();

        });
    }
    
}
