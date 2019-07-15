using System;
using System.Text;
using System.Collections.Generic;

namespace CuteUDPApp {
    [Serializable]
    public class MiniPacket {
        public int i; // mid
        public string n; // 内容

        public MiniPacket(int mid, string content) {

            this.i = mid;

            this.n = content;
            
        }
    }
}