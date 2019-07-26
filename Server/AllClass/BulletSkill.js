
class BulletSkill {

    constructor(id) {

        this.id = id;
        this.spell;
        this.flySpeedOrigin;
        this.flySpeed;
        this.dmgOrigin;
        this.dmg;
        this.singOrigin;
        this.sing;
        this.cdOrigin;
        this.cd;

        this.initSkill(id);

    }

    initSkill(id) {
        
        switch(id) {

            case 2:
                this.spell = "AAB";
                this.flySpeedOrigin = 3.0;
                this.flySpeed = 3.0;
                this.dmgOrigin = 1.0;
                this.dmg = 1.0;
                this.singOrigin = 0.0;
                this.sing = 0.0;
                this.cdOrigin = 1.5;
                this.cd = 0.0;
                break;
            
            case 3:
                this.spell = "ABB";
                this.flySpeedOrigin = 0.5;
                this.flySpeed = 0.5;
                this.dmgOrigin = 2.0;
                this.dmg = 2.0;
                this.singOrigin = 0.0;
                this.sing = 0.0;
                this.cdOrigin = 2.0;
                this.cd = 0.0;
                break;
            
            case 4:
                this.spell = "ABA";
                this.flySpeedOrigin = 6.0;
                this.flySpeed = 6.0;
                this.dmgOrigin = 0.5;
                this.dmg = 0.5;
                this.singOrigin = 0.0;
                this.sing = 0.0;
                this.cdOrigin = 6.0;
                this.cd = 0.0;
                break;
            
            case 5:
                this.spell = "BBB";
                this.flySpeedOrigin = 15.0;
                this.flySpeed = 15.0;
                this.dmgOrigin = 0.2;
                this.dmg = 0.2;
                this.singOrigin = 1.5;
                this.sing = 1.5;
                this.cdOrigin = 8.0;
                this.cd = 0.0;
                break;

            default:
                console.log("错误的BulletInfo Id");
                break;
        }

    }
}


module.exports = BulletSkill;