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

        // registerKeyAct();

        roleCollider = roleInstance.GetComponent<Collider>();

        roleSpriteRenderer = roleInstance.GetComponentInChildren<SpriteRenderer>();

        // Debug.Log("roleCollider : " + roleCollider + "bounds : " + roleCollider.bounds);

    }

    void Update() {

        aniCheck(); // 动画更替

        if (!isMe) return;

        if (!roleState.isDead) {

            inputKeyboardCheck();
        
            inputMouseCheck();

        }
    }

    void FixedUpdate() {

        socketSendGap += Time.deltaTime;

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

            if (Input.GetKey(KeyCode.A)) {

                move(KeyCode.A, KeyCode.W);

                return;

            } else if (Input.GetKey(KeyCode.D)) {

                move(KeyCode.D, KeyCode.W);

                return;

            }

            move(KeyCode.None, KeyCode.W);

        }

        if (Input.GetKey(KeyCode.S)) {

            if (Input.GetKey(KeyCode.A)) {

                move(KeyCode.A, KeyCode.S);

                return;

            } else if (Input.GetKey(KeyCode.D)) {

                move(KeyCode.D, KeyCode.S);

                return;

            }

            move(KeyCode.None, KeyCode.S);

        }

        if (Input.GetKey(KeyCode.A)) {

            if (Input.GetKey(KeyCode.W)) {

                move(KeyCode.A, KeyCode.W);

                return;

            } else if (Input.GetKey(KeyCode.S)) {

                move(KeyCode.A, KeyCode.S);

                return;

            }

            move(KeyCode.A, KeyCode.None);

        }

        if (Input.GetKey(KeyCode.D)) {

            if (Input.GetKey(KeyCode.W)) {

                move(KeyCode.D, KeyCode.W);

                return;

            } else if (Input.GetKey(KeyCode.S)) {

                move(KeyCode.D, KeyCode.S);

                return;

            }

            move(KeyCode.D, KeyCode.None);
            
        }

        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D)) {

            roleState.isMoving = false;

            CuteUDPManager.cuteUDP.emitServer("CancelMove", "");

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
    // void registerKeyAct() {

    //     keyActDic = new Dictionary<KeyCode, Action<KeyCode>>();

    //     keyActDic.Add(KeyCode.W, move);

    //     keyActDic.Add(KeyCode.S, move);

    //     keyActDic.Add(KeyCode.A, move);

    //     keyActDic.Add(KeyCode.D, move);

    //     keyActDic.Add(KeyCode.Mouse0, shoot);

    //     keyActDic.Add(KeyCode.Mouse1, block);

    // }

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
    void move(KeyCode xkey = KeyCode.None, KeyCode ykey = KeyCode.None) {

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

        Vector2 po = roleInstance.transform.localPosition;

        if (ykey == KeyCode.W) {

            if (po.y + colHeight <= yWall) {

                roleInstance.transform.Translate(Vector2.up * roleState.moveSpeed * Time.deltaTime);

            }

        }

        if (ykey == KeyCode.S) {

            if (po.y - colHeight >= - yWall) {

                roleInstance.transform.Translate(Vector2.down * roleState.moveSpeed * Time.deltaTime);
                
            }
        }

        if (xkey == KeyCode.A) {

            if (roleState.isLeftAlly == false) {

                if (po.x - colWidth >= 0) {
                
                    roleInstance.transform.Translate(Vector2.left * roleState.moveSpeed * Time.deltaTime);

                }

            } else {

                if (po.x - colWidth >= - xWall) {
                
                    roleInstance.transform.Translate(Vector2.left * roleState.moveSpeed * Time.deltaTime);

                }
            }
        }

        if (xkey == KeyCode.D) {

            if (roleState.isLeftAlly == true) {

                if (po.x + colWidth <= 0) {

                    roleInstance.transform.Translate(Vector2.right * roleState.moveSpeed * Time.deltaTime);

                }

            } else {

                if (po.x + colWidth <= xWall) {

                    roleInstance.transform.Translate(Vector2.right * roleState.moveSpeed * Time.deltaTime);

                }
            }
        }

        Vector2 currentPo = roleInstance.transform.localPosition;

        int pox = (int)Mathf.Floor(currentPo.x * 1000) / 1000;

        Debug.Log(pox);

        int poy = (int)Mathf.Floor(currentPo.y * 1000) / 1000;

        int[] intPo = {pox, poy};

        Debug.Log(intPo[0]);

        if (socketSendGap > Time.deltaTime * 1.5f) {

            MoveInfo moveInfo = new MoveInfo(intPo);

            moveInfo.d = CuteUDPApp.CuteUDP.socketId;

            string moveInfoString = JsonUtility.ToJson(moveInfo);

            Debug.LogWarning(moveInfoString);

            CuteUDPManager.cuteUDP.emitServer("BattleMove", moveInfoString);

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
