class BasePacket {

    constructor(packetHeader, ip, port) {

        this.packetHeader = packetHeader;

        this.recvTimeSample = (new Date()).valueOf();

        this.jointStr = new Array();

        this.fullStr = "";

        this.recvMiniCount = 0;

        this.recvMiniSize = 0;
        
        this.fromIp = ip;

        this.fromPort = port;
    }
}

module.exports = BasePacket;