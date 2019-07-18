var DefaultLevel = require("./Config/DefaultLevel");

class COMPARE_GD {};

COMPARE_GD.COMPARE_STACK_LIST = {}; // 匹配列表 serverId : CompareStack

COMPARE_GD.CreateGD = function(SERVER_LIST) {

    for (let i in SERVER_LIST) {

        let serverId = i;

        COMPARE_GD.COMPARE_STACK_LIST[serverId] = new CompareStack(serverId).levelStack;

    }

}

class CompareStack {

    constructor(serverId) {

        this.serverId = serverId;

        this.levelStack = {}; // level or rank: Array -- 时间最小堆

        this.initStackList();

    }

    initStackList() {

        for (let i = 0; i < DefaultLevel.levelExpRequireList.length + DefaultLevel.rankScoreRequireList.length; i += 1) {

            this.levelStack[i] = []; // level or rank : Array

            this.levelStack[i][0] = []; // 1V1 列表
            
            this.levelStack[i][1] = []; // 5V5 列表

            this.levelStack[i][2] = []; // 50V50 列表

        }
    }
}

module.exports = COMPARE_GD;