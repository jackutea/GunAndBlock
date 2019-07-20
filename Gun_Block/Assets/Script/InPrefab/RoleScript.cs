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

    public bool isMe = false;

    public int index; // 在 PlayerDataScript.FIELD_INFO.roleArray内的索引
    public float currentX;
    public float currentY;

    public Dictionary<KeyCode, Action<KeyCode>> keyActDic;

    public float socketSendGap;

    void Awake() {

        roleState = new RoleState();

        roleState.username = PlayerDataScript.USER_NAME;

        roleAni = GetComponent<Animator>();

    }

    void Start() {

        socketSendGap = 0;

        registerKeyAct();

        roleCollider = roleInstance.GetComponent<Collider>();

        roleSpriteRenderer = roleInstance.GetComponentInChildren<SpriteRenderer>();

        // Debug.Log("roleCollider : " + roleCollider + "bounds : " + roleCollider.bounds);

    }

    void Update() {

        socketSendGap += Time.deltaTime;

        aniCheck(); // 动画更替

        if (!isMe) return;

        if (!roleState.isDead) {

            inputKeyboardCheck();
        
            inputMouseCheck();

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

            move(KeyCode.W);

            // Action<KeyCode> act = keyActDic[KeyCode.W];

            // act.Invoke(KeyCode.W);

        }

        if (Input.GetKey(KeyCode.S)) {

            move(KeyCode.S);

            // Action<KeyCode> act = keyActDic[KeyCode.S];

            // act.Invoke(KeyCode.S);

        }

        if (Input.GetKey(KeyCode.A)) {

            move(KeyCode.A);

            // Action<KeyCode> act = keyActDic[KeyCode.A];

            // act.Invoke(KeyCode.A);

        }

        if (Input.GetKey(KeyCode.D)) {

            move(KeyCode.D);

            // Action<KeyCode> act = keyActDic[KeyCode.D];

            // act.Invoke(KeyCode.D);

        }

        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D)) {

            roleState.isMoving = false;

        }
    }

    // 鼠标按键监测
    void inputMouseCheck() {

        // 开始格挡
        if (Input.GetMouseButton(1)) {

            block(KeyCode.Mouse1);

            // Action<KeyCode> act = keyActDic[KeyCode.Mouse1];
            
            // act.Invoke(KeyCode.Mouse1);

        }

        // 发射一次
        if (Input.GetMouseButtonUp(0)) {

            shoot(KeyCode.Mouse0);

            // Action<KeyCode> act = keyActDic[KeyCode.Mouse0];
            
            // act.Invoke(KeyCode.Mouse0);

        
        // 取消格档
        } else if (Input.GetMouseButtonUp(1)) {

            block(KeyCode.Mouse1);

            // Action<KeyCode> act = keyActDic[KeyCode.Mouse1];
            
            // act.Invoke(KeyCode.Mouse1);

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

        // 检测撞墙
        float colWidth = roleCollider.bounds.extents.x * 100;

        float colHeight = roleCollider.bounds.extents.z * 100;
       
        float yWall = battlePanelRect.rect.height / 2;

        float xWall = battlePanelRect.rect.width / 2;

        Vector3 po = roleInstance.transform.localPosition;

        if (key == KeyCode.W) {

            if (po.y + colHeight > yWall) return; // 禁止向上
            
            roleInstance.transform.Translate(Vector3.up * roleState.moveSpeed * Time.deltaTime);

        }

        if (key == KeyCode.S) {

            if (po.y - colHeight < - yWall) return; // 禁止向下

            roleInstance.transform.Translate(Vector3.down * roleState.moveSpeed * Time.deltaTime);

        }

        if (key == KeyCode.A) {

            if (po.x - colWidth < - xWall) return; // 禁止向左

            roleInstance.transform.Translate(Vector3.left * roleState.moveSpeed * Time.deltaTime);
        }

        if (key == KeyCode.D) {

            if (po.x + colWidth > xWall) return; // 禁止向右

            roleInstance.transform.Translate(Vector3.right * roleState.moveSpeed * Time.deltaTime);

        }

        Vector3 currentPo = roleInstance.transform.localPosition;

        float pox = (float)Math.Round(currentPo.x, 3);

        float poy = (float)Math.Round(currentPo.y, 3);

        float poz = (float)Math.Round(currentPo.z, 3);

        float[] floatPo = {pox, poy, poz};

        MoveInfo moveInfo = new MoveInfo();

        moveInfo.sid = CuteUDPApp.CuteUDP.socketId;

        moveInfo.vecArray = floatPo;

        string moveInfoString = JsonUtility.ToJson(moveInfo);

        if (socketSendGap > Time.deltaTime) {

            CuteUDPManager.cuteUDP.emitServer("BattleMove", moveInfoString);

            socketSendGap = 0;

        }
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

            if (bulletScript.isShootToRight) {

                bulletScript.gameObject.tag = "LeftAllyBullet";

            } else {

                bulletScript.direct = -1;

                bulletScript.gameObject.tag = "RightAllyBullet";

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

    // 死亡
    void dead() {

        roleState.isDead = true;

        roleSpriteRenderer.sortingOrder = 1;

    }

    // 子弹碰撞检测
    void OnTriggerEnter(Collider col) {

        BulletScript bs = col.gameObject.GetComponent<BulletScript>();

        // 我，是左边，碰到了右边来的子弹
        if (col.tag == "RightAllyBullet" && roleState.isLeftAlly && isMe) {

            if (col.tag != bs.gameObject.tag) {

                if (roleState.isLeftAlly) {

                    if (roleState.isPerfectBlocking) {

                        Debug.Log(bs.gameObject.tag + "被" + col.gameObject + "完美格挡");

                        bs.direct *= -1;

                        bs.shootSpeed *= 1.1f;

                        bs.gameObject.tag = (col.tag == "LeftAllyBullet") ? "RightAllyBullet" : "LeftAllyBullet";

                    } else if (roleState.isBlocking && !roleState.isPerfectBlocking) {

                        Debug.Log(bs.gameObject.tag + "被" + col.gameObject + "格挡");

                        roleState.blockLife -= 1;

                        Destroy(col.gameObject);

                    } else if (!roleState.isBlocking) {

                        Debug.Log(bs.gameObject.tag + "射中了" + col.gameObject);

                        roleState.life -= 1;

                        if (roleState.life <= 0) dead(); // awsl

                        Destroy(col.gameObject);
                    }
                }
            }

        // 我，是右边，碰到了左边来的子弹
        } else if (col.tag == "LeftAllyBullet" && roleState.isLeftAlly && isMe) {

            if (col.tag != bs.gameObject.tag) {

                if (!roleState.isLeftAlly) {

                    if (roleState.isPerfectBlocking) {

                        Debug.Log(bs.gameObject.tag + "被" + col.gameObject + "完美格挡");

                        bs.direct *= -1;

                        bs.shootSpeed *= 1.1f;

                        bs.gameObject.tag = (col.tag == "LeftAllyBullet") ? "RightAllyBullet" : "LeftAllyBullet";

                    } else if (roleState.isBlocking && !roleState.isPerfectBlocking) {

                        Debug.Log(bs.gameObject.tag + "被" + col.gameObject + "格挡");

                        roleState.blockLife -= 1;

                        Destroy(col.gameObject);

                    } else if (!roleState.isBlocking) {

                        Debug.Log(bs.gameObject.tag + "射中了" + col.gameObject);

                        roleState.life -= 1;

                        if (roleState.life <= 0) dead(); // awsl

                        Destroy(col.gameObject);
                    }
                }
            }
        }
    }
}
