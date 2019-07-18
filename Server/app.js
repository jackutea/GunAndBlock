var cluster = require("cluster");

var SocketApp = require("./SocketApp");

var HallApp = require("./HallApp");
var CompareApp = require("./CompareApp");
var BattleApp = require("./BattleApp");

if (cluster.isMaster) {

    for (let i = 0; i < 3; i += 1) {

        cluster.fork();

    }
    
    new SocketApp();

} else {

    if (cluster.worker.id === 1) {

        let hallApp = new HallApp();

    }

    if (cluster.worker.id === 2) {

        new CompareApp();

    }

    if (cluster.worker.id === 3) {

        new BattleApp();

    }
}

// TODO :
// 1. 匹配系统
// 2. 人物反馈（血条反馈、攻击反馈、格挡反馈、完美格挡反馈），以及声效
// 3. Ping功能
// 4. 水晶

// TODO : 同IP同端口检测