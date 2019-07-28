var Skill = require("../AllClass/Skill");
var SkillSpell = require("./SkillSpell");
var SkillEnum = require("./SkillEnum");

var SkillList = [];
SkillList[SkillEnum.block] = new Skill(SkillEnum.block, SkillSpell[SkillEnum.block]);
SkillList[SkillEnum.normalBullet] = new Skill(SkillEnum.normalBullet, SkillSpell[SkillEnum.normalBullet]);
SkillList[SkillEnum.slowBullet] = new Skill(SkillEnum.slowBullet, SkillSpell[SkillEnum.slowBullet]);
SkillList[SkillEnum.fastBullet] = new Skill(SkillEnum.fastBullet, SkillSpell[SkillEnum.fastBullet]);
SkillList[SkillEnum.rayLight] = new Skill(SkillEnum.rayLight, SkillSpell[SkillEnum.rayLight]);
SkillList[SkillEnum.blockWall] = new Skill(SkillEnum.blockWall, SkillSpell[SkillEnum.blockWall]);
SkillList[SkillEnum.shadow] = new Skill(SkillEnum.shadow, SkillSpell[SkillEnum.shadow]);
SkillList[SkillEnum.shield] = new Skill(SkillEnum.shield, SkillSpell[SkillEnum.shield]);

module.exports = SkillList;