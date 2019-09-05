var event = require("events");
var cluster = require("cluster");
var Redis = require("redis");

var CuteUDP = require("./CuteUDP/CuteUDP");

// 主进程，负责 I/O 调度
class SocketApp extends event {

    constructor(redisPort, remoteIp, remotePort, localPort) {

        super();

        SocketApp.instance = this;

        SocketApp.hallCluster = cluster.workers["1"];

        SocketApp.compareCluster = cluster.workers["2"];

        SocketApp.battleCluster = cluster.workers["3"];

        this.cuteUDP = new CuteUDP(remoteIp, remotePort, localPort);

        this.rds = Redis.createClient(redisPort, "localhost");

        this.delayTime = 25;

        this.initClusterListener();

        this.initCuteUDPListener();

        this.initMainClusterListener();

        this.openServer(1);

    }

    openServer(serverCount, i) {

        let serverNameList = ["枪林弹雨", "独木难支", "百步穿杨", "固若金汤"];

        if (i === undefined) i = 0;

        serverCount = (serverCount === undefined) ? serverNameList.length : serverCount;

        if (i < serverCount) {

            this.rds.HSET("serverList", i.toString(), serverNameList[i]);

            i += 1;

            this.openServer(serverCount, i);

        } else {

            console.log(serverCount, "个服务器开启");

        }
    }

    // 初始化进程监听器
    initClusterListener() {

        cluster.on("message", (worker, msg) => {

            let eventName = msg.eventName;

            let dataString = msg.dataString;

            let sid = msg.sid;

            this.emit(eventName, dataString, sid);

            // console.log("SocketApp Recv : ", msg);

        });

    }

    // 初始化 CuteUDP 监听事件
    initCuteUDPListener() {

        // Hall Cluster 负责处理
        // 注册
        this.cuteUDP.on("Register", (dataString, sid) => { this.clusterReq("hall", dataString, sid, "Register"); }); 

        // 登录
        this.cuteUDP.on("Login", (dataString, sid) => { this.clusterReq("hall", dataString, sid, "Login"); }); 

        // 显示角色列表
        this.cuteUDP.on("ShowRoles", (dataString, sid) => { this.clusterReq("hall", dataString, sid, "ShowRoles"); }); 

        // 创建角色
        this.cuteUDP.on("CreateRole", (dataString, sid) => { this.clusterReq("hall", dataString, sid, "CreateRole"); }); 

        // 删除角色
        this.cuteUDP.on("DeleteRole", (dataString, sid) => { this.clusterReq("hall", dataString, sid, "DeleteRole"); }); 

        // 选定角色后进入游戏
        this.cuteUDP.on("EnterGame", (dataString, sid) => { this.clusterReq("hall", dataString, sid, "EnterGame"); }); 

        // 显示服务器列表
        this.cuteUDP.on("ShowServer", (dataString, sid) => { this.clusterReq("hall", dataString, sid, "ShowServer"); }); 
        
        // 重新请求角色属性
        this.cuteUDP.on("RequestRoleState", (dataString, sid) => { this.clusterReq("hall", dataString, sid, "RequestRoleState"); }); 

        // 显示服务器自定义房间
        // this.cuteUDP.on("ShowRoom", (dataString, sid) => { this.clusterReq("hall", dataString, sid, "ShowRoom"); }); 

        // Compare Cluster 负责处理
        // 匹配申请
        this.cuteUDP.on("Compare", (dataString, sid) => { this.clusterReq("compare", dataString, sid, "Compare"); }); 

        this.cuteUDP.on("CancelCompare", (dataString, sid) => { this.clusterReq("CancelCompare", dataString, sid, "CancelCompare"); }); 

        // Battle Cluster 负责处理

        this.cuteUDP.on("CastSkill", (dataString, sid) => { this.clusterReq("battle", dataString, sid, "CastSkill"); }); 

        this.cuteUDP.on("ReflectBullet", (dataString, sid) => { this.clusterReq("battle", dataString, sid, "ReflectBullet"); }); 
        
        this.cuteUDP.on("ImmuneBullet", (dataString, sid) => { this.clusterReq("battle", dataString, sid, "ImmuneBullet"); }); 
        
        this.cuteUDP.on("KillBullet", (dataString, sid) => { this.clusterReq("battle", dataString, sid, "KillBullet"); }); 
        
        this.cuteUDP.on("BlockBullet", (dataString, sid) => { this.clusterReq("battle", dataString, sid, "BlockBullet"); }); 

        this.cuteUDP.on("BeAttacked", (dataString, sid) => { this.clusterReq("battle", dataString, sid, "BeAttacked"); }); 

        this.cuteUDP.on("Dead", (dataString, sid) => { this.clusterReq("battle", dataString, sid, "Dead"); }); 

    }

    // 初始化主进程监听事件
    initMainClusterListener() {

        this.on("error", (err) => {

            console.log("SocketApp Event Error:", err);

        })

        // ———— Hall Cluster ————
        // 注册
        this.on("Register", (dataString, sid) => { this.socketRes(dataString, sid, "Register"); });

        // 登录
        this.on("Login", (dataString, sid) => { this.socketRes(dataString, sid, "Login"); });

        // 显示服务器
        this.on("ShowServer", (dataString, sid) => { this.socketRes(dataString, sid, "ShowServer"); });

        // 显示角色
        this.on("ShowRoles", (dataString, sid) => { this.socketRes(dataString, sid, "ShowRoles"); });

        // 创建角色 成功
        this.on("CreateRole", (dataString, sid) => { this.socketRes(dataString, sid, "CreateRole"); });

        // 创建角色 失败
        this.on("CreateRoleFail", (dataString, sid) => { this.socketRes(dataString, sid, "CreateRoleFail"); });

        // 删除角色
        this.on("DeleteRole", (dataString, sid) => { this.socketRes(dataString, sid, "DeleteRole"); });

        // 进入游戏
        this.on("EnterGame", (dataString, sid) => { this.socketRes(dataString, sid, "EnterGame"); });

        // 重新请求角色属性
        this.on("RequestRoleState", (dataString, sid) => { this.socketRes(dataString, sid, "RequestRoleState"); });

        // 显示服务器自定义房间
        // this.on("ShowRoom", (dataString, sid) => { this.socketRes(dataString, sid, "ShowRoom"); });

        // ———— Compare Cluster ————
        this.on("CompareWait", (dataString, sid) => { this.socketRes(dataString, sid, "CompareWait"); });

        // 匹配成功
        this.on("CompareSuccess", (dataString, sid1) => {

            this.clusterReq("battle", dataString, sid1, "LoadFieldJson");

            this.on("LoadFieldSuccess", (dataJson, sid2) => {

                this.battleSocketRes(dataJson, sid2, "CompareSuccess");

            })
        });

        this.on("CancelCompare", (dataString, sid) => { this.socketRes(dataString, sid, "CancelCompare"); });

        // ———— Battle Cluster ————
        // 移动
        this.on("CastSkill", (dataJson, sid) => { this.battleSocketRes(dataJson, sid, "CastSkill"); });

        this.on("ReflectBullet", (dataJson, sid) => { this.battleSocketRes(dataJson, sid, "ReflectBullet"); });

        this.on("ImmuneBullet", (dataJson, sid) => { this.battleSocketRes(dataJson, sid, "ImmuneBullet"); });

        this.on("KillBullet", (dataJson, sid) => { this.battleSocketRes(dataJson, sid, "KillBullet"); });

        this.on("BlockBullet", (dataJson, sid) => { this.battleSocketRes(dataJson, sid, "BlockBullet"); });

        this.on("BeAttacked", (dataJson, sid) => { this.battleSocketRes(dataJson, sid, "BeAttacked"); });

        this.on("Dead", (dataJson, sid) => { this.battleSocketRes(dataJson, sid, "Dead"); });

        this.on("GameOver", (dataJson, sid) => { this.battleSocketRes(dataJson, sid, "GameOver"); });

    }

    // 进程请求
    clusterReq(clusterName, dataString, sid, eventName) {

        // process.nextTick(() => {

            switch(clusterName) {

                case "hall": SocketApp.hallCluster.send({eventName: eventName, dataString: dataString, sid: sid}); break;

                case "compare": SocketApp.compareCluster.send({eventName: eventName, dataString: dataString, sid: sid}); break;

                case "battle": SocketApp.battleCluster.send({eventName: eventName, dataString: dataString, sid: sid}); break;

                default: console.log("进程名错误"); break;

            }

            console.log(sid, "向", clusterName, "请求处理", eventName);

        // })
    }

    // 单人广播 网络回应
    socketRes(dataString, sid, eventName) {

        process.nextTick(() => {

            console.log("向", sid, "回传", eventName);

            this.cuteUDP.emitBackTo(eventName, dataString, sid);

        })
    }

    // 多人广播 网络回应
    battleSocketRes(dataJson, sid, eventName) {

        process.nextTick(() => {

            let data = dataJson.data;

            let sidArray = dataJson.sidArray;

            this.cuteUDP.emitBrocast(eventName, data, sidArray);

            console.log("向", sidArray, "回传", eventName);
                
        })
    }
}

module.exports = SocketApp;