using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class RoleScript : MonoBehaviour {

    Collider roleCollider;
    SpriteRenderer roleSpriteRenderer;
    Animator roleAni;

    public bool isMe = false;

    public Text nameBar;
    public Slider hpSlider;
    public Slider blockSlider;

    List<string> spellList;

    void Awake() {

    }

    void Start() {

        if (PlayerDataScript.ROLE_STATE == null) {

            // 重新请求加载角色数据

            return;

        }

        roleCollider = GetComponent<Collider>();

        roleSpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        roleAni = GetComponent<Animator>();

        nameBar.text = PlayerDataScript.ROLE_STATE.roleName;

        PlayerDataScript.ROLE_STATE.username = PlayerDataScript.USER_NAME;

        spellList = new List<string>();
        spellList.Add("AAA");
        spellList.Add("AAB");
        spellList.Add("ABB");
        spellList.Add("ABA");
        spellList.Add("BBB");
        spellList.Add("BAB");
        spellList.Add("BAA");
        spellList.Add("BBA");

    }

    void Update() {

        aniCheck(); // 动画更替

        barCheck(); // 生命值显示

        if (!isMe) return;

        if (!PlayerDataScript.ROLE_STATE.isDead) {

            inputKeyCheck();

            skillCastCheck();

        }
    }

    void FixedUpdate() {

        timeReduce(); // CD与BUFF持续时间计算

    }

    // 按键监测 Update
    void inputKeyCheck() {

        if (PlayerDataScript.ROLE_STATE == null) return;

        if (PlayerDataScript.ROLE_STATE.currentSpell.Length < 3) {

            if (Input.GetKeyDown(KeyCode.J)) {

                PlayerDataScript.ROLE_STATE.currentSpell += "A";

            } else if (Input.GetKeyDown(KeyCode.K)) {

                PlayerDataScript.ROLE_STATE.currentSpell += "B";

            } else if (Input.GetKeyDown(KeyCode.L)) {

                PlayerDataScript.ROLE_STATE.currentSpell = "";

            }
        }
    }

    // 技能检测
    void skillCastCheck() {

        if (PlayerDataScript.ROLE_STATE == null) return;

        if (spellList.Count < 8 || spellList == null) return;

        if (spellList.Contains(PlayerDataScript.ROLE_STATE.currentSpell)) {

            skillCast(PlayerDataScript.ROLE_STATE.currentSpell);
            
        }
    }

    // 技能施放
    void skillCast(string spell) {

        Debug.Log("施放了" + spell);

        PlayerDataScript.ROLE_STATE.currentSpell = "";

    }

    写技能

    // CD 计算
    void timeReduce() {

        // Skill CD
        if (PlayerDataScript.ROLE_STATE == null) return;

        PlayerDataScript.ROLE_STATE.blockSkill.reduceCD(Time.deltaTime);

        PlayerDataScript.ROLE_STATE.normalBullet.reduceCD(Time.deltaTime);

        PlayerDataScript.ROLE_STATE.slowBullet.reduceCD(Time.deltaTime);

        PlayerDataScript.ROLE_STATE.fastBullet.reduceCD(Time.deltaTime);

        PlayerDataScript.ROLE_STATE.rayBullet.reduceCD(Time.deltaTime);

        PlayerDataScript.ROLE_STATE.blockWall.reduceCD(Time.deltaTime);

        PlayerDataScript.ROLE_STATE.shadow.reduceCD(Time.deltaTime);

        PlayerDataScript.ROLE_STATE.shield.reduceCD(Time.deltaTime);

    }

    // 生命值 至 盾值 计算
    void barCheck() {

        float fullHp = PlayerDataScript.ROLE_STATE.lifeOrigin;

        float nowHp = PlayerDataScript.ROLE_STATE.life;

        hpSlider.direction = Slider.Direction.LeftToRight;

        hpSlider.minValue = 0;

        hpSlider.maxValue = fullHp;

        hpSlider.value = nowHp;

        float fullBlockLife = PlayerDataScript.ROLE_STATE.blockLifeOrigin;

        float nowBlockLife = PlayerDataScript.ROLE_STATE.blockLife;

        blockSlider.direction = Slider.Direction.LeftToRight;

        blockSlider.minValue = 0;

        blockSlider.maxValue = fullBlockLife;

        blockSlider.value = nowBlockLife;

    }

    // 动画设置
    void aniCheck() {

        if (PlayerDataScript.ROLE_STATE.isDead == false) {

            roleAni.Play("roleStand");

        } else {

            roleAni.Play("roleDead");

        }

    }


    // 死亡
    public void dead() {

        PlayerDataScript.ROLE_STATE.isDead = true;

        roleSpriteRenderer.sortingOrder = 1;

        CuteUDPManager.cuteUDP.emitServer("Dead", "0");

    }

    // 子弹碰撞检测
    void OnTriggerEnter(Collider col) {

        BulletScript bs = col.gameObject.GetComponent<BulletScript>();

        // 我，是左边，碰到了右边来的子弹
        if (col.tag == "RightAllyBullet" && PlayerDataScript.ROLE_STATE.isLeftAlly && isMe) {


        // 我，是右边，碰到了左边来的子弹
        } else if (col.tag == "LeftAllyBullet" && !PlayerDataScript.ROLE_STATE.isLeftAlly && isMe) {
            
        }
    }
}
