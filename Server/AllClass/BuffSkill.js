
class BuffSkill {

    constructor(id) {

        this.id = id;

        this.spell;

        this.singOrigin;

        this.sing;

        this.cdOrigin;

        this.cd;

        this.buffLastOrigin;

        this.buffLast;

        this.initSkill(id);

    }

    initSkill(id) {

        switch(id) {

            case 1:
                this.spell = "AAA";
                this.singOrigin = 0.0;
                this.sing = 0.0;
                this.cdOrigin = 0.2;
                this.cd = 0.0;
                this.buffLastOrigin = 0.2;
                this.buffLast = 0.2;
                break;

            case 6:
                this.spell = "BAB";
                this.singOrigin = 0.8;
                this.sing = 0.8;
                this.cdOrigin = 10.0;
                this.cd = 0.0;
                this.buffLastOrigin = 5.0;
                this.buffLast = 5.0;
                break;

            case 7:
                this.spell = "BAA";
                this.singOrigin = 2.0;
                this.sing = 2.0;
                this.cdOrigin = 30.0;
                this.cd = 0.0;
                this.buffLastOrigin = 5.0;
                this.buffLast = 5.0;
                break;

            case 8:
                this.spell = "BBA";
                this.singOrigin = 1.5;
                this.sing = 1.5;
                this.cdOrigin = 50.0;
                this.cd = 0.0;
                this.buffLastOrigin = 5.0;
                this.buffLast = 5.0;
                break;

            default:
                console.log("错误的BuffSkill Id");
                break;
        }
    }
}

module.exports = BuffSkill;