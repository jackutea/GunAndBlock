var SkillEnum = require("../../Config/SkillEnum");

class SkillState {

    constructor(skillEnum, spell) {

        this.id = skillEnum;
        this.spell = spell;
        this.skillName;
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
            this.skillName = "格挡";
            this.isBuff = true;
            this.cdOrigin = 0.3;
            this.buffLastOrigin = 0.3;
            this.isReflect = true;
        }
            
        if (skillEnum == SkillEnum.normalBullet) {
            this.skillName = "左轮";
            this.flySpeedOrigin = 3.0;
            this.flySpeed = 3.0;
            this.dmgOrigin = 1.0;
            this.dmg = 1.0;
            this.cdOrigin = 1.5;
        }
            
        if (skillEnum == SkillEnum.slowBullet) {
            this.skillName = "缓速弹";
            this.flySpeedOrigin = 1.2;
            this.flySpeed = 1.2;
            this.dmgOrigin = 2.0;
            this.dmg = 2.0;
            this.cdOrigin = 2.0;
        }
            
        if (skillEnum == SkillEnum.fastBullet) {
            this.skillName = "狙击";
            this.flySpeedOrigin = 6.0;
            this.flySpeed = 6.0;
            this.dmgOrigin = 0.5;
            this.dmg = 0.5;
            this.cdOrigin = 6.0;
        }
            
        if (skillEnum == SkillEnum.rayLight) {
            this.skillName = "镭射";
            this.flySpeedOrigin = 15.0;
            this.flySpeed = 15.0;
            this.dmgOrigin = 0.3;
            this.dmg = 0.3;
            this.singOrigin = 1.0;
            this.sing = 1.0;
            this.cdOrigin = 7.0;
            this.buffLastOrigin = 0.0;
        }

        if (skillEnum == SkillEnum.blockWall) {
            this.skillName = "惩罚";
            this.isBuff = true;
            this.singOrigin = 0.4;
            this.sing = 0.4;
            this.cdOrigin = 10.0;
            this.buffLastOrigin = 5.0;
            this.addBlockCount = 1;
        }

        if (skillEnum == SkillEnum.shadow) {
            this.skillName = "烟雾";
            this.isBuff = true;
            this.singOrigin = 0.5;
            this.sing = 0.5;
            this.cdOrigin = 20.0;
            this.buffLastOrigin = 5.0;
        }

        if (skillEnum == SkillEnum.shield) {
            this.skillName = "粒子护盾";
            this.isBuff = true;
            this.singOrigin = 0.6;
            this.sing = 0.6;
            this.cdOrigin = 30.0;
            this.buffLastOrigin = 5.0;
            this.isImmune = true;
        }
    }
}

module.exports = SkillState;