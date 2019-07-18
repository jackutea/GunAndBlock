var event = require("events");
var cluster = require("cluster");

var COMPARE_GD = require("./GlobalData/COMPARE_GD");

class CompareApp extends event {

    constructor() {

        super();

        CompareApp.instance = this;

        this.initClusterListener();

        this.initCompareClusterListener();

        this.heartBeat = setInterval(this.compareHeartBeat, 1);

    }

    // 匹配循环
    compareHeartBeat() {

        // COMPARE_GD.COMPARE_STACK_LIST 键值对 serverId : levelStack
        // levelStack 键值对 level or rank : array[modeCode0,1,2]
        // levelStack[level] 键值对 modeCode : array[sid...]

        let allServerStackList = COMPARE_GD.COMPARE_STACK_LIST

        for (let serverId in allServerStackList) {

            // 某一个服务器内的列表
            let levelStack = allServerStackList[serverId];

            for (let levelIndex in levelStack) {

                // 某个等级的列表
                let perLevelStack = levelStack[levelIndex];

                for (let modeCode in perLevelStack) {

                    // 某个匹配模式列表（1V1 5V5等）
                    let perModeList = perLevelStack[modeCode];

                    if (modeCode === "0") {

                        // 如果1V1 模式列表内有超过2人，则拉出这两人
                        if (perModeList.length >= 2) {

                            let a = perModeList.shift();

                            let b = perModeList.shift();

                            console.log(a, "和", b, "匹配成功");

                            process.send({ eventName: "CompareSuccess", dataString: [a, b], sid: "" });

                        } else {

                            // console.log("等级", levelIndex, "1V1人数不够");

                        }
                    }

                }
            }
        }


        // 遍历所有等级数组
        // 如果1v1里有2人，拉出来

        // 如果5V5里有10人，拉出来

        // 如果50V50里有100人，拉出来

        // 遍历所有天梯数组

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

        let searchList = COMPARE_GD.COMPARE_STACK_LIST[serverId];

        let searchLevelIndex;

        if (level < 10) {

            searchLevelIndex = level;

        } else {

            searchLevelIndex = level + 10;

        }

        searchList[searchLevelIndex][modeCode].push(sid);

        process.send({ eventName: "Compare", dataString: "", sid: sid })

    }
}

module.exports = CompareApp;