
class FieldInfo {

    constructor(index, modeCode, sidJson) {

        this.fieldId = index;
        
        this.modeCode = modeCode;
        
        this.sidJson = sidJson;

        this.bidJson = {};

    }
}


module.exports = FieldInfo;