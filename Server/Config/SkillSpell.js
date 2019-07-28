var SkillEnum = require("./SkillEnum");

var SkillSpell = [];
SkillSpell[SkillEnum.block] = "AAA";
SkillSpell[SkillEnum.normalBullet] = "AAB";
SkillSpell[SkillEnum.slowBullet] = "ABB";
SkillSpell[SkillEnum.fastBullet] = "ABA";
SkillSpell[SkillEnum.rayLight] = "BBB";
SkillSpell[SkillEnum.blockWall] = "BAB";
SkillSpell[SkillEnum.shadow] = "BAA";
SkillSpell[SkillEnum.shield] = "BBA";

module.exports = SkillSpell;