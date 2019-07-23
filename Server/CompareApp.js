var event = require("events");
var cluster = require("cluster");

var COMPARE_GD = require("./GlobalData/COMPARE_GD");

class Rarray {

    constructor() {

        this.arr = [];

    }
}

class CompareApp extends event {

    constructor() {

        super();

        CompareApp.instance = this;

        this.initClusterListener();

        this.initCompareClusterListener();

        setTimeout(this.compareHeartBeat, 1, this);

    }

    // 匹配循环
    compareHeartBeat(instance) {

        // COMPARE_GD.COMPARE_STACK_LIST 键值对 serverId : levelStack
        // levelStack 键值对 level or rank : array[modeCode0,1,2]
        // levelStack[level] 键值对 modeCode : array[sid...]

        // 遍历所有堆
        // 如果1v1里有2人，拉出来
        // 如果5V5里有10人，拉出来
        // 如果50V50里有100人，拉出来

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

                            let sidJson = new Object();

                            for (let i = 0; i < 2; i += 1) {

                                let sid = perModeList.shift();

                                sidJson[sid] = {};

                            }

                            process.send({ eventName: "CompareSuccess", dataString: JSON.stringify(sidJson), sid: "" });

                        } else {

                            // console.log("等级", levelIndex, "1V1人数不够");

                        }
                    }

                }
            }
        }

        setTimeout(instance.compareHeartBeat, 1, instance);
    }

    // 初始化进程监听器
    initClusterListener() {

        cluster.worker.on("message", (msg) => {

            let eventName = msg.eventName;

            let dataString = msg.dataString;

            let sid = msg.sid;

            this.emit(eventName, dataString, sid);

            // console.log("Compare App Recv : ", msg);

        });

        cluster.worker.on("error", (err) => {

            console.log("CompareApp Error :", err);
            
        });

        cluster.worker.on("disconnect", () => {

            console.log("CompareApp disconnect");
            
        });

    }

    // 初始化全局变量
    // dataString = HALL_GD
    initCompareGD(dataString, sid) {

        COMPARE_GD.CreateGD(dataString);

    }

    // 初始化监听事件
    initCompareClusterListener() {

        this.on("error", (err) => {

            console.log("CompareApp Event Error:", err);
            
        })

        // 处理非UDP
        this.on("InitCompareGD", this.initCompareGD);

        // 处理UDP
        this.on("PreCompare", this.preCompare); // 传参过来后再匹配
        this.on("Compare", this.compare); // 开始匹配
    }

    // 先传参，再匹配
    preCompare(data, sid) {

        process.nextTick(() => {

            let roleState = data.roleState;

            let dataString = data.dataString;

            this.emit("Compare", roleState, dataString, sid);
                            
        })
    }

    // 开始匹配
    compare(roleState, dataString, sid) {

        process.nextTick(() => {

            // dataString = int code
            // code 0 : 1V1 / code 1 : 5V5 / code 2 : 50V50

            let modeCode = parseInt(dataString);

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

            process.send({ eventName: "CompareWait", dataString: modeCode.toString(), sid: sid })
                    
        })
    }
}

module.exports = CompareApp;