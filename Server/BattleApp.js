var event = require("events");
var cluster = require("cluster");
var Redis = require("redis");

class BattleApp extends event {

    constructor(redisPort, redisIp) {

        super();

        BattleApp.instance = this;

        this.rds = Redis.createClient(redisPort, redisIp);

        this.delayTime = 20;

        this.initClusterListener();

        this.initBattleClusterListener();

        this.FieldJson = {};

        setInterval(() => {

            if (this.delayTime > 0) {

                this.delayTime -= 1;

            }
            
        }, 1);

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

        this.on("Move", this.move); // 玩家移动

        this.on("CancelMove", (dataString, sid) => { this.requestSidJson(dataString, sid, "CancelMove"); }); // 玩家取消移动

        this.on("RedBlock", (dataString, sid) => { this.requestSidJson(dataString, sid, "Block"); }); // 玩家格挡
        this.on("BlueBlock", (dataString, sid) => { this.requestSidJson(dataString, sid, "Block"); }); // 玩家格挡

        this.on("CancelRedBlock", (dataString, sid) => { this.requestSidJson(dataString, sid, "CancelRedBlock"); }); // 玩家取消格挡
        this.on("CancelBlueBlock", (dataString, sid) => { this.requestSidJson(dataString, sid, "CancelBlueBlock"); }); // 玩家取消格挡

        this.on("RedPerfectBlock", (dataString, sid) => { this.requestSidJson(dataString, sid, "RedPerfectBlock"); }); // 玩家完美格挡
        this.on("BluePerfectBlock", (dataString, sid) => { this.requestSidJson(dataString, sid, "BluePerfectBlock"); }); // 玩家完美格挡

        this.on("CancelRedPerfectBlock", (dataString, sid) => { this.requestSidJson(dataString, sid, "CancelRedPerfectBlock"); }); // 玩家取消完美格挡
        this.on("CancelBluePerfectBlock", (dataString, sid) => { this.requestSidJson(dataString, sid, "CancelBluePerfectBlock"); }); // 玩家取消完美格挡

        this.on("Shoot", this.shoot); // 玩家射击

        this.on("PerfectBlockBullet", this.perfectBlockBullet); // 玩家完美格挡了子弹

        this.on("BlockBullet", this.blockBullet); // 玩家普通格挡了子弹

        this.on("BeAttacked", this.beAttacked); // 子弹直接命中

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

            process.send({ eventName: "LoadFieldSuccess", dataString: {data: dataString, sidArray: sidArray}, sid: sid});

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

    // 请求 sidJson
    requestSidJson(dataString, sid, eventName) {

        process.nextTick(() => {

            this.getRedisSidJson(sid, (sidJson) => {

                let sidArray = Object.keys(sidJson);

                process.send({ eventName: eventName, dataString: {data: sid, sidArray: sidArray}, sid: sid});

            });
        })
    }

    // 玩家移动
    move(dataString, sid) {

        process.nextTick(() => {

            // console.log("BattleApp move :", dataString);

            if (this.delayTime <= 0) {

                this.getRedisSidJson(sid, (sidJson) => {

                    let sidArray = Object.keys(sidJson);
    
                    process.send({ eventName: "Move", dataString : {data: dataString, sidArray: sidArray}, sid: sid });
    
                })
            }
        })
    }

    // 子弹的处理方式
    bulletEvent(dataString, sid, eventName) {

        process.nextTick(() => {

            let bulletInfo = JSON.parse(dataString);

            bulletInfo.sid = sid;

            this.getRedisSidJson(sid, (sidJson) => {

                let sidArray = Object.keys(sidJson);

                process.send({ eventName: eventName, dataString: {data: JSON.stringify(bulletInfo), sidArray: sidArray} , sid: sid});

            })
        })
    }

    // 玩家射击
    // dataString = class BulletInfo
    shoot(dataString, sid) { this.bulletEvent(dataString, sid, "Shoot"); }

    // 玩家完美格挡了子弹
    // dataString = BulletInfo
    perfectBlockBullet(dataString, sid) { this.bulletEvent(dataString, sid, "PerfectBlockBullet"); }

    // 玩家普通格挡了子弹
    // dataString = BulletInfo
    blockBullet(dataString, sid) { this.bulletEvent(dataString, sid, "BlockBullet"); }

    // 被直接击中
    // dataString = BulletInfo
    beAttacked(dataString, sid) { this.bulletEvent(dataString, sid, "BeAttacked"); }

    // dead
    // dataString = ""
    dead(dataString, sid) {

        process.nextTick(() => {

            this.rds.hget(sid, "fieldId", (err0, data0) => {

                let fieldId = data0;

                let sidJson = this.FieldJson[fieldId].sidJson

                sidJson[sid].isDead = true;

                let sidArray = Object.keys(sidJson);

                process.send({ eventName: "Dead", dataString: {data: sid, sidArray: sidArray}, sid: sid});

            });
        })
    }
}

module.exports = BattleApp;