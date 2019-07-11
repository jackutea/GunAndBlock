using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

class PrefabCollection : MonoBehaviour {

    public static PrefabCollection instance;
    public GameObject alertWindow;

    public GameObject rolePrefab;
    public GameObject bulletPrefab;

    void Awake() {

        if (instance == null) instance = this;
        
    }
}