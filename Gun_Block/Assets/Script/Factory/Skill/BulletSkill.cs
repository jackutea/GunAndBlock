using System;

// [Serializable] 
public class BulletSkill : SkillBase {

    public byte id;
    public string spell;
    public float flySpeedOrigin;
    public float flySpeed;
    public float dmgOrigin;
    public float dmg;
    public float singOrigin;
    public float sing;
    public float cdOrigin;
    public float cd;

    public override void reduceCD(float deltaTime) {

        if (cd > 0) this.cd -= deltaTime;

    }
}