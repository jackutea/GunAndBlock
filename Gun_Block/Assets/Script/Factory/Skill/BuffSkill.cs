using System;

[Serializable]
public abstract class SkillBase {
    public abstract void reduceCD(float deltaTime);
}


public class BuffSkill : SkillBase {

    public byte id;
    public string spell;
    public float singOrigin;
    public float sing;
    public float cdOrigin;
    public float cd;
    public float buffLastOrigin;
    public float buffLast;

    public override void reduceCD(float deltaTime) {

        if (cd > 0) this.cd -= deltaTime;

    }

}