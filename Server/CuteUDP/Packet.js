var PacketHeader = require("./PacketHeader");
var MiniPacket = require("./MiniPacket");

class Packet {

    constructor(eventName, orginStr, ipStr, port) {

        // 包参数
        this.perLength = 128;

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
        
        let n = (parseInt(nx) === nx) ? parseInt(nx) : parseInt(nx) + 1;

        let index = 0;

        this.packetCount = n;

        this.packetStringSize = this.orginStr.length;
        
        let i = 0;

        while (i < n) {

            let s = this.orginStr.substring(index, this.perLength);

            let codeByte = Buffer.from("2", "utf-8");

            let sendBytes = Buffer.from(JSON.stringify(new MiniPacket(i, s)), "utf-8");

            let totalLength = codeByte.length + sendBytes.length;

            this.miniPacketList.push(Buffer.concat([codeByte, sendBytes], totalLength));

            index += this.perLength;

            i += 1;
        }
    }

    getHeaderBytes() {

        this.packetHeader = new PacketHeader(this.eventName, this.packetCount, this.packetStringSize);
        
        let sendStr = JSON.stringify(this.packetHeader);
        
        let sendBytes = Buffer.from(sendStr, "utf-8");
        
        let packetCodeBytes = Buffer.from(this.packetCode, "utf-8");
        
        let totalLength = sendBytes.length + packetCodeBytes.length;
        
        return Buffer.concat([packetCodeBytes, sendBytes], totalLength);
    }
}

module.exports = Packet;