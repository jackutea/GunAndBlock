var event = require("events");
var cluster = require("cluster");

var COMPARE_GD = require("./GlobalData/COMPARE_GD");

class CompareApp extends event {

    constructor() {

        super();

        CompareApp.instance = this;

        this.initClusterListener();

        this.initCompareClusterListener();

    }

    // 初始化进程监听器
    initClusterListener() {

        process.on("message", (msg) => {

            let eventName = msg.eventName;

            let dataString = msg.dataString;

            let sid = msg.sid;

            this.emit(eventName, dataString, sid);

            // console.log("Compare App Recv : ", msg);

        });

    }

    // 初始化全局变量
    // dataString = HALL_GD
    initCompareGD(dataString, sid) {

        COMPARE_GD.CreateGD(dataString);

    }

    // 初始化监听事件
    initCompareClusterListener() {

        // 处理非UDP
        this.on("InitCompareGD", this.initCompareGD);

        // 处理UDP
        this.on("PreCompare", this.preCompare); // 传参过来后再匹配
        this.on("Compare", this.compare); // 开始匹配
    }

    // 先传参，再匹配
    preCompare(data, sid) {

        let ONLINE_ACCOUNT = data.ONLINE_ACCOUNT;

        let ONLINE_ROLE = data.ONLINE_ROLE;

        let dataString = data.dataString;

        this.emit("Compare", ONLINE_ACCOUNT, ONLINE_ROLE, dataString, sid);
        
    }

    // 开始匹配
    compare(ONLINE_ACCOUNT, ONLINE_ROLE, dataString, sid) {

        // dataString = int code
        // code 0 : 1V1 / code 1 : 5V5 / code 2 : 50V50

        let modeCode = parseInt(dataString);

        let username = ONLINE_ACCOUNT[sid];

        let roleState = ONLINE_ROLE[username];

        let serverId = roleState.inServerId;

        let level = roleState.level;

        let rank = roleState.rank;

        let searchList;

        let searchLevelIndex;

        if (level < 10) {

            searchList = COMPARE_GD.COMPARE_STACK_LIST[serverId].levelStack;

            searchLevelIndex = level;

        } else {

            searchList = COMPARE_GD.COMPARE_STACK_LIST[serverId].rankStack;

            searchLevelIndex = rank;

        }

        searchList[searchLevelIndex][modeCode].push(sid);

        process.send({ eventName: "Compare", dataString: "", sid: sid })

    }
}

module.exports = CompareApp;