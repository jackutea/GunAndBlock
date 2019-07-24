using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class RoleScript : MonoBehaviour {

    public GameObject battlePanel;
    public RectTransform battlePanelRect;

    public GameObject roleInstance;
    public SpriteRenderer roleSpriteRenderer;
    public RoleState roleState;
    public Collider roleCollider;
    public Animator roleAni;

    public bool isMe = false;

    public float currentX;
    public float currentY;

    public Slider hpSlider;
    public Slider blockSlider;

    public Dictionary<KeyCode, Action<KeyCode>> keyActDic;

    public float socketSendGap;

    void Awake() {

        roleState = new RoleState();

        roleState.username = PlayerDataScript.USER_NAME;

        roleAni = GetComponent<Animator>();

    }

    void Start() {

        socketSendGap = Time.deltaTime * 1.5f;

        // registerKeyAct();

        roleCollider = roleInstance.GetComponent<Collider>();

        roleSpriteRenderer = roleInstance.GetComponentInChildren<SpriteRenderer>();

        // Debug.Log("roleCollider : " + roleCollider + "bounds : " + roleCollider.bounds);

    }

    void Update() {

        aniCheck(); // 动画更替

        timeReduce();

        barCheck(); // 生命值显示

        if (!isMe) return;

        if (!roleState.isDead) {

            inputKeyCheck();

        }
    }

    // 按键监测 Update
    void inputKeyCheck() {

        // 开始格挡
        if (Input.GetKeyDown(KeyCode.Mouse1)) {

            block(KeyCode.Mouse1);

        // 发射一次
        } else if (Input.GetKeyDown(KeyCode.Mouse0)) {

            shoot();
        
        } 

        // 取消格挡
        if (Input.GetKeyUp(KeyCode.Mouse1)) {

            block(KeyCode.Mouse1);

        }

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

        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) {

            if (roleState.isMoving) {

                roleState.isMoving = false;

                CuteUDPManager.cuteUDP.emitServer("CancelMove", "");

            }
        }
    }

    void timeReduce() {

        socketSendGap -= Time.deltaTime;

        if (roleState.shootGap > 0) {

            roleState.shootGap -= Time.deltaTime;

        }

        if (roleState.blockGap > 0) {

            roleState.blockGap -= Time.deltaTime;

        }

        if (roleState.perfectBlockLast > 0) {

            roleState.perfectBlockLast -= Time.deltaTime;

        } else {

            if (roleState.isPerfectBlocking == true) {

                CuteUDPManager.cuteUDP.emitServer("CancelPerfectBlock", "");

            }

            roleState.isPerfectBlocking = false;
        }
    }

    void barCheck() {

        float fullHp = roleState.lifeOrigin;

        float nowHp = roleState.life;

        hpSlider.direction = Slider.Direction.LeftToRight;

        hpSlider.minValue = 0;

        hpSlider.maxValue = fullHp;

        hpSlider.value = nowHp;

        float fullBlockLife = roleState.blockLifeOrigin;

        float nowBlockLife = roleState.blockLife;

        blockSlider.direction = Slider.Direction.LeftToRight;

        blockSlider.minValue = 0;

        blockSlider.maxValue = fullBlockLife;

        blockSlider.value = nowBlockLife;

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

                if (po.x + colWidth >= 0) {
                
                    roleInstance.transform.Translate(Vector2.left * roleState.moveSpeed * Time.deltaTime);

                }

            } else {

                if (po.x + colWidth >= - xWall) {
                
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

        int pox = (int)currentPo.x;

        int poy = (int)currentPo.y;

        int[] intPo = {pox, poy};

        if (socketSendGap <= 0) {

            MoveInfo moveInfo = new MoveInfo(intPo);

            moveInfo.d = CuteUDPApp.CuteUDP.socketId;

            string moveInfoString = JsonUtility.ToJson(moveInfo);

            // Debug.LogWarning(moveInfoString);

            CuteUDPManager.cuteUDP.emitServer("Move", moveInfoString);

        }
    }

    // 射击
    void shoot() {

        if (roleState.shootGap > 0) {

            // Debug.Log("shootGap : " + roleState.shootGap);

            return;
            
        }

        if (!roleState.isBlocking) {

            shootBullet();

            roleState.shootGap = roleState.shootGapOrigin;

        }
    }

    // 生成子弹
    public void shootBullet(BulletInfo bulletInfo = null) {

        GameObject bulletInstance = GameObject.Instantiate(PrefabCollection.instance.bulletPrefab, transform.position, transform.rotation, battlePanel.transform);

        BulletScript bulletScript = bulletInstance.GetComponent<BulletScript>();

        if (bulletInfo == null) {

            bulletScript.bulletInfo = new BulletInfo(PlayerDataScript.sid);

            bulletInstance.name = bulletScript.bulletInfo.bid;

            bulletScript.bulletInfo.shootSpeed = roleState.shootSpeed;

            bulletScript.bulletInfo.dmg = roleState.damage;

            if (roleState.isLeftAlly) {

                bulletScript.bulletInfo.direct = 1;

                bulletInstance.tag = "LeftAllyBullet";

            } else {

                bulletScript.bulletInfo.direct = -1;

                bulletInstance.tag = "RightAllyBullet";

            }

            // Debug.Log("我发射子弹，速度" + bulletScript.bulletInfo.shootSpeed);

            string dataString = JsonConvert.SerializeObject(bulletScript.bulletInfo);

            CuteUDPManager.cuteUDP.emitServer("Shoot", dataString);

        } else {

            bulletScript.bulletInfo = bulletInfo;

            bulletInstance.name = bulletScript.bulletInfo.bid;

            if (bulletInfo.direct == 1) {

                bulletInstance.tag = "LeftAllyBullet";

            } else {

                bulletInstance.tag = "RightAllyBullet";

            }

            // Debug.Log("他人发射子弹，速度:" + bulletScript.bulletInfo.shootSpeed);

        }
    }

    // 格挡
    void block(KeyCode key) {

        if (roleState.blockGap <= 0) {

            if (Input.GetKeyDown(key)) {

                if (roleState.isBlocking == false) {

                    if (roleState.perfectBlockLast <= 0) {

                        roleState.isPerfectBlocking = true;

                        roleState.perfectBlockLast = roleState.perfectBlockLastOrigin;

                        CuteUDPManager.cuteUDP.emitServer("PerfectBlock", "");

                    }

                    roleState.isBlocking = true;

                    CuteUDPManager.cuteUDP.emitServer("Block", "");

                }
            }

            if (Input.GetKeyUp(key)) {

                if (roleState.isBlocking) {

                    roleState.isBlocking = false;

                    roleState.blockGap = roleState.blockGapOrigin;

                    CuteUDPManager.cuteUDP.emitServer("CancelBlock", "");

                }
            }
        }
    }

    // 死亡
    public void dead() {

        roleState.isDead = true;

        roleSpriteRenderer.sortingOrder = 1;

        CuteUDPManager.cuteUDP.emitServer("Dead", "");

    }

    // 子弹碰撞检测
    void OnTriggerEnter(Collider col) {

        BulletScript bs = col.gameObject.GetComponent<BulletScript>();

        // 我，是左边，碰到了右边来的子弹
        if (col.tag == "RightAllyBullet" && roleState.isLeftAlly && isMe) {

            // Debug.Log("碰到了右边来的子弹");

            beAttacked(bs, col);

        // 我，是右边，碰到了左边来的子弹
        } else if (col.tag == "LeftAllyBullet" && !roleState.isLeftAlly && isMe) {

            // Debug.Log("碰到了左边来的子弹");

            beAttacked(bs, col);
            
        }
    }

    // 被子弹射中 子弹id 为 bs.bid
    void beAttacked(BulletScript bs, Collider col) {

        if (roleState.isDead == false) {

            if (roleState.isPerfectBlocking) {

                // Debug.Log("完美格挡了" + col.gameObject);

                bs.bulletInfo.bePerfectBlock();

                bs.gameObject.tag = (col.tag == "LeftAllyBullet") ? "RightAllyBullet" : "LeftAllyBullet";

                string dataString = JsonConvert.SerializeObject(bs.bulletInfo);

                CuteUDPManager.cuteUDP.emitServer("PerfectBlockBullet", dataString);

            } else if (roleState.isBlocking && !roleState.isPerfectBlocking) {

                // Debug.Log("格挡了" + col.gameObject);

                roleState.blockLife -= bs.bulletInfo.dmg;

                if (roleState.blockLife <= 0) {

                    roleState.life -= bs.bulletInfo.dmg;

                    if (roleState.life <= 0) dead(); // awsl

                }

                Destroy(col.gameObject);

                string dataString = JsonConvert.SerializeObject(bs.bulletInfo);

                CuteUDPManager.cuteUDP.emitServer("BlockBullet", dataString);

            } else if (!roleState.isBlocking) {

                // Debug.Log("被" + col.gameObject + "射中了");

                roleState.life -= bs.bulletInfo.dmg;

                if (roleState.life <= 0) dead(); // awsl

                // Debug.Log(roleState.roleName + "（我）的剩余生命值" + roleState.life);

                Destroy(col.gameObject);

                string dataString = JsonConvert.SerializeObject(bs.bulletInfo);

                CuteUDPManager.cuteUDP.emitServer("BeAttacked", dataString);

            }
        }
    }
}
