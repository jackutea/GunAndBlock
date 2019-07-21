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

    // 初始化全局变量
    initBattleGD(dataString, sid) {

        BATTLE_GD.CreateGD();

    }

    // 初始化进程监听事件
    initBattleClusterListener() {

        this.on("error", (err) => {

            console.log("BattleApp Event Error:", err);
            
        })

        this.on("InitBattleGD", this.initBattleGD); // 初始化全局变量

        this.on("BattleLoadField", this.battleLoadField); // 匹配成功，载入战场

        this.on("BattleMove", this.battleMove); // 玩家移动

        this.on("CancelMove", this.cancelMove); // 玩家取消移动

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

    // 玩家移动
    // 返回FieldInfo内的role
    battleMove(dataString, sid) {

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

                process.send({ eventName: "BattleMove", dataString : {moveInfo: moveInfo, sidArray: sidArray}, sid: sid });

            }
        })
    }

    // 玩家取消移动
    // 返回 roleArray
    cancelMove(dataString, sid) {

        process.nextTick(() => {

            let role = BATTLE_GD.ONLINE_ROLE[sid];

            let fieldIndex = role.inFieldId;

            let sidJson = BATTLE_GD.FIELD_JSON[fieldIndex].sidJson;

            let roleArray = Object.keys(sidJson);

            process.send({ eventName: "CancelMove", dataString: JSON.stringify(roleArray), sid: sid});

        })
    }
}

module.exports = BattleApp;