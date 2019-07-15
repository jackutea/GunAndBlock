using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace CuteUDPApp {
    // CuteContent 就是一个包对象
    // 由1.包参数；2.源包内容；3.拆分后的小包组；4.生成的包头，四部分组成
    [Serializable]
    public class PacketHeader {

        ///<summary>
        ///包头 id
        ///</summary>
        public int i; // 包头 id

        ///<summary>
        ///包 emit 事件
        ///</summary>
        public string n; // 事件名

        ///<summary>
        ///小包组总数量
        ///</summary>
        public int c; // 小包组 总数量

        ///<summary>
        ///小包总大小
        ///</summary>
        public int s; // 小包组 总大小声明

        public PacketHeader(string eventName, int packetCount, int packetStringSize) {

            this.i = CuteUDP.count;

            this.n = eventName;

            this.c = packetCount;

            this.s = packetStringSize;
        }
    }
}