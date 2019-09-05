using System;

[Serializable]
public class SkillState {

    public int id;
    public string spell;
    public string skillName;
    public bool isBuff;
    public float flySpeedOrigin;
    public float flySpeed;
    public float dmgOrigin;
    public float dmg;
    public float singOrigin;
    public float sing;
    public float cdOrigin;
    public float cd;
    public float buffLastOrigin;
    public float buffLast;
    
    public bool isImmune;
    public bool isReflect;
    public int addBlockCount;

}