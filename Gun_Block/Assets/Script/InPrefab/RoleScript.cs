using System;
using System.Linq;
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

    AudioSource audioSource;
    AudioClip dongAudio;
    AudioClip taAudio;

    public bool isMe;
    public string oppoSid;
    public int marchIndex;

    public Text nameBar;
    public Slider hpSlider;
    public Slider blockSlider;
    public Slider singSlider;

    float singTime = 0;

    public Button AButton;
    public Button BButton;

    public RoleState roleState = null;

    Dictionary<string, int> spellToSkillDic;

    void Awake() {

        isMe = false;

    }

    void Start() {

        roleCollider = GetComponent<Collider>();

        roleSpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        roleAni = GetComponent<Animator>();

        audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;

        dongAudio = Resources.Load<AudioClip>("Audio/dong");

        taAudio = Resources.Load<AudioClip>("Audio/ta");

        nameBar.text = roleState.roleName;

        roleState.username = PlayerDataScript.USER_NAME;

        initSpellToSkill();

        AButton = GameObject.FindWithTag("AButton").GetComponent<Button>();
        
        BButton = GameObject.FindWithTag("BButton").GetComponent<Button>();

        AButton.onClick.AddListener(() => {

            touchCheck("A");

        });

        BButton.onClick.AddListener(() => {

            touchCheck("B");

        });

    }

    void Update() {

        aniCheck(); // 动画更替

        barCheck(); // 生命条显示

        if (!isMe) return;

        PlayerDataScript.ROLE_STATE = roleState;

        if (!roleState.isDead) {

            inputKeyCheck();

            skillCastCheck();

        }
    }

    void FixedUpdate() {

        if (!isMe) return;

        timeReduce(); // CD与BUFF持续时间计算

    }

    // (通用) 初始化技能键值对
    void initSpellToSkill() {

        spellToSkillDic = new Dictionary<string, int>();

        if (ConfigCollection.SkillSpell == null) return;

        foreach (var kv in ConfigCollection.SkillSpell) {

            int skillEnum = kv.Key;

            string spell = kv.Value;
            
            spellToSkillDic.Add(spell, skillEnum);

        }
    }

    // 手机按键监测
    void touchCheck(string btn) {

        if (!isMe) return;

        if (roleState == null) return;

        if (roleState.isDead) return;

        if (roleState.currentSpell.Length < 3) {

            roleState.currentSpell += btn;

        }
    }

    // (个人角色) 按键监测 Update
    void inputKeyCheck() {

        if (!isMe) return;

        if (roleState == null) return;

        if (roleState.currentSpell.Length < 3) {

            if (Input.GetKeyDown(KeyCode.J)) {

                roleState.currentSpell += "A";

                roleAni.Play("roleRed");

                audioSource.clip = dongAudio;

                audioSource.Play();

            } else if (Input.GetKeyDown(KeyCode.K)) {

                roleState.currentSpell += "B";

                roleAni.Play("roleBlue");

                audioSource.clip = taAudio;

                audioSource.Play();

            } else if (Input.GetKeyDown(KeyCode.L)) {

                roleState.currentSpell = "";

            }
        }
    }

    // (个人角色) 技能检测
    void skillCastCheck() {

        if (!isMe) return;

        if (roleState == null) return;

        string currentSpell = roleState.currentSpell;

        if (spellToSkillDic.ContainsKey(currentSpell)) {

            int skillEnum = (int)spellToSkillDic[currentSpell];

            SkillState skill = roleState.skillList[skillEnum];

            if (roleState.isSinging == false) {

                roleState.isBlocking = false;

                if (roleState.skillList[skillEnum].cd > 0) {

                    // Debug.Log("CD中");

                    roleState.currentSpell = "";

                    return;

                }

                roleState.isSinging = true;

                singTime = roleState.skillList[skillEnum].singOrigin;

                singSlider.maxValue = roleState.skillList[skillEnum].singOrigin;

                singSlider.value = roleState.skillList[skillEnum].singOrigin;

                if (singSlider.maxValue > 0) {

                    singSlider.gameObject.SetActive(true);
                    
                }

            } else {

                if (singTime > 0) {

                    singTime -= Time.deltaTime;

                } else {

                    roleState.currentSpell = "";

                    CuteUDPManager.cuteUDP.emitServer(BattleEventEnum.CastSkill.ToString(), skillEnum.ToString());

                }
            }

        } else {

            roleState.isSinging = false;

            singTime = 0;

        }
    }

    // (通用) 技能施放
    public void castSkill(int skillEnum, string timeSample) {

        if (roleState == null) return;

        roleState.isSinging = false;

        roleState.skillList[skillEnum].cd = roleState.skillList[skillEnum].cdOrigin;

        roleState.skillList[skillEnum].buffLast = roleState.skillList[skillEnum].buffLastOrigin;

        GameObject skillObj = Instantiate(PrefabCollection.skillPrefabDic[skillEnum], transform.position, transform.rotation, transform.parent.transform);

        SkillScript skillScript = skillObj.GetComponent<SkillScript>();

        skillObj.tag = (roleState.isLeftAlly) ? "LeftSkill" : "RightSkill";

        skillScript.sid = roleState.sid;

        skillScript.timeSample = timeSample;

        skillScript.bid = skillScript.getBid(timeSample);

        skillScript.oppoSid = oppoSid;

        skillScript.skillEnum = skillEnum;

    }

    // (个人角色) CD 计算
    void timeReduce() {

        if (!isMe) return;

        // Skill CD
        if (roleState == null) return;

        if (roleState.skillList == null) return;

        if (roleState.skillList.Length <= 0) return;

        for (int i = 0; i < roleState.skillList.Length; i += 1) {

            int skillEnum = i;

            SkillState skill = roleState.skillList[skillEnum];

            if (skill.cd > 0) {

                roleState.skillList[skillEnum].cd -= Time.deltaTime;

            }

            if (skill.buffLast > 0) {

                roleState.skillList[skillEnum].buffLast -= Time.deltaTime;

            }
        }
    }

    // (通用) 生命值 与 盾值 计算
    void barCheck() {

        // 血量
        float fullHp = roleState.lifeOrigin;

        float nowHp = roleState.life;

        hpSlider.direction = Slider.Direction.LeftToRight;

        hpSlider.minValue = 0;

        hpSlider.maxValue = fullHp;

        hpSlider.value = nowHp;

        if (nowHp <= 0 && isMe && !roleState.isDead) dead();

        // 盾值
        float fullBlockLife = roleState.blockLifeOrigin;

        float nowBlockLife = roleState.blockLife;

        blockSlider.direction = Slider.Direction.LeftToRight;

        blockSlider.minValue = 0;

        blockSlider.maxValue = fullBlockLife;

        blockSlider.value = nowBlockLife;

        // 吟唱条
        if (singSlider.value <= 0) {

            singSlider.gameObject.SetActive(false);

        } else {

            singSlider.value = singTime;

        }
    }

    // 动画设置
    void aniCheck() {

        if (roleState.isDead == false) {

            roleAni.Play("roleStand");

        } else {

            roleAni.Play("roleDead");

        }
    }

    // 死亡
    public void dead() {

        roleState.isDead = true;

        // gameObject.SetActive(false);

        // roleSpriteRenderer.sortingOrder = 1;

        CuteUDPManager.cuteUDP.emitServer(BattleEventEnum.Dead.ToString(), "0");

    }
}
