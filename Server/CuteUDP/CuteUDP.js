var event = require("events");
var udp = require("dgram");

var Packet = require("./Packet");
var PacketHeader = require("./PacketHeader");
var BasePacket = require("./BasePacket");
var IpInfo = require("./IpInfo");

class CuteUDP extends event {

    constructor(_remoteIp, _remotePort, _localPort) {

        super();

        CuteUDP.instance = this;

        // 实例化 Socket;
        this.socket = udp.createSocket("udp4");

        // 远程服务端配置
        this.serverIp = _remoteIp;
        
        this.localPort = _localPort; // 本地接收端口
        
        this.remotePort = _remotePort; // 远程接收端口

        // CuteUDP 配置
        this.abortTimeMs = 5000; // 总超时弃发(默认5秒)

        this.perLength = 256;

        Packet.perLength = this.perLength;
        
        this.nowTimeSample; // 当前时间
        
        this.repeatHeaderTimeMs = 20; // 包头 重发延时
        
        this.repeatMiniTimeMs = 5; // 每个小包 重发延时 (默认为 0)

        this.socketIdToIpInfo = {};

        CuteUDP.socketId = Packet.getSocketId();

        // 发送
        this.sendJson = new Object();

        // 接收
        this.recvJson = new Object();

        // 启动 udp
        this.initSocketFunc();

        this.initPrivateFunc();

        // 启动发送心跳
        setTimeout(this.sendHearBeat, 1, this);
    }

    sendHearBeat(instance) {

        // 更新当前时间
        instance.nowTimeSample = (new Date()).valueOf();

        // 待收列表超时检测
        let recvJson = instance.recvJson;

        if (recvJson && Object.keys(recvJson).length > 0) {

            let keyList = Object.keys(recvJson);

            for (let i = 0; i < keyList.length; i += 1) {

                let sid = keyList[i];

                let currentBasePacket = instance.getCurrentPacket(recvJson, sid);

                if (!currentBasePacket) continue;

                if (currentBasePacket.recvMiniSize >= PacketHeader.getArraySize(currentBasePacket.packetHeader.a)) continue;

                let timeGap = parseInt(instance.nowTimeSample - currentBasePacket.recvTimeSample);

                if (timeGap > instance.abortTimeMs) {

                    delete recvJson[sid];

                    console.log("超时", instance.abortTimeMs, "ms，清空", sid, "所有待收包");

                }
            }
        }

        // 待发列表检测（重发包头和小包）
        let sendJson = instance.sendJson;

        if (!sendJson) return;

        let keyList = Object.keys(sendJson);

        if (keyList.length <= 0) return;

        for (let i = 0; i < keyList.length; i += 1) {

            let ipOrSid = keyList[i];

            let currentPacket = instance.getCurrentPacket(sendJson, ipOrSid);

            if (!currentPacket) continue;

            let timeGap = parseInt(instance.nowTimeSample - currentPacket.sendTimeSample);

            if (currentPacket.packetHeaderRecvState == true) {

                if (currentPacket.miniPacketListRecvState == true) {

                    //数据齐全，删掉上个包头
                    delete instance.sendJson[ipOrSid][currentPacket.packetHeader.i];

                    if (Object.keys(instance.sendJson[ipOrSid]).length == 0) {

                        delete instance.sendJson[ipOrSid];

                    }

                } else {

                    // 在一定时间内，重发未被接收的小包
                    if (timeGap % instance.repeatMiniTimeMs == 0) {

                        let o = 0;

                        instance.repeatSendMini(o, currentPacket);
    
                    }

                }

            } else {

                if (timeGap % instance.repeatHeaderTimeMs == 0) {

                    // 隔一段时间 发送包头
                    instance.socket.send(currentPacket.headerBytes, 0, currentPacket.headerBytes.length, currentPacket.toPort, currentPacket.toIp, instance.sendHeaderCallBack);
    
                }
            }

            if (timeGap > instance.abortTimeMs) {

                delete sendJson[ipOrSid];

                console.log("超时", instance.abortTimeMs, "清空 ", ipOrSid, " 要发的包");

                continue;
            }
        }

        setTimeout(instance.sendHearBeat, 1, instance);

    }

    initSocketFunc() {

        this.socket.on("listening", this.onListening);
        
        this.socket.on("error", this.onError);
        
        this.socket.on("message", this.onMessage);
        
        this.socket.on("close", this.onClose);

        this.socket.bind({

            port: this.localPort,

            exclusive: true,

        }, () => {

            console.log(this.localPort, "端口绑定成功");

        });

    }

    initPrivateFunc() {

        this.on("connectOnce", this.onConnectOnce); // 监听初次连接，记录 ip/port 键值对

        this.on("addHeader", this.addHeader); // 收到包头

        this.on("headerRecieved", this.headerRecieved); // 反馈包头确认
        
        this.on("jointPacket", this.jointPacket); // 收到小包，开始拼接，并反馈收到的小包序号
        
        this.on("miniPacketRecieved", this.miniPacketRecieved); // 收到已确认的小包序号
        
        this.on("fullPacketRecieved", this.fullPacketRecieved); // 收到小包齐全确认
        
        this.on("requestWrongMini", this.requestWrongMini); // 收到弃发请求

    }

    // 初次连接
    onConnectOnce(dataString, sid) {

        let remoteIp = this.socketIdToIpInfo[sid].ip;

        let remotePort = this.socketIdToIpInfo[sid].port;

        // 增加 IP / Port 键值对
        console.log("Msg :", dataString, "; From ", remoteIp, ":", remotePort);

    }

    // 向单体发消息（发包头）
    emitTo(eventName, objStr, ipStr, port, sid) {

        process.nextTick(() => {
        
            let ipOrSocketId;

            if (sid !== undefined && this.socketIdToIpInfo[sid]) {

                ipOrSocketId = sid;

                ipStr = this.socketIdToIpInfo[sid].ip;

                port = this.socketIdToIpInfo[sid].port;

            } else {

                ipOrSocketId = ipStr;

                port = (port === undefined) ? this.remotePort : port;

            }

            let sendJson = this.sendJson;

            if (!sendJson[ipOrSocketId])

                sendJson[ipOrSocketId] = new Object();

            let packet = new Packet(eventName, objStr, ipStr, port);

            if (!sendJson[ipOrSocketId][packet.packetHeader.i]) {

                sendJson[ipOrSocketId][packet.packetHeader.i] = packet;

                this.socket.send(packet.headerBytes, packet.toPort, packet.toIp, this.sendHeaderCallBack);

                // console.log("发包头", packet.packetHeader.a, "内容", packet.orginStr);

            }

        })
    }

    // 向 sidArray 列表内所有人广播
    emitBrocast(eventName, objStr, sidArray, notSendMineSid) {
        
        for (let index = 0; index < sidArray.length; index += 1) {

            let sid = sidArray[index];

            if (notSendMineSid) {

                if (sid != notSendMineSid) {

                    this.emitBackTo(eventName, objStr, sid);

                }

            } else {

                this.emitBackTo(eventName, objStr, sid);

            }
        }
    }

    // 回传消息
    emitBackTo(eventName, objStr, sid) {

        try {

            let ip = this.socketIdToIpInfo[sid].ip;

            let port = this.socketIdToIpInfo[sid].port;

            this.emitTo(eventName, objStr, ip, port, sid);

        } catch (err) {

            if (err) throw err;

        }
    }

    // 向服务端发消息
    emitServer(eventName, objStr) {

        this.emitTo(eventName, objStr, this.serverIp);

    }

    // 向发送者反馈状态
    responseState(stateCode, obj, ipStr, ipPort) {

        process.nextTick(() => {
        
            let sendStr = stateCode + CuteUDP.socketId + obj.toString();

            let sendBytes = Buffer.from(sendStr, "utf-8");

            this.socket.send(sendBytes, 0, sendBytes.length, ipPort, ipStr, this.sendResponseStateBack);

            // console.log("发送反馈码：", stateCode, "内容：", obj.toString());
            
        })
    }

    // 循环发小包 递归
    repeatSendMini(index, currentPacket) {

        process.nextTick(() => {
        
            if (index < currentPacket.miniPacketList.length) {

                let mid = currentPacket.miniPacketList[index].i;

                let hasStr = currentPacket.miniPacketCheckList[mid];
            
                if (hasStr !== "1") {

                    this.socket.send(currentPacket.miniPacketList[index], currentPacket.toPort, currentPacket.toIp, (err, byte) => {

                        // console.log("发送小包id", index, "包头", currentPacket.packetHeader.i, " 小包长:", byte);

                        index += 1;

                        this.repeatSendMini(index, currentPacket);

                    });
                }
            }
        })
    }

    // 0 收到新包头
    addHeader(dataString, ipStr, ipPort, sid) {

        // console.log("收到反馈码 0 新包头 id: ", dataString, "来自", ipStr, ":", ipPort);

        process.nextTick(() => {

            let packetHeader = JSON.parse(dataString);

            let recvJson = this.recvJson;

            if (!recvJson[sid]) {

                recvJson[sid] = new Object();
                
            }

            this.socketIdToIpInfo[sid] = new IpInfo(ipStr, ipPort);

            let basePacket = new BasePacket(packetHeader, ipStr, ipPort);

            let headerId = packetHeader.i;
            
            if (!recvJson[sid][headerId]) {

                recvJson[sid][headerId] = basePacket;

            }

            // 如果是空包，直接触发事件
            if (packetHeader.a.length === 0) {

                // 触发自定义事件
                this.emit(packetHeader.n, "", sid);

                delete this.recvJson[sid][packetHeader.i];

                if (Object.keys(this.recvJson[sid]).length == 0) {

                    delete this.recvJson[sid];

                }

                // 发送小包齐全声明
                this.responseState("4", packetHeader.i, ipStr, ipPort);

                return;

            }

            // 反馈收到新包头
            this.responseState("1", headerId, ipStr, ipPort);

        })

    }

    // 1 收到包头确认
    headerRecieved(dataString, ipStr, ipPort, sid) {

        process.nextTick(() => {

            // dataString 即 headerId === PacketHeader.i
            let headerId = parseInt(dataString);

            let sendJson = this.sendJson;

            let ipOrSocketId = (sendJson[sid]) ? sid : ipStr;

            if (sendJson[ipOrSocketId]) {

                if (sendJson[ipOrSocketId][headerId]) {

                    // console.log("收到反馈码 1 包头 :", headerId, "已被对方接收");

                    let currentPacket = sendJson[ipOrSocketId][headerId]

                    currentPacket.packetHeaderRecvState = true;

                    // 开始发小包（先一次性全发）
                    let i = 0;

                    this.repeatSendMini(i, currentPacket);
                    
                } else {

                    // console.log("收到反馈码 1 包头 :", headerId, "，但该包头已处理过");

                }

            } else {

                // console.log("收到反馈码 1 包头 id : " + headerId + "，但这是一个奇怪的IP地址");

            }
        })
    }

    // 2 收到新小包 MiniPacket 开始拼接
    jointPacket(dataString, ipStr, ipPort, sid) {

        process.nextTick(() => {

            let miniPacket = JSON.parse(dataString);

            let currentBasePacket = this.getCurrentPacket(this.recvJson, sid);

            if (currentBasePacket) {

                let ph = currentBasePacket.packetHeader;

                let mid = miniPacket.i;

                let declareSize = ph.a[mid];

                let recvSize = miniPacket.n.length;

                // console.log("接收大小 / 声明大小 ： ", recvSize, "/", declareSize);

                if (recvSize != declareSize) {

                    // TODO 请求补发
                    console.log("请求补发", currentBasePacket.packetHeader.n, "的小包", mid);

                    this.responseState("5", mid, ipStr, ipPort);

                    return;
                }

                // 发送已收到的小包序号
                this.responseState("3", mid, ipStr, ipPort);

                // console.log("收到反馈码 2 新小包", mid, ":", miniPacket.n);

                // 如果未计算过该 小包
                if (currentBasePacket.jointStr[mid] === undefined) {

                    currentBasePacket.recvMiniCount += 1;

                    currentBasePacket.recvMiniSize += miniPacket.n.length;

                    // 在字符组加入该小包
                    currentBasePacket.jointStr[mid] = miniPacket.n;

                }

                // 如果长度和数量已收完，停止接收
                if (currentBasePacket.recvMiniSize >= PacketHeader.getArraySize(currentBasePacket.packetHeader.a)) {

                    if (currentBasePacket.fullStr.length == currentBasePacket.recvMiniSize) {

                        // 不用拼接，直接补发齐全声明
                        this.responseState("4", currentBasePacket.packetHeader.i, ipStr, ipPort);

                        // console.log("补发", currentBasePacket.packetHeader.i, "齐全声明");

                    } else {

                        // 收到所有小包，开始拼接，接完发齐全声明，并触发事件
                        for (let s in currentBasePacket.jointStr) {

                            currentBasePacket.fullStr += currentBasePacket.jointStr[s];

                        }

                        // 触发自定义事件
                        this.emit(currentBasePacket.packetHeader.n, currentBasePacket.fullStr, sid);

                        // 触发完删除旧包头
                        delete this.recvJson[sid][currentBasePacket.packetHeader.i];

                        if (Object.keys(this.recvJson[sid]).length == 0) {

                            delete this.recvJson[sid];
                            
                        }

                        // 发送小包齐全声明
                        this.responseState("4", currentBasePacket.packetHeader.i, ipStr, ipPort);

                        // console.log("收到", currentBasePacket.packetHeader.i, "的所有小包");
                    }
                }
            }
        })
    }

    // 3 收到小包序号反馈 mid
    miniPacketRecieved(dataString, ipStr, ipPort, sid) {

        process.nextTick(() => {

            // console.log("收到反馈码 3 小包序号 mid: ", dataString);

            let mid = parseInt(dataString);

            let ipOrSocketId = (this.sendJson[sid]) ? sid : ipStr;
            
            let currentPacket = this.getCurrentPacket(this.sendJson, ipOrSocketId);

            if (!currentPacket) return;

            currentPacket.miniPacketCheckList[mid] = "1";

            // console.log("收到反馈码 3， 小包序号 mid: ", mid, "来自 ", ipStr, ":", ipPort);

        })

    }

    // 4 收到小包齐全反馈 headerId ，删除旧包头
    fullPacketRecieved(dataString, ipStr, ipPort, sid) {

        process.nextTick(() => {

            let headerId = parseInt(dataString);

            let ipOrSocketId = (this.sendJson[sid]) ? sid : ipStr;

            let existPacket = this.getCurrentPacket(this.sendJson, ipOrSocketId);

            if (existPacket !== null && existPacket !== undefined) {

                existPacket.miniPacketListRecvState = true;

                // console.log(" 收到反馈码 4 ，" + headerId + "的所有小包被收到, 耗时", this.nowTimeSample - existPacket.sendTimeSample);

                // 已接收完毕，删除该包
                delete this.sendJson[ipOrSocketId][headerId];

                if (Object.keys(this.sendJson[ipOrSocketId]).length == 0) {

                    delete this.sendJson[ipOrSocketId];

                }
            }
        })
    }

    // 5 收到错误小包重发请求 mid
    requestWrongMini(dataString, ipStr, ipPort, sid) {

        process.nextTick(() => {

            let mid = parseInt(dataString);

            // console.log("SendJson", Object.keys(this.sendJson));

            // console.log("sid", sid);

            let currentPacket = (this.sendJson[sid] && Object.keys(this.sendJson[sid]).length > 0) ? this.getCurrentPacket(this.sendJson, sid) : this.getCurrentPacket(this.sendJson, ipStr);

            if (currentPacket) {

                if (currentPacket.miniPacketList[mid]) {

                    this.socket.send(currentPacket.miniPacketList[mid], currentPacket.toPort, currentPacket.toIp, (err, byte) => {

                        console.log("事件", currentPacket.packetHeader.n, "因错误补发送小包id", mid);
    
                    });
                }
            }
        })
    }

    // socket 监听
    onListening() {

        let address = this.address();

        console.log(address);

        CuteUDP.instance.emitServer("connectOnce", "Hello World!");

    }

    // socket 错误
    onError(err) {

        console.log("Socket 发生错误", err);

    }

    // socket 接收消息
    onMessage(msg, rinfo) {

        process.nextTick(() => {

            let bf = Buffer.from(msg);

            let recvString = bf.toString();

            let ipStr = rinfo.address;

            let ipPort = rinfo.port;

            let stateCode = recvString[0].toString();

            let sid = recvString.substr(1, 24);

            let dataString = recvString.substring(25);

            // console.log("Socket Message", recvString);

            switch (stateCode) {

                case "0": CuteUDP.instance.emit("addHeader", dataString, ipStr, ipPort, sid);
                    break;

                case "1": CuteUDP.instance.emit("headerRecieved", dataString, ipStr, ipPort, sid);
                    break;

                case "2": CuteUDP.instance.emit("jointPacket", dataString, ipStr, ipPort, sid);
                    break;

                case "3": CuteUDP.instance.emit("miniPacketRecieved", dataString, ipStr, ipPort, sid);
                    break;

                case "4": CuteUDP.instance.emit("fullPacketRecieved", dataString, ipStr, ipPort, sid);
                    break;

                case "5": CuteUDP.instance.emit("requestWrongMini", dataString, ipStr, ipPort, sid);
                    break;

                default:

                    break;
            }

        })

        // console.log(dataString);
    }

    // socket 关闭
    onClose() {

        console.log("UDP服务关了");

    }

    // 异步，发包头的回调
    sendHeaderCallBack(err, byte) {

        if (err) console.log("发送包头时出错", err);

    }

    // 异步，发小包的回调
    sendMiniCallBack(err, byte) {

        if (arguments.length == 1) byte = err;

        // console.log("back CPM Leng :", byte);

        if (err) console.log("发送小包出错", byte);

    }

    // 异步，发状态的回调
    sendResponseStateBack(err, byte) {

        if (err) console.log("发送反馈时出错", err);
        
    }

    // 获取当前收发的 Packet / BasePacket
    getCurrentPacket(_json, ipOrSocketId) {

        let v = _json[ipOrSocketId];

        if (!v) return null;

        let k = Object.keys(v)[0];

        if (!k) return null;

        let j = _json[ipOrSocketId][k];

        if (j.packetHeaderRecvState !== undefined) {

            if (j.packetHeaderRecvState === true) {

                if (j.miniPacketListRecvState === true) {

                    delete _json[ipOrSocketId][k];

                    return this.getCurrentPacket(_json, ipOrSocketId);

                }
            }
        }

        if (j.fullStr !== undefined) {

            if (j.fullStr.length === PacketHeader.getArraySize(j.packetHeader.a)) {

                delete _json[ipOrSocketId][k];

                return this.getCurrentPacket(_json, ipOrSocketId);
                
            }
        }

        return _json[ipOrSocketId][k];

    }

}

CuteUDP.socketId = "";

module.exports = CuteUDP;