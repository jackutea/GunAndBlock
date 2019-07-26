using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class RoleScript : MonoBehaviour {

    public GameObject roleInstance;
    SpriteRenderer roleSpriteRenderer;
    public RoleState roleState;
    Collider roleCollider;
    Animator roleAni;

    public bool isMe = false;

    public Text nameBar;
    public Slider hpSlider;
    public Slider blockSlider;

    void Awake() {

        roleState = new RoleState();

        roleState.username = PlayerDataScript.USER_NAME;

        roleAni = GetComponent<Animator>();

    }

    void Start() {

        roleCollider = roleInstance.GetComponent<Collider>();

        roleSpriteRenderer = roleInstance.GetComponentInChildren<SpriteRenderer>();

    }

    void Update() {

        aniCheck(); // 动画更替

        barCheck(); // 生命值显示

        timeReduce(); // CD与BUFF持续时间计算

        if (!isMe) return;

        if (!roleState.isDead) {

            inputKeyCheck();

        }
    }

    void FixedUpdate() {

    }

    // 按键监测 Update
    void inputKeyCheck() {
        
    }

    void timeReduce() {

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

    // 动画设置
    void aniCheck() {

    }


    // 死亡
    public void dead() {

        roleState.isDead = true;

        roleSpriteRenderer.sortingOrder = 1;

        CuteUDPManager.cuteUDP.emitServer("Dead", "0");

    }

    // 子弹碰撞检测
    void OnTriggerEnter(Collider col) {

        BulletScript bs = col.gameObject.GetComponent<BulletScript>();

        // 我，是左边，碰到了右边来的子弹
        if (col.tag == "RightAllyBullet" && roleState.isLeftAlly && isMe) {


        // 我，是右边，碰到了左边来的子弹
        } else if (col.tag == "LeftAllyBullet" && !roleState.isLeftAlly && isMe) {
            
        }
    }
}
