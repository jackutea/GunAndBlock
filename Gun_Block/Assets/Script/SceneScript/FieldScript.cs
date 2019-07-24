using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FieldScript : MonoBehaviour {

    public static FieldScript instance;
    public Camera mainCamera;
    public GameObject battlePanel;
    RectTransform battlePanelRect;
    public float scaleSpeed;

    public FieldInfo fieldInfo;
    public int fieldId;
    public int modeCode;
    public Dictionary<string, RoleState> sidJson;
    public static Dictionary<string, GameObject> roleInstanceDic;

    void Awake() {

        if (GameObject.FindWithTag("MainScript") == null) {

            SceneManager.LoadScene("InitGame");

        }
    }

    void Start() {

        if (instance == null) instance = this;

        fieldInfo = PlayerDataScript.FIELD_INFO;

        fieldId = fieldInfo.fieldId;

        modeCode = fieldInfo.modeCode;

        sidJson = fieldInfo.sidJson;

        roleInstanceDic = new Dictionary<string, GameObject>();

        battlePanelRect = GetComponent<RectTransform>();

        scaleSpeed = 0.5f;

        born();

    }

    void Update() {

    }

    // 生成角色 me & other
    void born() {

        List<string> sidList = new List<string>(sidJson.Keys);

        for (int i = 0; i < sidJson.Count; i += 1) {
            
            string sid = sidList[i];

            RoleState role = sidJson[sid];

            GameObject roleObj = Instantiate(PrefabCollection.instance.rolePrefab, battlePanel.transform);

            roleObj.name = sid;

            RoleScript roleScript = roleObj.GetComponentInChildren<RoleScript>();

            roleScript.roleState = role;

            roleScript.isMe = (role.roleName == PlayerDataScript.ROLE_STATE.roleName) ? true : false;

            roleScript.roleInstance = roleObj;

            roleScript.battlePanel = battlePanel;

            roleScript.battlePanelRect = battlePanelRect;

            SpriteRenderer roleRenderer = roleScript.roleInstance.GetComponentInChildren<SpriteRenderer>();

            if(roleScript.roleState.isLeftAlly) {

                roleRenderer.flipY = false;

                roleScript.roleInstance.transform.localPosition = new Vector3(-battlePanelRect.rect.width / 2 + 100, 0, 0);

            } else {

                roleRenderer.flipY = true;

                roleScript.roleInstance.transform.localPosition = new Vector3(battlePanelRect.rect.width / 2 - 100, 0, 0);

            }
        }
    }

    // 有人移动
    public static void Move(string sid, int[] vecArray) {

        Vector2 po = new Vector2(vecArray[0], vecArray[1]);

        Debug.Log("移动" + sid);

        GameObject go = GameObject.Find(sid);

        RoleScript roleScript = go.GetComponent<RoleScript>();

        roleScript.roleState.isMoving = true;

        if (go != null) {

            go.transform.localPosition = po;

            
        }
    }

    // 有人取消移动
    public static void CancelMove(string sid) {

        GameObject go = GameObject.Find(sid);

        RoleScript roleScript = go.GetComponent<RoleScript>();

        roleScript.roleState.isMoving = false;

        // Debug.Log(sid + " : 取消了移动");

    }

    // 有人射击
    public static void Shoot(BulletInfo bulletInfo) {

        string sid = bulletInfo.sid;

        Debug.Log("射击" + sid);

        GameObject go = GameObject.Find(sid);

        RoleScript roleScript = go.GetComponent<RoleScript>();

        roleScript.shootBullet(bulletInfo);

        // Debug.Log("对方子弹速度" + bulletInfo.shootSpeed);

    }

    // 有人格挡
    public static void Block(string sid) {

        GameObject go = GameObject.Find(sid);

        RoleScript roleScript = go.GetComponent<RoleScript>();

        roleScript.roleState.isBlocking = true;
    }

    // 有人取消格挡
    public static void CancelBlock(string sid) {

        GameObject go = GameObject.Find(sid);

        RoleScript roleScript = go.GetComponent<RoleScript>();

        roleScript.roleState.isBlocking = false;
    }

    // 有人完美格挡
    public static void PerfectBlock(string sid) {

        GameObject go = GameObject.Find(sid);

        RoleScript roleScript = go.GetComponent<RoleScript>();

        roleScript.roleState.isPerfectBlocking = true;
    }

    // 有人取消完美格挡
    public static void CancelPerfectBlock(string sid) {

        GameObject go = GameObject.Find(sid);

        RoleScript roleScript = go.GetComponent<RoleScript>();

        roleScript.roleState.isPerfectBlocking = false;
    }

    // 有人完美格挡了子弹
    public static void PerfectBlockBullet(BulletInfo bulletInfo) {

        GameObject bulletGo = GameObject.Find(bulletInfo.bid);

        if (bulletGo != null) Destroy(bulletGo);

        string sid = bulletInfo.sid;

        BulletInfo bi = new BulletInfo(sid);

        bi.dmg = bulletInfo.dmg;

        bi.shootSpeed = bulletInfo.shootSpeed;

        bi.direct = bulletInfo.direct;

        GameObject roleGo = GameObject.Find(sid);

        Debug.Log(roleGo.name);

        RoleScript roleScript = roleGo.GetComponent<RoleScript>();

        roleScript.shootBullet(bi);

    }
    
    // 有人普通格挡了子弹
    public static void BlockBullet(BulletInfo bulletInfo) {

        GameObject bulletGo = GameObject.Find(bulletInfo.bid);

        if (bulletGo != null) Destroy(bulletGo);

        GameObject roleGo = GameObject.Find(bulletInfo.sid);

        RoleScript roleScript = roleGo.GetComponent<RoleScript>();

        roleScript.roleState.blockLife -= bulletInfo.dmg;

        if (roleScript.roleState.blockLife <= 0) {

            roleScript.roleState.life -= bulletInfo.dmg;

        }
    }

    // 有人被子弹直接击中
    public static void BeAttacked(BulletInfo bulletInfo) {

        GameObject bulletGo = GameObject.Find(bulletInfo.bid);

        if (bulletGo != null) Destroy(bulletGo);

        GameObject roleGo = GameObject.Find(bulletInfo.sid);

        RoleScript roleScript = roleGo.GetComponent<RoleScript>();

        roleScript.roleState.life -= bulletInfo.dmg;

        // Debug.Log(roleScript.roleState.roleName + "的剩余生命值" + roleScript.roleState.life);

    }

    // 有人挂菜
    public static void Dead(string sid) {

        GameObject go = GameObject.Find(sid);

        RoleScript roleScript = go.GetComponent<RoleScript>();

        roleScript.roleState.isDead = true;

        roleScript.roleSpriteRenderer.sortingOrder = 1;

    }
}
