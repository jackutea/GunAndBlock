var event = require("events");
var cluster = require("cluster");
var Redis = require("redis");
var mongoDB = require("./mongoDB/mongoDB");

var DefaultLevel = require("./Config/DefaultLevel");
var FrameQueue = require("./AllClass/FrameQueue");

class BattleApp extends event {

    constructor(redisPort, redisIp) {

        super();

        BattleApp.instance = this;

        this.rds = Redis.createClient(redisPort, redisIp);

        this.frameTime = 18;

        this.initClusterListener();

        this.initBattleClusterListener();

        this.FieldJson = {};

        this.frameList = [];

        this.delayFunc(this);

    }

    // 帧同步
    delayFunc(instance) {

        new Promise((res, rej) => {

            process.nextTick(() => {

                for (let i = 0; i < instance.frameList.length; i += 1) {

                    let nowFrame = instance.frameList.shift();

                    if (nowFrame !== undefined) {

                        let eventName = nowFrame.eventName;

                        let dataString = nowFrame.dataString;

                        let sid = nowFrame.sid;

                        process.send({ eventName: eventName, dataString: dataString, sid: sid});

                    } else {

                        break;

                    }
                }

                res();

            });
            
        }).then(() => {

            setTimeout(instance.delayFunc, instance.frameTime, instance);

        })
    }

    // 初始化进程监听器
    initClusterListener() {

        cluster.worker.on("message", (msg) => {

            let eventName = msg.eventName;

            let dataString = msg.dataString;

            let sid = msg.sid;

            this.emit(eventName, dataString, sid);

            // console.log("Battle App Recv : ", msg);

        });

        cluster.worker.on("error", (err) => {

            console.log("BattleApp Error :", err);
            
        });

    }

    // 初始化进程监听事件
    initBattleClusterListener() {

        this.on("error", (err) => { console.log("BattleApp Event Error:", err); });

        this.on("LoadFieldJson", this.loadFieldJson);

        this.on("CastSkill", this.castSkill); // 玩家施放技能

        this.on("ReflectBullet", (dataString, sid) => { this.bulletEvent(dataString, sid, "ReflectBullet"); }); // 玩家完美格挡了子弹

        this.on("ImmuneBullet", (dataString, sid) => { this.bulletEvent(dataString, sid, "ImmuneBullet"); }); // 玩家免疫了子弹

        this.on("KillBullet", (dataString, sid) => { this.bulletEvent(dataString, sid, "KillBullet"); }); // 子弹击中护盾

        this.on("BlockBullet", (dataString, sid) => { this.bulletEvent(dataString, sid, "BlockBullet"); }); // 玩家普通格挡了子弹

        this.on("BeAttacked", (dataString, sid) => { this.bulletEvent(dataString, sid, "BeAttacked"); }); // 子弹直接命中

        this.on("Dead", this.dead); // dead

    }

    // 新增战场数据
    // dataString = FieldInfo
    loadFieldJson(dataString, sid) {

        process.nextTick(() => {

            let fieldInfo = JSON.parse(dataString);

            this.FieldJson[fieldInfo.fieldId] = fieldInfo;

            let sidJson = fieldInfo.sidJson;

            let sidArray = Object.keys(sidJson);

            this.frameList.push(new FrameQueue("LoadFieldSuccess", {data: dataString, sidArray: sidArray}, sid));

            // process.send({ eventName: "LoadFieldSuccess", dataString: {data: dataString, sidArray: sidArray}, sid: sid});

        });
    }

    getRedisSidJson(sid, callback) {

        process.nextTick(() => {

            new Promise((res, rej) => {

                this.rds.hget(sid, "sidJson", (err, data) => {

                    if (data !== null) {

                        let sidJson = data;

                        res(sidJson);

                    } else {

                        res(false);

                    }

                });

            }).then((data) => {

                if (data === false) {

                    this.rds.hget(sid, "username", (err0, data0) => {

                        let username = data0;
        
                        this.rds.hget(username, "roleState", (err1, data1) => {

                            let role = JSON.parse(data1);
        
                            let fieldIndex = role.inFieldId;

                            let fieldInfo = this.FieldJson[fieldIndex];

                            let sidJson = fieldInfo.sidJson;
        
                            this.rds.hset(sid, "sidJson", JSON.stringify(sidJson))

                            callback(sidJson);

                        });
                    })

                } else {

                    let sidJson = JSON.parse(data);

                    callback(sidJson);

                }
            });
        })
    }

    // 子弹击中的处理方式
    // dataString = bid
    bulletEvent(dataString, sid, eventName) {

        process.nextTick(() => {

            let bid = dataString;

            let json = {

                bid: bid,

                sid: sid,
            }

            this.getRedisSidJson(sid, (sidJson) => {

                let sidArray = Object.keys(sidJson);

                this.frameList.push(new FrameQueue(eventName, {data: JSON.stringify(json), sidArray: sidArray}, sid));

                // process.send({ eventName: eventName, dataString: {data: JSON.stringify(json), sidArray: sidArray} , sid: sid});

            });
        });
    }

    // 施放技能
    // dataString = skillEnum
    castSkill(dataString, sid) {

        process.nextTick(() => {

            let skillEnum = parseInt(dataString);

            let json = {

                skillEnum: skillEnum,
                
                sid: sid,

                timeSample: (new Date()).valueOf().toString()

            }

            this.getRedisSidJson(sid, (sidJson) => {

                let sidArray = Object.keys(sidJson);

                this.frameList.push(new FrameQueue("CastSkill", {data: JSON.stringify(json), sidArray: sidArray}, sid));

                // process.send({ eventName: "CastSkill", dataString: {data: JSON.stringify(json), sidArray: sidArray} , sid: sid});

            });

        });
    }

    // dead
    // dataString = ""
    dead(dataString, sid) {

        process.nextTick(() => {

            this.rds.hget(sid, "fieldId", (err0, data0) => {

                let fieldId = data0; // 获取死亡角色所在的 战场id

                let sidJson = this.FieldJson[fieldId].sidJson // 战场的所有玩家信息

                sidJson[sid].isDead = true;

                let sidArray = Object.keys(sidJson);

                if (sidJson[sid].isLeftAlly) {

                    this.FieldJson[fieldId].leftLive -= 1; // 如果死亡角色属于左边

                } else {

                    this.FieldJson[fieldId].rightLive -= 1;

                }

                this.frameList.push(new FrameQueue("Dead", {data: sid, sidArray: sidArray}, sid));

                // 处理胜负情况
                if (this.FieldJson[fieldId].leftLive <= 0) {

                    // 左败 游戏结束
                    console.log("左败");

                    new Promise((res, rej) => {

                        this.insertGameResult(sidJson, false, 0);

                        res();

                    }).then(() => {

                        this.frameList.push(new FrameQueue("GameOver", {data: 0, sidArray: sidArray}, sid)); // 推送游戏结束给客户端 0 为左败

                    })

                } else if (this.FieldJson[fieldId].rightLive <= 0) {

                    // 右败 游戏结束
                    console.log("右败");

                    new Promise((res, rej) => {

                        this.insertGameResult(sidJson, true, 0);

                        res();

                    }).then(() => {

                        this.frameList.push(new FrameQueue("GameOver", {data: 0, sidArray: sidArray}, sid)); // 推送游戏结束给客户端 0 为左败

                    })

                } else {

                    // 未分胜负
                    console.log("有角色死亡，但未分胜负");

                }

                console.log("fieldjson", this.FieldJson);
                // console.log("sidJson[sid]", sidJson[sid]);

                // process.send({ eventName: "Dead", dataString: {data: sid, sidArray: sidArray}, sid: sid});

            });
        })
    }

    // 将游戏结果插入数据库
    insertGameResult(sidJson, isLeftWin, index) {

        let winScore = 10;

        let winExp = 10;

        let sidList = Object.keys(sidJson);

        if (index < sidList.length) {

            let sid = sidList[index];

            let roleState = sidJson[sid];

            if (!isLeftWin) {

                if (roleState.isLeftAlly) {

                    if (roleState.level >= DefaultLevel.levelExpRequireList.length) {

                        roleState.exp = DefaultLevel.levelExpRequireList.reverse()[0];

                        roleState.score -= winScore;

                        roleState.rank = this.growRank(roleState.score);

                    }
    
                } else {

                    if (roleState.level >= DefaultLevel.levelExpRequireList.length) {

                        roleState.score += winScore;

                        roleState.exp = DefaultLevel.levelExpRequireList.reverse()[0];

                        roleState.rank = this.growRank(roleState.score);

                    } else {

                        roleState.exp += winExp;

                        roleState.level = this.growLevel(roleState.exp);

                    }
                }

            } else {

                if (roleState.isLeftAlly) {

                    if (roleState.level >= DefaultLevel.levelExpRequireList.length) {

                        roleState.score += winScore;

                        roleState.exp = DefaultLevel.levelExpRequireList.reverse()[0];

                        roleState.rank = this.growRank(roleState.score);

                    } else {

                        roleState.exp += winExp;

                        roleState.level = this.growLevel(roleState.exp);

                    }
    
                } else {
    
                    if (roleState.level >= DefaultLevel.levelExpRequireList.length) {

                        roleState.exp = DefaultLevel.levelExpRequireList.reverse()[0];

                        roleState.score -= winScore;

                        roleState.rank = this.growRank(roleState.score);

                    }
    
                }
            }

            let insertData = { score: roleState.score, exp: roleState.exp, level: roleState.level, rank: roleState.rank };

            mongoDB.updateOne("role", { username: roleState.username }, insertData, (err, result) => {

                // console.log(result);

                index += 1;

                this.insertGameResult(sidJson, isLeftWin, index, () => {});

            })
            
        }
    }

    // 升级
    growLevel(exp) {

        for (let i = 0; i < DefaultLevel.levelExpRequireList.length; i += 1) {

            let requireExp = DefaultLevel.levelExpRequireList[i];

            if (exp < requireExp) {

                return i;

            }
        }

    }

    // 升降段
    growRank(score) {

        for (let i = 0; i < DefaultLevel.rankScoreRequireList.length; i += 1) {

            let requireScore = DefaultLevel.rankScoreRequireList[i];

            if (score < requireScore) {

                return i;

            }
        }
    }
}

module.exports = BattleApp;