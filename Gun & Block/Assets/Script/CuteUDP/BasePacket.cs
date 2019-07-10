using System;

namespace CuteUDPApp {
    public class BasePacket {
        public PacketHeader packetHeader;
        public double recvTimeSample;
        public string[] jointStr;
        public string fullStr;
        public int recvMiniCount;
        public int recvMiniSize;
        public string fromIp;
        public int fromPort;

        public BasePacket(PacketHeader packetHeader, string ip, int port) {

            this.packetHeader = packetHeader;

            this.recvTimeSample = new TimeSpan(DateTime.Now.Ticks).TotalMilliseconds;

            this.jointStr = new string[packetHeader.c];

            this.fullStr = "";

            this.recvMiniCount = 0;
            
            this.recvMiniSize = 0;

            this.fromIp = ip;

            this.fromPort = port;
        }
    }
}