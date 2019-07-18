var DefaultLevel = require("./Config/DefaultLevel");

class COMPARE_GD {};

COMPARE_GD.COMPARE_STACK_LIST = {}; // 匹配列表 serverId : CompareStack

COMPARE_GD.CreateGD = function(SERVER_LIST) {

    for (let i in SERVER_LIST) {

        let serverId = i;

        COMPARE_GD.COMPARE_STACK_LIST[serverId] = new CompareStack(serverId);

    }

}

class CompareStack {

    constructor(serverId) {

        this.serverId = serverId;

        this.levelStack = {}; // level : Array -- 时间最小堆

        this.rankStack = {}; // rank : Array -- 时间最小堆

        this.initStackList();

    }

    initStackList() {

        for (let i = 0; i < DefaultLevel.levelExpRequireList.length; i += 1) {

            this.levelStack[i] = []; // level : Array

            this.levelStack[i][0] = []; // 1V1 列表
            
            this.levelStack[i][1] = []; // 5V5 列表

            this.levelStack[i][2] = []; // 50V50 列表

        }

        for (let i = 0; i < DefaultLevel.rankScoreRequireList.length; i += 1) {

            this.rankStack[i] = []; // rank : Array

            this.rankStack[i][0] = []; // 1V1 列表
            
            this.rankStack[i][1] = []; // 5V5 列表

            this.rankStack[i][2] = []; // 50V50 列表
            
        }
    }
}

module.exports = COMPARE_GD;