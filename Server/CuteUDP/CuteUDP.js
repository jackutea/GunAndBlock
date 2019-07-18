var event = require("events");
var udp = require("dgram");

var Packet = require("./Packet");
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
        this.abortTimeMs = 3000; // 总超时弃发(默认3秒)

        this.perLength = 128;

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
        this.sendInterval = setInterval(() => {

            // 更新当前时间
            this.nowTimeSample = (new Date()).valueOf();
    
            // 待收列表超时检测
            let recvJson = this.recvJson;
    
            if (recvJson && Object.keys(recvJson).length > 0) {
    
                let ipList = Object.keys(recvJson);
    
                for (let i = 0; i < ipList.length; i += 1) {
    
                    let ip = ipList[i];
    
                    let currentBasePacket = this.getCurrentPacket(recvJson, ip);
    
                    if (!currentBasePacket) continue;

                    if (currentBasePacket.recvMiniCount == currentBasePacket.packetHeader.c && currentBasePacket.recvMiniSize == currentBasePacket.packetHeader.s) continue;
    
                    let timeGap = parseInt(this.nowTimeSample - currentBasePacket.recvTimeSample);
    
                    if (timeGap > this.abortTimeMs) {
    
                        delete recvJson[ip];
    
                        console.log("超时", this.abortTimeMs, "ms，清空", ip, "所有待收包");
    
                    }
                }
            }
    
            // 待发列表检测（重发包头和小包）
            let sendJson = this.sendJson;
    
            if (!sendJson) return;
    
            let ipList = Object.keys(sendJson);
    
            if (ipList.length <= 0) return;
    
            for (let i = 0; i < ipList.length; i += 1) {
    
                let ip = ipList[i];
    
                let currentPacket = this.getCurrentPacket(sendJson, ip);
    
                if (!currentPacket) continue;
    
                let timeGap = parseInt(this.nowTimeSample - currentPacket.sendTimeSample);
    
                if (currentPacket.packetHeaderRecvState == true) {
    
                    if (currentPacket.miniPacketListRecvState == true) {

                        //数据齐全，删掉上个包头
                        delete this.sendJson[ip][currentPacket.packetHeader.i];

                        continue;
                    }
    
                    // 在一定时间内，重发未被接收的小包
                    if (timeGap % this.repeatMiniTimeMs == 0) {
    
                        for (let o = 0; o < currentPacket.miniPacketList.length; o += 1) {
    
                            let hasStr = currentPacket.miniPacketCheckList[o];
    
                            if (hasStr === undefined) {
    
                                // 发未被接收的小包
                                this.socket.send(currentPacket.miniPacketList[o], currentPacket.toPort, currentPacket.toIp, this.sendMiniCallBack(currentPacket.miniPacketList[o]));
    
                            }
                        }
                    }

                } else {

                    if (timeGap % this.repeatHeaderTimeMs == 0) {
    
                        // 隔一段时间 发送包头
                        this.socket.send(currentPacket.headerBytes, 0, currentPacket.headerBytes.length, currentPacket.toPort, currentPacket.toIp, this.sendHeaderCallBack(currentPacket.packetHeader.i));
        
                    }
                }
    
                if (timeGap > this.abortTimeMs) {
    
                    delete sendJson[ip];
    
                    console.log("超时", this.abortTimeMs, "清空 ", ip, " 要发的包");
    
                    continue;
                }
            }
        }, 1);
    }

    initSocketFunc() {

        this.socket.on("listening", this.onListening);
        
        this.socket.on("error", this.onError);
        
        this.socket.on("message", this.onMessage);
        
        this.socket.on("close", this.onClose);

        this.socket.bind(this.localPort);

    }

    initPrivateFunc() {

        this.on("connectOnce", this.onConnectOnce); // 监听初次连接，记录 ip/port 键值对

        this.on("addHeader", this.addHeader); // 收到包头

        this.on("headerRecieved", this.headerRecieved); // 反馈包头确认
        
        this.on("jointPacket", this.jointPacket); // 收到小包，开始拼接，并反馈收到的小包序号
        
        this.on("miniPacketRecieved", this.miniPacketRecieved); // 收到已确认的小包序号
        
        this.on("fullPacketRecieved", this.fullPacketRecieved); // 收到小包齐全确认
        
        this.on("abortPacket", this.abortPacket); // 收到弃发请求

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

            this.socket.send(packet.headerBytes, 0, packet.headerBytes.length, packet.toPort, packet.toIp, this.sendHeaderCallBack(packet.packetHeader.i));

            console.log("正在发送：", packet.packetHeader.i, ":", objStr, " 至", ipStr, ":", port, "的", eventName, "字符串长度", packet.orginStr.length);

        }
    }

    // 回传消息
    emitBackTo(eventName, objStr, sid) {

        let ip = this.socketIdToIpInfo[sid].ip;

        let port = this.socketIdToIpInfo[sid].port;

        this.emitTo(eventName, objStr, ip, port, sid);

    }

    // 向服务端发消息
    emitServer(eventName, objStr) {

        this.emitTo(eventName, objStr, this.serverIp);

    }

    // 向发送者反馈状态
    responseState(stateCode, obj, ipStr, ipPort) {

        let sendStr = stateCode + CuteUDP.socketId + obj.toString();

        let sendBytes = Buffer.from(sendStr, "utf-8");

        this.socket.send(sendBytes, 0, sendBytes.length, ipPort, ipStr, this.sendResponseStateBack(sendStr));

        // console.log("发送反馈码：", stateCode, "内容：", obj.toString());

    }

    // 0 收到新包头
    addHeader(dataString, ipStr, ipPort, sid) {

        // console.log("收到反馈码 0 新包头 id: ", dataString, "来自", ipStr, ":", ipPort);

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

        // 反馈收到新包头
        this.responseState("1", headerId, ipStr, ipPort);

    }

    // 1 收到包头确认
    headerRecieved(dataString, ipStr, ipPort, sid) {

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
                for (let i = 0; i < currentPacket.packetCount; i += 1) {

                    this.socket.send(currentPacket.miniPacketList[i], ipPort, ipStr, this.sendMiniCallBack(currentPacket.miniPacketList[i]));

                    // console.log("正在发小包", i);

                }

            } else {

                // console.log("收到反馈码 1 包头 :", headerId, "，但该包头已处理过");

            }

        } else {

            // console.log("收到反馈码 1 包头 id : " + headerId + "，但这是一个奇怪的IP地址");

        }
    }

    // 2 收到新小包 MiniPacket 开始拼接
    jointPacket(dataString, ipStr, ipPort, sid) {

        let miniPacket = JSON.parse(dataString);

        // 发送已收到的小包序号
        this.responseState("3", miniPacket.i, ipStr, ipPort);

        let currentBasePacket = this.getCurrentPacket(this.recvJson, sid);

        if (currentBasePacket) {

            // console.log("收到反馈码 2 新小包", miniPacket.i, ":", miniPacket.n);

            // 如果未计算过该 小包
            if (currentBasePacket.jointStr[miniPacket.i] === undefined) {

                currentBasePacket.recvMiniCount += 1;

                currentBasePacket.recvMiniSize += miniPacket.n.length;

                // 在字符组加入该小包
                currentBasePacket.jointStr[miniPacket.i] = miniPacket.n;

            }

            // 如果长度和数量已收完，停止接收
            if (currentBasePacket.recvMiniCount == currentBasePacket.packetHeader.c && currentBasePacket.recvMiniSize == currentBasePacket.packetHeader.s) {

                // 如果是空包，直接触发事件
                if (currentBasePacket.packetHeader.c === 0) {

                    // console.log("触发事件", currentBasePacket.packetHeader.n);

                    // 触发自定义事件
                    this.emit(currentBasePacket.packetHeader.n, currentBasePacket.fullStr, sid);

                    // 触发完删除旧包头
                    delete this.recvJson[sid][currentBasePacket.packetHeader.i];

                    // 发送小包齐全声明
                    this.responseState("4", currentBasePacket.packetHeader.i, ipStr, ipPort);

                    return;
                }

                if (currentBasePacket.fullStr.length == currentBasePacket.recvMiniSize) {

                    // 不用拼接，直接补发齐全声明
                    this.responseState("4", currentBasePacket.packetHeader.i, ipStr, ipPort);

                    console.log("补发", currentBasePacket.packetHeader.i, "齐全声明");

                } else {

                    // 收到所有小包，开始拼接，接完发齐全声明，并触发事件
                    for (let s in currentBasePacket.jointStr) {

                        currentBasePacket.fullStr += currentBasePacket.jointStr[s];

                    }

                    // console.log("触发事件", currentBasePacket.packetHeader.n);

                    // 触发自定义事件
                    this.emit(currentBasePacket.packetHeader.n, currentBasePacket.fullStr, sid);

                    // 触发完删除旧包头
                    delete this.recvJson[sid][currentBasePacket.packetHeader.i];

                    // 发送小包齐全声明
                    this.responseState("4", currentBasePacket.packetHeader.i, ipStr, ipPort);

                    // console.log("收到", currentBasePacket.packetHeader.i, "的所有小包");
                }
            }

        } else {

            // console.log("收到反馈码 2 但该包已不存在");

        }
    }

    // 3 收到小包序号反馈 mid
    miniPacketRecieved(dataString, ipStr, ipPort, sid) {

        // console.log("收到反馈码 3 小包序号 mid: ", dataString);

        let mid = parseInt(dataString);

        let ipOrSocketId = (this.sendJson[sid]) ? sid : ipStr;
        
        let currentPacket = this.getCurrentPacket(this.sendJson, ipOrSocketId);

        if (!currentPacket) return;

        currentPacket.miniPacketCheckList[mid] = "1";

        // console.log("收到小包序号为" + mid + "状态(1为正常)" + currentPacket.miniPacketCheckList[mid]);

    }

    // 4 收到小包齐全反馈 headerId ，删除旧包头
    fullPacketRecieved(dataString, ipStr, ipPort, sid) {

        let headerId = parseInt(dataString);

        let ipOrSocketId = (this.sendJson[sid]) ? sid : ipStr;

        let existPacket = this.getCurrentPacket(this.sendJson, ipOrSocketId);

        if (existPacket !== null && existPacket !== undefined) {

            existPacket.miniPacketListRecvState = true;

            console.log(" 收到反馈码 4 所有小包被收到, 耗时", this.nowTimeSample - existPacket.sendTimeSample);

            // 已接收完毕，删除该包
            delete this.sendJson[ipOrSocketId][headerId];

        } else {

            // console.log(" 所有小包被收到，但该包已不存在");

        }
    }

    // 5 收到弃发小包请求 headerId
    abortPacket(dataString, ipStr, ipPort, sid) {

        console.log("收到弃发小包请求" + dataString);

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
            
        let bf = Buffer.from(msg);

        let recvString = bf.toString();

        let ipStr = rinfo.address;

        let ipPort = rinfo.port;

        let stateCode = recvString[0].toString();

        let sid = recvString.substr(1, 24);

        let dataString = recvString.substring(25);

        // console.log("recvString", recvString);
        // console.log("stateCode", stateCode);
        // console.log("sid", sid);
        // console.log("dataString", dataString);

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

            case "5": CuteUDP.instance.emit("abortPacket", dataString, ipStr, ipPort, sid);
                break;

            default:

                break;
        }

        // console.log(dataString);
    }

    // socket 关闭
    onClose() {

        console.log("UDP服务关了");

    }

    // 异步，发包头的回调
    sendHeaderCallBack(data) {

        // console.log("已发送包头：", data);

    }

    // 异步，发小包的回调
    sendMiniCallBack(data) {

        // console.log("已发小包：", data.toString());

    }

    // 异步，发状态的回调
    sendResponseStateBack(data) {

        // console.log("已回复状态", data);
        
    }

    // 获取当前收发的 Packet / BasePacket
    getCurrentPacket(_json, ipOrSocketId) {

        let v = _json[ipOrSocketId];

        if (!v) return null;

        let k = Object.keys(v)[0];

        if (!k) return null;

        return _json[ipOrSocketId][k];

    }

}

CuteUDP.socketId = "";

module.exports = CuteUDP;