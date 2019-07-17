var PacketHeader = require("./PacketHeader");
var MiniPacket = require("./MiniPacket");
var crypto = require("crypto");

class Packet {

    constructor(eventName, orginStr, ipStr, port) {

        // 包参数
        this.perLength = Packet.perLength;

        this.sendTimeSample = (new Date()).valueOf();
        
        this.packetHeaderRecvState = false;
        
        this.miniPacketListRecvState = false;
        
        this.toIp = ipStr;

        this.toPort = port;

        this.packetCode = "0";

        // 源内容
        this.orginStr = orginStr;
        
        this.orginBytes = Buffer.from(this.orginStr, "utf-8");
        
        this.eventName = eventName;

        // 包头
        this.packetHeader = null;
        
        this.headerBytes = null;

        // 小包组 
        this.miniPacketList = new Array();

        this.miniPacketCheckList = new Array();
        
        this.packetCount = 0;
        
        this.packetStringSize = 0;

        // 方法
        this.splitBytes();
        
        this.headerBytes = this.getHeaderBytes();
    }

    splitBytes() {

        let len = this.orginStr.length;

        let nx = len / this.perLength;
        
        let n = (parseInt(nx) === nx && nx !== 0) ? parseInt(nx) : parseInt(nx) + 1;

        let index = 0;

        this.packetCount = n;

        this.packetStringSize = this.orginStr.length;
        
        let i = 0;

        while (i < n) {

            let s = this.orginStr.substring(index, this.perLength * (i + 1));

            let codeByte = Buffer.from("2", "utf-8");

            let sidBytes = Buffer.from(Packet.socketId, "utf-8");

            let sendBytes = Buffer.from(JSON.stringify(new MiniPacket(i, s)), "utf-8");

            let totalLength = codeByte.length + sidBytes.length + sendBytes.length;

            let concated = Buffer.concat([Buffer.concat([codeByte, sidBytes], codeByte.length + sidBytes.length), sendBytes], totalLength);

            this.miniPacketList.push(concated);

            index += this.perLength;

            i += 1;
        }
    }

    getHeaderBytes() {

        this.packetHeader = new PacketHeader(this.eventName, this.packetCount, this.packetStringSize);
        
        let sendStr = JSON.stringify(this.packetHeader);
        
        let sendBytes = Buffer.from(sendStr, "utf-8");

        let sidBytes = Buffer.from(Packet.socketId, "utf-8");

        let packetCodeBytes = Buffer.from(this.packetCode, "utf-8");
        
        let totalLength = packetCodeBytes.length + sidBytes.length + sendBytes.length;

        return Buffer.concat([Buffer.concat([packetCodeBytes, sidBytes], packetCodeBytes.length + sidBytes.length), sendBytes], totalLength);
    }
}

Packet.perLength = 0;

Packet.getSocketId = function() {

    let orginStr = "";

    for (let i = 1; i < 5; i += 1) {

        orginStr += (this.nowTimeSample + i).toString();

    }

    orginStr += (Math.random() * 1000000).toString();

    orginStr += (Math.random() * 1000000).toString();

    let md5 = new crypto.createHash("md5");

    let socketIdStr = md5.update(orginStr).digest("base64");

    return socketIdStr;

}

Packet.socketId = Packet.getSocketId();

module.exports = Packet;