class PacketHeader {

    constructor(eventName) {

        this.i = PacketHeader.count; // headerId
        
        this.n = eventName; // 事件名

        this.a = []; // 小包数组

        PacketHeader.count += 1;

    }
}

PacketHeader.getArraySize = function(arr) {

    let i = 0;

    for (let index in arr) {

        i += arr[index];

    }

    return i;
}

PacketHeader.count = 0;

module.exports = PacketHeader;