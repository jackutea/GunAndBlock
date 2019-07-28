using System;
using System.Collections;
using System.Collections.Generic;

public class ConfigCollection {

    public static Dictionary<int, string> SkillSpell = null;

    public static void initConf() {

        SkillSpell = new Dictionary<int, string>();
        SkillSpell.Add((int)SkillEnum.block, "AAA");
        SkillSpell.Add((int)SkillEnum.normalBullet, "AAB");
        SkillSpell.Add((int)SkillEnum.slowBullet, "ABB");
        SkillSpell.Add((int)SkillEnum.fastBullet, "ABA");
        SkillSpell.Add((int)SkillEnum.rayLight, "BBB");
        SkillSpell.Add((int)SkillEnum.blockWall, "BAB");
        SkillSpell.Add((int)SkillEnum.shadow, "BAA");
        SkillSpell.Add((int)SkillEnum.shield, "BBA");

    }
}