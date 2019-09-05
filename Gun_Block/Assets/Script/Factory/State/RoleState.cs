using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoleState{

    // ———— 战前相关 ————
    public string sid;
    public string username;
    public string roleName;
    public int level;
    public int exp;
    public int rank;
    public int score;
    public int gold;
    public int inServerId = -1;
    public int waitMode = -1;
    
    // ———— 战时相关 ————（不存入数据库）
    public string oppoSid = ""; // 对位sid
    public int inFieldId = -1;
    public float lifeOrigin = 5;
    public float life = 5;
    public float blockLifeOrigin = 3;
    public float blockLife = 3;
    public bool isBlocking = false;
    
    // ———— 技能 ————
    public string currentSpell = ""; // 技能命令
    public SkillState[] skillList = null; // 技能表
    // public BuffSkill blockSkill; // 格挡 AAA
    // public BulletSkill normalBullet; // 普攻
    // public BulletSkill slowBullet; // 慢攻
    // public BulletSkill fastBullet; // 快攻
    // public BulletSkill rayBullet; // 激光
    // public BuffSkill blockWall; // 拒鹿
    // public BuffSkill shadow; // 雾
    // public BuffSkill shield; // 粒子盾

    // ———— 状态 ————
    public bool isLeftAlly = true; // 阵营
    public bool isSinging = false;
    public bool isImmune = false;
    public bool isReflect = false;
    public int blockCount = 0;
    public bool isDead = false; // 复活状态

}