using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

class PrefabCollection : MonoBehaviour {

    public static PrefabCollection instance;
    public GameObject alertWindow;

    void Start() {

        instance = this;
        
    }
}