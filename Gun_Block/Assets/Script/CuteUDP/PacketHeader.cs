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
        ///小包对应mid的字节组长度
        ///</summary>
        public int[] a; // index = mid , value = (byte[] minipacket).length

        public PacketHeader(string eventName) {

            this.i = CuteUDP.count;

            this.n = eventName; 

            this.a = new int[0];

        }

        public int getArraySize() {

            int _count = 0;

            foreach (int i in a) {

                _count += i;

            }

            return _count;
        }
    }
}