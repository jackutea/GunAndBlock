var skillList = require("../Config/SkillList");

class RoleState {
    
    constructor() {

        // 战前相关
        this.sid = "";
        this.username = "";
        this.roleName = "";
        this.level = 1;
        this.exp = 0;
        this.rank = 0;
        this.score = 0;
        this.gold = 0;
        this.inServerId = -1;
        this.waitMode = -1;

        // 战斗时相关（不会存入数据库）
        this.inFieldId = -1;
        this.lifeOrigin = 5.0;
        this.life = 5.0;
        this.blockLifeOrigin = 3.0;
        this.blockLife = 3.0;
        this.isBlocking = false;
        this.towerLifeOrigin = 50.0;
        this.towerLife = 50.0;

        // 技能
        this.currentSpell = "";
        this.skillList = skillList;
        // this.blockSkill = new BuffSkill(1);
        // this.normalBullet = new BulletSkill(2);
        // this.slowBullet = new BulletSkill(3);
        // this.fastBullet = new BulletSkill(4);
        // this.rayBullet = new BulletSkill(5);
        // this.blockWall = new BuffSkill(6);
        // this.shadow = new BuffSkill(7);
        // this.shield = new BuffSkill(8);

        // 状态
        this.isLeftAlly = true;
        this.isSinging = false;
        this.isImmune = false;
        this.isReflect = false;
        this.blockCount = 0;
        this.isDead = false;

    }
}

module.exports = RoleState;