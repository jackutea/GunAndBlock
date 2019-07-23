using System;
using System.Text;
using System.Net;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CuteUDPApp {
    // CuteContent 就是一个包对象
    // 由1.包参数；2.源包内容；3.拆分后的小包组；4.生成的包头，四部分组成
    [Serializable]
    public class Packet {

        // 包参数
        public int perLength; // 字符串切割长度
        public int sendTimeSample; // 发送的时间
        public bool packetHeaderRecvState; // 包头 回馈状态
        public bool miniPacketListRecvState; // 小包列表 回馈状态
        public string toIp; // 发往的IP
        public IPEndPoint toIpEndPoint; // 发往的IP方向
        // string packetCode; // 0 代表新发的包头

        // 源内容
        public string orginStr; // 要发送的源数据
        public byte[] orginBytes; // 要发送的源数据
        public string eventName; // 事件名

        // 包头
        public PacketHeader packetHeader;
        public byte[] headerBytes;

        // 小包组
        public List<byte[]> miniPacketList; // 小包列表
        public string[] miniPacketCheckList;
        public int packetCount; // 小包组 总数量
        public int packetStringSize; // 小包组 总大小

        // 构造方法
        public Packet(string eventName, string orginStr, string ip, int toPort) {

            this.initConfig();

            this.orginStr = orginStr;

            this.toIp = ip;

            this.toIpEndPoint = new IPEndPoint(IPAddress.Parse(ip), toPort);

            this.orginBytes = Encoding.UTF8.GetBytes(orginStr);

            this.eventName = eventName;

            packetHeader = new PacketHeader(eventName);

            this.splitBytes();

            CuteUDP.count += 1;

            this.headerBytes = getHeaderBytes();

        }

        // 初始化参数
        void initConfig() {

            this.perLength = CuteUDP.perLength;

            this.sendTimeSample = DateTime.Now.Millisecond;

            this.packetHeaderRecvState = false;

            this.miniPacketListRecvState = false;

        }

        byte[] getHeaderBytes() {

            string sendStr = JsonConvert.SerializeObject(this.packetHeader);

            byte[] packetCodeBytes = Encoding.UTF8.GetBytes("0");

            byte[] sidBytes = Encoding.UTF8.GetBytes(CuteUDP.socketId);

            byte[] sendBytes = Encoding.UTF8.GetBytes(sendStr);

            byte[] concated = concatBytes(concatBytes(packetCodeBytes, sidBytes), sendBytes);

            string constr = Encoding.UTF8.GetString(concated);

            return concated;
        }

        byte[] concatBytes(byte[] a, byte[] b) {

            byte[] c = new byte[a.Length + b.Length];

            Array.Copy(a, 0, c, 0, a.Length);

            Array.Copy(b, 0, c, a.Length, b.Length);

            return c;
        }


        // 大包拆小包，小包由：1.序号(长度不限，一般为3以下) 2.解码字符串（长度为secretReg的长度） 2.包内容(最长255) 组成
        void splitBytes() {

            miniPacketList = new List<byte[]>(); // 初始化小包组

            int len = orginStr.Length; // 大包长度

            float nx = (float)orginStr.Length / (float)perLength; // 计算大包需切成多少小包

            int n = ((int)nx == nx) ? (int)nx : (int)nx + 1; // 分包总数

            int index = 0;

            this.packetCount= n;

            this.miniPacketCheckList = new string[n];

            this.packetStringSize = orginStr.Length;

            int i = 0;

            packetHeader.a = new int[n];

            // 若包大于0，重复，直至分包结束
            while (i < n) {

                int p = (perLength > len) ? len : perLength;

                string s = this.orginStr.Substring(index, p);

                string sendStr = JsonConvert.SerializeObject(new MiniPacket(i, s));

                byte[] packetCodeBytes = Encoding.UTF8.GetBytes("2");

                byte[] sidBytes = Encoding.UTF8.GetBytes(CuteUDP.socketId);
                
                byte[] sendBytes = Encoding.UTF8.GetBytes(sendStr);

                byte[] concatedBytes = concatBytes(concatBytes(packetCodeBytes, sidBytes), sendBytes);

                packetHeader.a[i] = s.Length;

                miniPacketList.Add(concatedBytes); // 从最后一个开始打包

                index += perLength;

                len -= perLength;

                i += 1;
            }
        }
    }
}