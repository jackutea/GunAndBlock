var event = require("events");
var cluster = require("cluster");

var BATTLE_GD = require("./GlobalData/BATTLE_GD");

class BattleApp extends event {

    constructor() {

        super();

        BattleApp.instance = this;

        this.initClusterListener();

    }

    // 初始化进程监听器
    initClusterListener() {

        process.on("message", (msg) => {

            let eventName = msg.eventName;

            let dataString = msg.dataString;

            this.emit(eventName, dataString);

            // console.log("Battle App Recv : ", msg);

        });

    }

    // 初始化全局变量
    initBattleGD(dataString, sid) {

        BATTLE_GD.CreateGD();

    }

    // 初始化进程监听事件
    initBattleClusterListener() {

        this.on("InitBattleGD", this.initBattleGD)

    }
}

module.exports = BattleApp;