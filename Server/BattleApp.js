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

    }

    // 匹配成功，载入战场
    battleLoadField(dataString, sid) {

        process.nextTick(() => {

            let roleArray = dataString;

            let index = BATTLE_GD.FIELD_JSON.length;

            for (let i in roleArray) {

                let role = roleArray[i];

                role.inFieldId = index;

                role.inRoleArrayIndex = i;

                BATTLE_GD.ONLINE_ROLE[role.sid] = role;

                if (i < roleArray.length / 2) {

                    role.isLeftAlly = true;

                } else {

                    role.isLeftAlly = false;

                }
            }

            let modeCode = roleArray[0].inComparingMode;

            BATTLE_GD.FIELD_JSON.push(new FieldInfo(index, modeCode, roleArray));

            // 传FieldInfo回去
            process.send({ eventName: "BattleLoadField", dataString : BATTLE_GD.FIELD_JSON[index], sid: "" });
                    
        })
    }

    // 玩家移动
    // 返回FieldInfo内的role
    battleMove(dataString, sid) {

        process.nextTick(() => {

            // console.log("BattleApp move :", dataString);

            let moveInfo = JSON.parse(dataString);

            if (BATTLE_GD.ONLINE_ROLE[sid]) {

                let role = BATTLE_GD.ONLINE_ROLE[sid];

                let fieldIndex = role.inFieldId;

                let fieldRole = BATTLE_GD.FIELD_JSON[fieldIndex].roleArray[role.inRoleArrayIndex];

                fieldRole.vecArray = moveInfo.v;

                process.send({ eventName: "BattleMove", dataString : JSON.stringify(BATTLE_GD.FIELD_JSON[fieldIndex]), sid: sid });

            }
        })
    }
}

module.exports = BattleApp;