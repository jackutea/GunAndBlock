using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleScript : MonoBehaviour {

    public GameObject battlePanel;
    public RectTransform battlePanelRect;

    public GameObject roleInstance;
    public SpriteRenderer roleSpriteRenderer;
    public GameObject bulletInstance;
    public RoleState roleState;
    public Collider roleCollider;
    public Animator roleAni;

    public Dictionary<KeyCode, Action<KeyCode>> keyActDic;

    void Awake() {

        roleState = new RoleState();

        roleState.username = PlayerDataScript.USER_NAME;

        roleAni = GetComponent<Animator>();

    }

    void Start() {

        registerKeyAct();

        roleCollider = roleInstance.GetComponent<Collider>();

        roleSpriteRenderer = roleInstance.GetComponentInChildren<SpriteRenderer>();

        // Debug.Log("roleCollider : " + roleCollider + "bounds : " + roleCollider.bounds);

    }

    void Update() {

        if (!roleState.isDead) {

            inputKeyboardCheck();
        
            inputMouseCheck();

            outCircle();

            aniCheck();

        }
    }

    void FixedUpdate() {

        if (roleState.shootGap > 0)

            roleState.shootGap -= Time.fixedDeltaTime;

        if (roleState.blockGap > 0)

            roleState.blockGap -= Time.fixedDeltaTime;

        if (roleState.perfectBlockGap > 0)

            roleState.perfectBlockGap -= Time.fixedDeltaTime;
        
        else

            roleState.isPerfectBlocking = false;

    }

    // 键盘按键监测 Update
    void inputKeyboardCheck() {

        if (Input.GetKey(KeyCode.W)) {

            Action<KeyCode> act = keyActDic[KeyCode.W];

            act.Invoke(KeyCode.W);

        }

        if (Input.GetKey(KeyCode.S)) {

            Action<KeyCode> act = keyActDic[KeyCode.S];

            act.Invoke(KeyCode.S);

        }

        if (Input.GetKey(KeyCode.A)) {

            Action<KeyCode> act = keyActDic[KeyCode.A];

            act.Invoke(KeyCode.A);

        }

        if (Input.GetKey(KeyCode.D)) {

            Action<KeyCode> act = keyActDic[KeyCode.D];

            act.Invoke(KeyCode.D);

        }

        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D)) {

            roleState.isMoving = false;

        }
    }

    // 鼠标按键监测
    void inputMouseCheck() {

        // 开始格挡
        if (Input.GetMouseButton(1)) {

            Action<KeyCode> act = keyActDic[KeyCode.Mouse1];
            
            act.Invoke(KeyCode.Mouse1);

        }

        // 发射一次
        if (Input.GetMouseButtonUp(0)) {

            Action<KeyCode> act = keyActDic[KeyCode.Mouse0];
            
            act.Invoke(KeyCode.Mouse0);

        
        // 取消格档
        } else if (Input.GetMouseButtonUp(1)) {

            Action<KeyCode> act = keyActDic[KeyCode.Mouse1];
            
            act.Invoke(KeyCode.Mouse1);

        }
    }

    // 注册按键事件
    void registerKeyAct() {

        keyActDic = new Dictionary<KeyCode, Action<KeyCode>>();

        keyActDic.Add(KeyCode.W, move);

        keyActDic.Add(KeyCode.S, move);

        keyActDic.Add(KeyCode.A, move);

        keyActDic.Add(KeyCode.D, move);

        keyActDic.Add(KeyCode.Mouse0, shoot);

        keyActDic.Add(KeyCode.Mouse1, block);

    }

    // 动画设置
    void aniCheck() {

        if (roleState.isDead) {

            roleAni.Play("roleDead");

            return;
        }

        if (roleState.isBlocking) {

            if (roleState.isMoving) {

                roleAni.Play("roleBlockMove");

            } else {

                roleAni.Play("roleBlock");

            }

        } else {

            if (roleState.isMoving) {

                roleAni.Play("roleMove");

            } else {

                roleAni.Play("roleStand");

            }
        }
    }

    // 移动
    void move(KeyCode key) {

        roleState.isMoving = true;

        if (roleState.isBlocking) {

            roleState.moveSpeed = roleState.moveSpeedOrigin * 0.65f;

        } else {

            roleState.moveSpeed = roleState.moveSpeedOrigin;
            
        }

        sbyte xDirect = 0;

        sbyte yDirect = 0;

        if (key == KeyCode.W) yDirect = 1;

        if (key == KeyCode.S) yDirect = -1;

        if (key == KeyCode.A) xDirect = -1;

        if (key == KeyCode.D) xDirect = 1;

        Vector3 moveVec = new Vector3(xDirect, yDirect, 0);

        roleInstance.transform.Translate(moveVec * roleState.moveSpeed * Time.deltaTime);

    }

    // 射击
    void shoot(KeyCode key) {

        if (roleState.shootGap > 0) {

            // Debug.Log("shootGap : " + roleState.shootGap);

            return;
            
        }

        if (!roleState.isBlocking) {

            // Debug.Log("Shoot");

            bulletInstance = GameObject.Instantiate(PrefabCollection.instance.bulletPrefab, transform.position, transform.rotation, battlePanel.transform);

            BulletScript bulletScript = bulletInstance.GetComponent<BulletScript>();

            bulletScript.shootSpeed = roleState.shootSpeed;

            bulletScript.isShootToRight = roleState.isLeftAlly;

            if (!bulletScript.isShootToRight) {

                bulletScript.direct = -1;

                this.gameObject.tag = "LeftAllyBullet";

            } else {

                this.gameObject.tag = "RightAllyBullet";

            }

            roleState.shootGap = roleState.shootGapOrigin;

        }
    }

    // 格挡
    void block(KeyCode key) {

        if (roleState.blockGap > 0) {

            // Debug.Log("blockGap : " + roleState.blockGap);

            return;
            
        }

        if (Input.GetKeyDown(key)) {

            if (roleState.perfectBlockGap <= 0)

                roleState.isPerfectBlocking = true;

                roleState.perfectBlockGap = roleState.perfectBlockGapOrigin;

                Debug.Log("触发完美格档");
        }

        if (Input.GetKey(key)) {

            // Debug.Log("正在格档");

            roleState.isBlocking = true;

        } else {

            // Debug.Log("取消格档");

            roleState.isBlocking = false;

            roleState.blockGap = roleState.blockGapOrigin;
        }
    }

    // 被缩圈 检测
    void outCircle() {

        float colWidth = roleCollider.bounds.extents.x * 100;

        float colHeight = roleCollider.bounds.extents.z * 100;
       
        float yWall = battlePanelRect.rect.height / 2;

        float xWall = battlePanelRect.rect.width / 2;

        Vector3 po = roleInstance.transform.localPosition;

        if (po.x - colWidth < - xWall) {

            dead();

        } else if (po.x + colWidth > xWall) {

            dead();

        } else if (po.y - colHeight < - yWall) {

            dead();

        } else if (po.y + colHeight > yWall) {

            dead();

        }
    }

    // 死亡
    void dead() {

        roleState.isDead = true;

        roleSpriteRenderer.sortingOrder = 1;

    }

    // 子弹碰撞检测
    void OnTriggerEnter(Collider col) {

        if (col.tag == "RightAllyBullet") {
            
            if (roleState.isLeftAlly) {

                Debug.Log("l");

                damageCheck(col);

            } else {

                Debug.Log("自己人，别开枪");

            }

        } else if (col.tag == "LeftAllyBullet") {

            if (!roleState.isLeftAlly) {

                Debug.Log("l");

                damageCheck(col);

            } else {

                Debug.Log("自己人，别开枪");

            }
        }
    }

    void damageCheck(Collider col) {

        BulletScript bs = col.gameObject.GetComponent<BulletScript>();

        // Debug.LogAssertion(bs.shootSpeed);
        
        if (roleState.isPerfectBlocking) {

            bs.direct *= -1;

            bs.shootSpeed *= 1.1f;

            bs.gameObject.tag = (col.tag == "LeftAllyBullet") ? "RightAllyBullet" : "LeftAllyBullet";

        } else if (roleState.isBlocking && !roleState.isPerfectBlocking) {

            roleState.blockLife -= 1;

            Destroy(col.gameObject);

        } else if (!roleState.isBlocking) {

            roleState.life -= 1;

            Destroy(col.gameObject);

        }
    }
}
