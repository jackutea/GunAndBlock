var SkillState = require("../AllClass/State/SkillState");
var SkillSpell = require("./SkillSpell");
var SkillEnum = require("./SkillEnum");

var SkillList = [];
SkillList[SkillEnum.block] = new SkillState(SkillEnum.block, SkillSpell[SkillEnum.block]);
SkillList[SkillEnum.normalBullet] = new SkillState(SkillEnum.normalBullet, SkillSpell[SkillEnum.normalBullet]);
SkillList[SkillEnum.slowBullet] = new SkillState(SkillEnum.slowBullet, SkillSpell[SkillEnum.slowBullet]);
SkillList[SkillEnum.fastBullet] = new SkillState(SkillEnum.fastBullet, SkillSpell[SkillEnum.fastBullet]);
SkillList[SkillEnum.rayLight] = new SkillState(SkillEnum.rayLight, SkillSpell[SkillEnum.rayLight]);
SkillList[SkillEnum.blockWall] = new SkillState(SkillEnum.blockWall, SkillSpell[SkillEnum.blockWall]);
SkillList[SkillEnum.shadow] = new SkillState(SkillEnum.shadow, SkillSpell[SkillEnum.shadow]);
SkillList[SkillEnum.shield] = new SkillState(SkillEnum.shield, SkillSpell[SkillEnum.shield]);

module.exports = SkillList;