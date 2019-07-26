using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

class PrefabCollection : MonoBehaviour {

    public static PrefabCollection instance;
    public GameObject alertWindow = null;

    public GameObject oneRolePanelPrefab = null;

    public GameObject serverPanelPrefab = null;

    public GameObject middleWall = null;
    public GameObject roadLine = null;

    public GameObject tower = null;

    public GameObject rolePrefab = null;
    public GameObject normalBullet = null;
    public GameObject slowBullet = null;
    public GameObject fastBullet = null;
    public GameObject rayLight = null;
    public GameObject blockWall = null;
    public GameObject shield = null;
    public GameObject shadow = null;

    void Awake() {

        if (instance == null) instance = this;
        
    }
}