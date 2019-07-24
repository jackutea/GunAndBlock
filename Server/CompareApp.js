var event = require("events");
var cluster = require("cluster");
var Redis = require("redis");

var DefaultLevel = require("./GlobalData/Config/DefaultLevel");
var FieldInfo = require("./AllClass/FieldInfo");

class CompareApp extends event {

    constructor(redisPort, redisIp) {

        super();

        CompareApp.instance = this;

        this.rds = Redis.createClient(redisPort, redisIp);

        this.initClusterListener();

        this.initCompareClusterListener();

        this.levelStackList = {};

        this.initCompareStack((data) => {

            if (data === true) {

                setTimeout(this.compareHeartBeat, 1, this);

            }
        });
    }

    initCompareStack(callback) {

        for (let i = 0; i < DefaultLevel.levelExpRequireList.length + DefaultLevel.rankScoreRequireList.length; i += 1) {

            this.levelStackList[i] = []; // level or rank : Array

            this.levelStackList[i]["0"] = []; // 1V1 列表
            
            this.levelStackList[i]["1"] = []; // 5V5 列表

            this.levelStackList[i]["2"] = []; // 50V50 列表

        }

        callback(true);

    }

    // 匹配循环
    compareHeartBeat(instance) {

        // levelStack 键值对 level or rank : array[modeCode0,1,2]
        // levelStack[level] 键值对 modeCode : array[sid...]

        // 遍历所有堆
        // 如果1v1里有2人，拉出来
        // 如果5V5里有10人，拉出来
        // 如果50V50里有100人，拉出来

        let levelStack = instance.levelStackList;

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

                        instance.emit("LoadField", sidJson);

                    } else {

                        // console.log("等级", levelIndex, "1V1人数不够");

                    }

                } else if (modeCode === "1") {

                    // 如果5V5 模式列表内有超过10人，则拉出10人
                    if (perModeList.length >= 10) {

                        let sidJson = new Object();

                        for (let i = 0; i < 10; i += 1) {

                            let sid = perModeList.shift();

                            sidJson[sid] = {};

                        }

                        instance.emit("LoadField", sidJson);

                    }

                } else if (modeCode === "2") {

                    // 如果50V50 模式列表内有超过100人，则拉出100人
                    if (perModeList.length >= 100) {

                        let sidJson = new Object();

                        for (let i = 0; i < 100; i += 1) {

                            let sid = perModeList.shift();

                            sidJson[sid] = {};

                        }

                        instance.emit("LoadField", sidJson);

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

    // 初始化监听事件
    initCompareClusterListener() {

        this.on("error", (err) => {

            console.log("CompareApp Event Error:", err);
            
        })

        // 处理非UDP
        this.on("LoadField", this.loadField);

        // 处理UDP
        this.on("Compare", this.compare); // 开始匹配
    }

    // 开始匹配
    // dataString = modeCode
    compare(dataString, sid) {

        process.nextTick(() => {

            // dataString = int code
            // code 0 : 1V1 / code 1 : 5V5 / code 2 : 50V50

            let modeCode = dataString;

            this.rds.hget(sid, "username", (err0, data0) => {

                let username = data0;

                this.rds.hget(username, "roleState", (err1, data1) => {

                    let roleState = JSON.parse(data1);

                    let level = roleState.level;

                    let searchList = this.levelStackList;

                    let searchLevelIndex;

                    if (level < 10) {

                        searchLevelIndex = level;

                    } else {

                        searchLevelIndex = level + 10;

                    }

                    searchList[searchLevelIndex][modeCode].push(sid);

                    process.send({ eventName: "CompareWait", dataString: modeCode, sid: sid })

                });
            });
        })
    }

    // 载入战场
    loadField(sidJson, i) {

        if (i === undefined) i = 0;

        let keys = Object.keys(sidJson);

        if (i < keys.length) {

            let sid = keys[i];

            this.rds.hget(sid, "username", (err1, data1) => {

                let username = data1;

                this.rds.hget(username, "roleState", (err2, data2) => {

                    let roleState = JSON.parse(data2);

                    roleState.sid = sid;

                    if (i < keys.length / 2) {

                        roleState.isLeftAlly = true;

                    } else {

                        roleState.isLeftAlly = false;

                    }

                    this.rds.hlen("FieldJson", (err3, data3) => {

                        roleState.inFieldId = data3;

                        this.rds.hset(sid, "fieldId", roleState.inFieldId.toString());

                        this.rds.hset(roleState.username, "roleState", JSON.stringify(roleState));

                        sidJson[sid] = roleState;

                        i += 1;

                        this.loadField(sidJson, i);

                    });
                });
            });

        } else {

            let promise = new Promise((res, rej) => {

                let roleState = Object.values(sidJson)[0];

                let fieldInfo;

                if (roleState.inFieldId < 0) {

                    fieldInfo = new FieldInfo(0, roleState.inComparingMode, sidJson);

                    res(fieldInfo);

                } else {

                    fieldInfo = new FieldInfo(roleState.inFieldId, roleState.inComparingMode, sidJson);

                    res(fieldInfo);

                }

            });

            promise.then((data) => {

                let fieldInfo = data;

                this.rds.hset("FieldJson", fieldInfo.fieldId.toString(), JSON.stringify(fieldInfo));

                process.send({ eventName: "CompareSuccess", dataString: JSON.stringify(fieldInfo), sid: "" });

            });
        }
    }
}

module.exports = CompareApp;