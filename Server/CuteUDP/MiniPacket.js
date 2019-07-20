class MiniPacket {

    constructor(mid, content) {

        this.i = mid;

        this.n = content.toString();
    }
}

module.exports = MiniPacket;