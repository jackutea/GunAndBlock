var CuteUDP = require("./CuteUDP");

class PacketHeader {

    constructor(eventName, packetCount, packetStringSize) {

        this.i = PacketHeader.count; // headerId
        
        this.n = eventName; // 事件名

        this.c = packetCount; // 小包总数量

        this.s = packetStringSize; // 小包总字节

        PacketHeader.count += 1;

    }
}

PacketHeader.count = 0;

module.exports = PacketHeader;