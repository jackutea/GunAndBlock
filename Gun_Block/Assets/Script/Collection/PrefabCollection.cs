using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

class PrefabCollection : MonoBehaviour {

    public static PrefabCollection instance;
    public GameObject alertWindow = null;
    public GameObject gameOverWindow = null;
    public GameObject compareWindow = null;

    public GameObject oneRolePanelPrefab = null;

    public GameObject serverPanelPrefab = null;

    public GameObject roadLine = null;

    public GameObject tower = null;

    public Slider skillSlider = null;

    public GameObject rolePrefab = null;
    public GameObject block = null;
    public GameObject normalBullet = null;
    public GameObject slowBullet = null;
    public GameObject fastBullet = null;
    public GameObject rayLight = null;
    public GameObject blockWall = null;
    public GameObject shield = null;
    public GameObject shadow = null;

    public static Dictionary<int, GameObject> skillPrefabDic;

    void Awake() {

        if (instance == null) instance = this;

    }

    void Start() {

        initSkillPrefab();

    }

    void initSkillPrefab() {

        skillPrefabDic = new Dictionary<int, GameObject>();

        skillPrefabDic.Add((int)SkillEnum.block, block);

        skillPrefabDic.Add((int)SkillEnum.normalBullet, normalBullet);

        skillPrefabDic.Add((int)SkillEnum.slowBullet, slowBullet);

        skillPrefabDic.Add((int)SkillEnum.fastBullet, fastBullet);

        skillPrefabDic.Add((int)SkillEnum.rayLight, rayLight);

        skillPrefabDic.Add((int)SkillEnum.blockWall, blockWall);

        skillPrefabDic.Add((int)SkillEnum.shield, shield);

        skillPrefabDic.Add((int)SkillEnum.shadow, shadow);

    }
}