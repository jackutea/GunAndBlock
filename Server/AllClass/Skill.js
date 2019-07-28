var SkillEnum = require("../Config/SkillEnum");

class Skill {

    constructor(skillEnum, spell) {

        this.id = skillEnum;
        this.spell = spell;
        this.isBuff = false;
        this.flySpeedOrigin = 0.0;
        this.flySpeed = 0.0;
        this.dmgOrigin = 0.0;
        this.dmg = 0.0;
        this.singOrigin = 0.0;
        this.sing = 0.0;
        this.cdOrigin = 0.0;
        this.cd = 0.0;
        this.buffLastOrigin = 0.0;
        this.buffLast = 0.0;
        this.isImmune = false;
        this.isReflect = false;
        this.addBlockCount = 0;
        this.initSkill(skillEnum);

    }

    initSkill(skillEnum) {

        if (skillEnum == SkillEnum.block) {
            this.isBuff = true;
            this.cdOrigin = 0.2;
            this.buffLastOrigin = 0.2;
            this.isReflect = true;
        }
            
        if (skillEnum == SkillEnum.normalBullet) {
            this.flySpeedOrigin = 3.0;
            this.flySpeed = 3.0;
            this.dmgOrigin = 1.0;
            this.dmg = 1.0;
            this.cdOrigin = 1.5;
        }
            
        if (skillEnum == SkillEnum.slowBullet) {
            this.flySpeedOrigin = 0.5;
            this.flySpeed = 0.5;
            this.dmgOrigin = 2.0;
            this.dmg = 2.0;
            this.cdOrigin = 2.0;
        }
            
        if (skillEnum == SkillEnum.fastBullet) {
            this.flySpeedOrigin = 6.0;
            this.flySpeed = 6.0;
            this.dmgOrigin = 0.5;
            this.dmg = 0.5;
            this.cdOrigin = 6.0;
        }
            
        if (skillEnum == SkillEnum.rayLight) {
            this.flySpeedOrigin = 15.0;
            this.flySpeed = 15.0;
            this.dmgOrigin = 0.2;
            this.dmg = 0.2;
            this.singOrigin = 1.5;
            this.sing = 1.5;
            this.cdOrigin = 8.0;
            this.cd = 0.0;
            this.buffLastOrigin = 0.0;
        }

        if (skillEnum == SkillEnum.blockWall) {
            this.isBuff = true;
            this.singOrigin = 0.8;
            this.sing = 0.8;
            this.cdOrigin = 10.0;
            this.buffLastOrigin = 5.0;
            this.addBlockCount = 1;
        }

        if (skillEnum == SkillEnum.shadow) {
            this.isBuff = true;
            this.singOrigin = 2.0;
            this.sing = 2.0;
            this.cdOrigin = 30.0;
            this.buffLastOrigin = 5.0;
        }

        if (skillEnum == SkillEnum.shield) {
            this.isBuff = true;
            this.singOrigin = 1.5;
            this.sing = 1.5;
            this.cdOrigin = 50.0;
            this.buffLastOrigin = 5.0;
            this.isImmune = true;
        }
    }
}

module.exports = Skill;