var event = require("events");
var cluster = require("cluster");

var BATTLE_GD = require("./GlobalData/BATTLE_GD");
var FieldInfo = require("./AllClass/FieldInfo");

class BattleApp extends event {

    constructor() {

        super();

        BattleApp.instance = this;

        this.initClusterListener();

        this.initBattleClusterListener();

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

        this.on("error", (err) => { console.log("BattleApp Event Error:", err); })

        this.on("InitBattleGD", (dataString, sid) => { BATTLE_GD.CreateGD(); }); // 初始化全局变量

        this.on("BattleLoadField", this.battleLoadField); // 匹配成功，载入战场

        this.on("RequestSidJson", this.requestSidJson); // 请求 sidJson 

        this.on("Move", this.move); // 玩家移动

        this.on("CancelMove", (dataString, sid) => { this.requestSidJson(dataString, sid, "CancelMove"); }); // 玩家取消移动

        this.on("Block", (dataString, sid) => { this.requestSidJson(dataString, sid, "Block"); }); // 玩家格挡

        this.on("CancelBlock", (dataString, sid) => { this.requestSidJson(dataString, sid, "CancelBlock"); }); // 玩家取消格挡

        this.on("PerfectBlock", (dataString, sid) => { this.requestSidJson(dataString, sid, "PerfectBlock"); }); // 玩家完美格挡

        this.on("CancelPerfectBlock", (dataString, sid) => { this.requestSidJson(dataString, sid, "CancelPerfectBlock"); }); // 玩家取消完美格挡

        this.on("Shoot", this.shoot); // 玩家射击

        this.on("PerfectBlockBullet", this.perfectBlockBullet); // 玩家完美格挡了子弹

        this.on("BlockBullet", this.blockBullet); // 玩家普通格挡了子弹

        this.on("BeAttacked", this.beAttacked); // 子弹直接命中

    }

    // 匹配成功，载入战场
    // dataString = sidJson 键值对为 sid : roleState
    battleLoadField(dataString, sid) {

        process.nextTick(() => {

            let sidJson = dataString;

            let index = BATTLE_GD.FIELD_JSON.length;

            let modeCode; // str

            let i = 0;

            for (let _sid in sidJson) {

                let role = sidJson[_sid];

                if(modeCode === undefined) modeCode = role.inComparingMode;

                role.inFieldId = index;

                BATTLE_GD.ONLINE_ROLE[_sid] = role;

                let len = Object.keys(sidJson).length;

                if (i < len / 2) {

                    role.isLeftAlly = true;

                } else {

                    role.isLeftAlly = false;

                }

                i += 1;
            }

            BATTLE_GD.FIELD_JSON.push(new FieldInfo(index, modeCode, sidJson));

            // 传FieldInfo回去
            process.send({ eventName: "BattleLoadField", dataString : BATTLE_GD.FIELD_JSON[index], sid: "" });
                    
        })
    }

    // 请求 sidJson
    requestSidJson(dataString, sid, eventName) {

        process.nextTick(() => {

            let role = BATTLE_GD.ONLINE_ROLE[sid];

            let fieldIndex = role.inFieldId;

            let sidJson = BATTLE_GD.FIELD_JSON[fieldIndex].sidJson;

            let sidArray = Object.keys(sidJson);

            process.send({ eventName: eventName, dataString: {data: sid, sidArray: sidArray}, sid: sid});

        })
    }

    // 玩家移动
    move(dataString, sid) {

        process.nextTick(() => {

            // console.log("BattleApp move :", dataString);

            let moveInfo = JSON.parse(dataString); // moveInfo = 谁要移动，移到哪里

            if (BATTLE_GD.ONLINE_ROLE[sid]) {

                let role = BATTLE_GD.ONLINE_ROLE[sid]; // 请求移动的玩家（玩家列表变量内）

                let fieldIndex = role.inFieldId; // 玩家所在战场ID

                let sidJson = BATTLE_GD.FIELD_JSON[fieldIndex].sidJson;

                let fieldRole = sidJson[sid]; // 玩家（战场变量内）

                fieldRole.vecArray = moveInfo.v; // 玩家坐标修改为新上传坐标

                let sidArray = Object.keys(sidJson);

                process.send({ eventName: "Move", dataString : {data: JSON.stringify(moveInfo), sidArray: sidArray}, sid: sid });

            }
        })
    }

    // 子弹的处理方式
    bulletEvent(dataString, sid, eventName) {

        process.nextTick(() => {

            let bulletInfo = JSON.parse(dataString);

            let role = BATTLE_GD.ONLINE_ROLE[sid];

            let fieldIndex = role.inFieldId;

            let sidJson = BATTLE_GD.FIELD_JSON[fieldIndex].sidJson;

            bulletInfo.sid = sid;

            if (eventName === "PerfectBlockBullet" || eventName === "Shoot") {

                let bidJson = BATTLE_GD.FIELD_JSON[fieldIndex].bidJson;

                bidJson[bulletInfo.bid] = bulletInfo;

            } else if (eventName === "BlockBullet") {

                sidJson[sid].blockLife -= bulletInfo.dmg;

                delete BATTLE_GD.FIELD_JSON[fieldIndex].bidJson[bulletInfo.bid];

            } else if (eventName === "BeAttacked") {

                sidJson[sid].life -= bulletInfo.dmg;

                delete BATTLE_GD.FIELD_JSON[fieldIndex].bidJson[bulletInfo.bid];

            }

            let sidArray = Object.keys(sidJson);

            process.send({ eventName: eventName, dataString: {data: JSON.stringify(bulletInfo), sidArray: sidArray} , sid: sid});

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

}

module.exports = BattleApp;