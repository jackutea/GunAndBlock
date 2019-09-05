var cluster = require("cluster");

var SocketApp = require("./SocketApp");

var HallApp = require("./HallApp");
var CompareApp = require("./CompareApp");
var BattleApp = require("./BattleApp");

var redisPort = 6379;

var remotePort = 10001;

var localPort = 10000;

var remoteIp = "47.104.82.241";

if (cluster.isMaster) {

    for (let i = 0; i < 3; i += 1) {

        cluster.fork();

    }
    
    new SocketApp(redisPort, remoteIp, remotePort, localPort);

} else {

    if (cluster.worker.id === 1) {

        new HallApp(redisPort, "localhost");

    }

    if (cluster.worker.id === 2) {

        new CompareApp(redisPort, "localhost");

    }

    if (cluster.worker.id === 3) {

        new BattleApp(redisPort, "localhost");

    }
}

// TODO :
// 1. 匹配系统
// 2. 人物反馈（血条反馈、攻击反馈、格挡反馈、完美格挡反馈），以及声效
// 3. Ping功能
// 4. 水晶

// TODO : 同IP同端口检测