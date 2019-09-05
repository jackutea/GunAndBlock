
class FieldState {

    constructor(index, modeCode, sidJson) {

        this.fieldId = index; // 该战场id
        
        this.modeCode = modeCode; // string 该战场的匹配模式 0：1v1 1：5v5 2：50v50
        
        this.sidJson = sidJson; // 在该战场的玩家

        this.leftLive = 0; // 左方队伍

        this.rightLive = 0; // 右方队伍

        this.initFieldState(modeCode);

    }

    initFieldState(modeCode) {

        switch(modeCode) {

            case "0":

                this.leftLive = 1;

                this.rightLive = 1;

                break;

            case "1":

                this.leftLive = 5;

                this.rightLive = 5;

                break;

            case "2":

                this.leftLive = 50;

                this.rightLive = 50;

                break;

            default:

                break;
        }
    }
}


module.exports = FieldState;