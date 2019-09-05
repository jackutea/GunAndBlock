var event = require("events");
var cluster = require("cluster");
var Redis = require("redis");

var DefaultLevel = require("./Config/DefaultLevel");
var FieldState = require("./AllClass/State/FieldState");

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

                this.compareHeartBeat(this);

            }
        });
    }

    initCompareStack(callback) {

        // 只设两个匹配表，其中一个是未满级者，另一个是满级者（以后再改分段位匹配）
        for (let i = 0; i < 2; i += 1) {

            this.levelStackList[i] = []; // level or rank : Array

            this.levelStackList[i]["0"] = []; // 1V1 列表
            
            this.levelStackList[i]["1"] = []; // 5V5 列表

            this.levelStackList[i]["2"] = []; // 50V50 列表

        }

        callback(true);

    }

    // 匹配循环
    compareHeartBeat(instance) {

        new Promise((res, rej) => {

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

                            instance.emit("LoadField", sidJson, modeCode);

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

                            instance.emit("LoadField", sidJson, modeCode);

                        }

                    } else if (modeCode === "2") {

                        // 如果50V50 模式列表内有超过100人，则拉出100人
                        if (perModeList.length >= 100) {

                            let sidJson = new Object();

                            for (let i = 0; i < 100; i += 1) {

                                let sid = perModeList.shift();

                                sidJson[sid] = {};

                            }

                            instance.emit("LoadField", sidJson, modeCode);

                        }
                    }
                }
            }

            res();

        }).then(() => {

            setTimeout(instance.compareHeartBeat, 15, instance);

        })
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

        this.on("CancelCompare", this.CancelCompare); // 开始匹配

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

                        searchLevelIndex = 0; // 如果未满级，找寻未满级者匹配

                    } else {

                        searchLevelIndex = 1; // 反之找已满级者

                    }

                    let checkExist = searchList[searchLevelIndex][modeCode].indexOf(sid);

                    if (checkExist === -1) {

                        searchList[searchLevelIndex][modeCode].push(sid);

                    }

                    process.send({ eventName: "CompareWait", dataString: modeCode, sid: sid });
                });
            });
        })
    }

    // 取消匹配
    CancelCompare(dataString, sid) {

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

                        searchLevelIndex = 0; // 如果未满级，找寻未满级者匹配

                    } else {

                        searchLevelIndex = 1; // 反之找已满级者

                    }

                    let checkExist = searchList[searchLevelIndex][modeCode].indexOf(sid);

                    if (checkExist !== -1) {

                        searchList[searchLevelIndex][modeCode].splice(checkExist, 1);

                        process.send({ eventName: "CancelCompare", dataString: modeCode, sid: sid })

                    }
                });
            });
        })

    }

    // 载入战场
    loadField(sidJson, modeCode, i) {

        if (i === undefined) i = 0; // 递归计数

        let keys = Object.keys(sidJson); // sid key

        if (i < keys.length) {

            let sid = keys[i]; // sid

            this.rds.hget(sid, "username", (err1, data1) => { // 根据玩家 sid 获取 username

                let username = data1; // 玩家 username

                if (err1) throw err1;

                this.rds.hget(username, "roleState", (err2, data2) => { // 根据 username 获取 角色属性

                    if (err2) throw err2;

                    this.rds.hlen("FieldJson", (err3, data3) => {

                        if (err3) throw err3;

                        let roleState = JSON.parse(data2);

                        roleState.sid = sid;

                        roleState.waitMode = modeCode; // 匹配模式 1v1 5v5 50v50

                        if (i < keys.length / 2) {

                            roleState.isLeftAlly = true; // 设置该局玩家属于哪一边

                        } else {

                            roleState.isLeftAlly = false;

                        }

                        roleState.inFieldId = data3; // 设置玩家所在战场id

                        this.rds.hset(sid, "fieldId", roleState.inFieldId.toString()); // 存入 redis

                        this.rds.hset(roleState.username, "roleState", JSON.stringify(roleState));

                        sidJson[sid] = roleState;

                        i -= -1;

                        this.loadField(sidJson, modeCode, i); // 递归

                    });
                });
            });

        } else {
            
            // 当递归完成之后，生成新战场

            let promise = new Promise((res, rej) => {

                let roleState = Object.values(sidJson)[0];

                let fieldInfo;

                if (roleState.inFieldId < 0) {

                    fieldInfo = new FieldState(0, roleState.waitMode, sidJson);

                    res(fieldInfo);

                } else {

                    fieldInfo = new FieldState(roleState.inFieldId, roleState.waitMode, sidJson);

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