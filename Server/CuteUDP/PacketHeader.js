class PacketHeader {

    constructor(eventName, packetCount, packetStringSize) {

        this.i = PacketHeader.prototype.count; // headerId
        
        this.n = eventName; // 事件名

        this.c = packetCount; // 小包总数量

        this.s = packetStringSize; // 小包总字节

        PacketHeader.prototype.count += 1;

    }
}

PacketHeader.prototype.count = 0;

module.exports = PacketHeader;