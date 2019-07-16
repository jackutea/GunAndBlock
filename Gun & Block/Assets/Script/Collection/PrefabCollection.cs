using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

class PrefabCollection : MonoBehaviour {

    public static PrefabCollection instance;
    public GameObject alertWindow = null;

    public GameObject oneRolePanelPrefab = null;

    public GameObject serverPanelPrefab = null;
    public GameObject rolePrefab = null;
    public GameObject bulletPrefab = null;

    

    void Awake() {

        if (instance == null) instance = this;
        
    }
}