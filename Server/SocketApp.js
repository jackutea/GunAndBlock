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

            this.rds.hset("serverList", "serverId", i.toString());

            this.rds.hset("serverList", "serverName", serverNameList[i]);

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

        // 显示服务器自定义房间
        // this.cuteUDP.on("ShowRoom", (dataString, sid) => { this.clusterReq("hall", dataString, sid, "ShowRoom"); }); 

        // Compare Cluster 负责处理
        // 匹配申请
        this.cuteUDP.on("Compare", (dataString, sid) => { this.clusterReq("compare", dataString, sid, "Compare"); }); 

        // Battle Cluster 负责处理
        this.cuteUDP.on("Move", (dataString, sid) => { this.clusterReq("battle", dataString, sid, "Move"); }); 

        this.cuteUDP.on("CancelMove", (dataString, sid) => { this.clusterReq("battle", dataString, sid, "CancelMove"); }); 

        this.cuteUDP.on("RedBlock", (dataString, sid) => { this.clusterReq("battle", dataString, sid, "RedBlock"); }); 
        this.cuteUDP.on("BlueBlock", (dataString, sid) => { this.clusterReq("battle", dataString, sid, "BlueBlock"); }); 

        this.cuteUDP.on("CancelRedBlock", (dataString, sid) => { this.clusterReq("battle", dataString, sid, "CancelRedBlock"); }); 
        this.cuteUDP.on("CancelBlueBlock", (dataString, sid) => { this.clusterReq("battle", dataString, sid, "CancelBlueBlock"); }); 

        this.cuteUDP.on("RedPerfectBlock", (dataString, sid) => { this.clusterReq("battle", dataString, sid, "RedPerfectBlock"); }); 
        this.cuteUDP.on("BluePerfectBlock", (dataString, sid) => { this.clusterReq("battle", dataString, sid, "BluePerfectBlock"); }); 

        this.cuteUDP.on("CancelRedPerfectBlock", (dataString, sid) => { this.clusterReq("battle", dataString, sid, "CancelRedPerfectBlock"); }); 
        this.cuteUDP.on("CancelBluePerfectBlock", (dataString, sid) => { this.clusterReq("battle", dataString, sid, "CancelBluePerfectBlock"); }); 

        this.cuteUDP.on("Shoot", (dataString, sid) => { this.clusterReq("battle", dataString, sid, "Shoot"); }); 

        this.cuteUDP.on("PerfectBlockBullet", (dataString, sid) => { this.clusterReq("battle", dataString, sid, "PerfectBlockBullet"); }); 

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

        // 显示服务器自定义房间
        // this.on("ShowRoom", (dataString, sid) => { this.socketRes(dataString, sid, "ShowRoom"); });

        // ———— Compare Cluster ————
        this.on("CompareWait", (dataString, sid) => { this.socketRes(dataString, sid, "CompareWait"); });

        this.on("CompareSuccess", (dataString, sid1) => {

            this.clusterReq("battle", dataString, sid1, "LoadFieldJson");

            this.on("LoadFieldSuccess", (dataJson, sid2) => {

                this.battleSocketRes(dataJson, sid2, "CompareSuccess");

            })
        });  // 匹配成功

        // ———— Battle Cluster ————
        // 移动
        this.on("Move", (dataJson, sid) => { this.battleSocketRes(dataJson, sid, "Move"); });

        this.on("CancelMove", (dataJson, sid) => { this.battleSocketRes(dataJson, sid, "CancelMove"); });

        this.on("RedBlock", (dataJson, sid) => { this.battleSocketRes(dataJson, sid, "RedBlock"); });
        this.on("BlueBlock", (dataJson, sid) => { this.battleSocketRes(dataJson, sid, "BlueBlock"); });

        this.on("CancelRedBlock", (dataJson, sid) => { this.battleSocketRes(dataJson, sid, "CancelRedBlock"); });
        this.on("CancelBlueBlock", (dataJson, sid) => { this.battleSocketRes(dataJson, sid, "CancelBlueBlock"); });

        this.on("RedPerfectBlock", (dataJson, sid) => { this.battleSocketRes(dataJson, sid, "RedPerfectBlock"); });
        this.on("BluePerfectBlock", (dataJson, sid) => { this.battleSocketRes(dataJson, sid, "BluePerfectBlock"); });

        this.on("CancelRedPerfectBlock", (dataJson, sid) => { this.battleSocketRes(dataJson, sid, "CancelRedPerfectBlock"); });
        this.on("CancelBluePerfectBlock", (dataJson, sid) => { this.battleSocketRes(dataJson, sid, "CancelBluePerfectBlock"); });

        this.on("Shoot", (dataJson, sid) => { this.battleSocketRes(dataJson, sid, "Shoot"); });

        this.on("PerfectBlockBullet", (dataJson, sid) => { this.battleSocketRes(dataJson, sid, "PerfectBlockBullet"); });

        this.on("BlockBullet", (dataJson, sid) => { this.battleSocketRes(dataJson, sid, "BlockBullet"); });

        this.on("BeAttacked", (dataJson, sid) => { this.battleSocketRes(dataJson, sid, "BeAttacked"); });

        this.on("Dead", (dataJson, sid) => { this.battleSocketRes(dataJson, sid, "Dead"); });

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

    // 网络回应
    socketRes(dataString, sid, eventName) {

        process.nextTick(() => {

            console.log("向", sid, "回传", eventName);

            this.cuteUDP.emitBackTo(eventName, dataString, sid);

        })
    }

    // 战斗 网络回应
    battleSocketRes(dataJson, sid, eventName) {

        process.nextTick(() => {

            let data = dataJson.data;

            let sidArray = dataJson.sidArray;

            this.cuteUDP.emitBrocast(eventName, data, sidArray, sid);

            console.log("向", sid, "回传", eventName);
                
        })
    }
}

module.exports = SocketApp;