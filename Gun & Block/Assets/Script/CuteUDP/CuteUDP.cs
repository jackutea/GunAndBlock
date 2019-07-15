using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuteUDPApp {

    // 逻辑：
    // 1.提交要发送的 "包"
    // 2.把 "包内容" 拆成 "小包组"
    // 3.通过 "小包组" 生成 "包头"，发送包头
    // 4.对方未接收到 "包头"，继续发送 "包头"
    // 6.对方接收到 "包头"，则发送 "小包组"
    // 5.对方收到所有 "小包组"，允许发送下一个包
    public class CuteUDP : EventEmitter {

        // UDP 配置
        Socket socket;
        public Thread sendThread;
        public Thread recvThread;
        public bool appRuning;

        // 服务端 配置
        string remoteIp;
        int localPort;
        int remotePort;
        IPEndPoint recvIpEndPoint;
        EndPoint recvEndPoint;
        IPEndPoint sendIpEndPoint;
        EndPoint sendEndPoint;

        // CuteUDP 配置
        public static int perLength;
        double abortTimeMs; // 总超时弃发(默认3秒)
        public double nowTimeSample; // 当前时间
        int repeatHeaderTimeMs; // 包头 重发延时
        int repeatMiniTimeMs; // 每个小包 重发延时 (默认为 0)
        public static int count = -1; // 包头自增计数

        // 发送
        Dictionary<string, Dictionary<int, Packet>> sendDic; // 待发数据列表

        // 接收
        Dictionary<string, Dictionary<int, BasePacket>> recvDic; // 待收数据列表

        // IP / Port 键值对
        Dictionary<string, int> ipToPortDic;

        // CuteUDP 构造方法
        public CuteUDP(string remoteIp, int _remotePort, int _localPort) {

            initConfig();

            initSocket(remoteIp, _remotePort, _localPort);

            eventListening();

            emitServer("connectOnce", "Hello World!");

            if (Thread.CurrentThread.Name == null) {

                Thread.CurrentThread.Name = "CuteMainThread";

            }

            // Debug.Log("CuteUDP 构造时线程 ：" + Thread.CurrentThread.Name);

        }

        // 初始化 CuteUDP 配置
        void initConfig() {

            abortTimeMs = 3000;

            perLength = 128;

            repeatHeaderTimeMs = 10; // 包头 重发延时

            repeatMiniTimeMs = 5; // 每个小包 重发延时 (默认为 0)

            appRuning = true;

            sendDic = new Dictionary<string, Dictionary<int, Packet>>();

            recvDic = new Dictionary<string, Dictionary<int, BasePacket>>();

            ipToPortDic = new Dictionary<string, int>();

        }

        // 初始化 Scoket
        void initSocket(string ip, int _remotePort, int _localPort) {

            remoteIp = ip;

            remoteIp = remoteIp.Trim();

            localPort = _localPort; // 本地端口

            remotePort = _remotePort; // 远程端口

            recvEndPoint = (EndPoint)new IPEndPoint(IPAddress.Any, 0);

            bool socketBindState = false;

            // 创建 Socket
            while (!socketBindState) {

                try {

                    recvIpEndPoint = new IPEndPoint(IPAddress.Any, localPort);

                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    localPort += 1;

                    // Socket 绑定本地端口
                    socket.Bind(recvIpEndPoint);

                    socketBindState = (socket.IsBound) ? true : false;

                    Debug.Log(socketBindState);

                } catch (Exception ex) {

                    string msg = ex.Message;

                    Debug.Log(socketBindState);

                }
            }
            
            // 创建发送线程
            sendThread = new Thread(new ParameterizedThreadStart(recvUpdating));

            sendThread.Name = "recvThread";
            
            sendThread.Start(false); // false 为启动阻塞接收线程

            // sendThread.Start(true); // true 为启动非阻塞接收线程， 这里有BUG

            // 创建接收线程
            recvThread = new Thread(new ThreadStart(sendUpdating));

            recvThread.Name = "sendThread";

            recvThread.Start();

        }

        // 发送消息(发包头)
        public void emitTo(string eventName, string obj, string ip, int port = 0) {

            // 如果不填 port 则发送系统默认对方的 port
            if (port == 0) {

                port = (ipToPortDic.ContainsKey(ip)) ? ipToPortDic[ip] : remotePort;

            }

            // 如果待发列表不存在该 IP ，添加
            if (!sendDic.ContainsKey(ip))

                sendDic.Add(ip, new Dictionary<int, Packet>());
            
            Packet packet = new Packet(eventName, obj, ip, port);

            if (sendDic[ip] != null) {

                if (!sendDic[ip].ContainsKey(packet.packetHeader.i)) {

                    // 添加包到队末
                    sendDic[ip].Add(packet.packetHeader.i, packet);

                    socket.SendTo(packet.headerBytes, 0, packet.headerBytes.Length, 0, packet.toIpEndPoint);

                    Debug.Log("正在发送 :" + packet.packetHeader.i + "  至" + ip + "的" + eventName + " 至IP" + ip + ":" + port);

                } else {

                    Debug.Log("该包头" + packet.packetHeader.i + "已在IP:" + ip + "的待发列表");

                }

            } else {

                // Debug.Log("该IP ：" + ip + "不存在待发内容");

                sendDic[ip] = new Dictionary<int, Packet>();

                // 添加包到队末
                sendDic[ip].Add(packet.packetHeader.i, packet);

                socket.SendTo(packet.headerBytes, 0, packet.headerBytes.Length, 0, packet.toIpEndPoint);

                Debug.Log("正在发送 :" + packet.packetHeader.i + "  至" + ip + "的" + eventName + " 至IP" + ip + ":" + port);
            }
        }

        // 广播消息(发包头)
        public void emitBrocast(string eventName, string obj, string[] ipGroup) {

            foreach (string ip in ipGroup) {

                emitTo(eventName, obj, ip);

            }
        }

        // 向服务端发消息
        public void emitServer(string eventName, string obj) {

            emitTo(eventName, obj, remoteIp, remotePort);

        }

        // 向发送者反馈状态
        void responseState(string stateCode, int obj, string ip, int port) {

            EndPoint ep = (EndPoint)new IPEndPoint(IPAddress.Parse(ip), port);

            string sendStr = stateCode + obj.ToString();

            byte[] sendBytes = Encoding.UTF8.GetBytes(sendStr);

            socket.SendTo(sendBytes, 0, sendBytes.Length, 0, ep);

            string stateString = "";

            switch (stateCode) {
                case "0" : stateString = "新增包头"; break;
                case "1" : stateString = "包头确认"; break;
                case "2" : stateString = "收到新小包"; break;
                case "3" : stateString = "小包序号确认"; break;
                case "4" : stateString = "小包齐全确认"; break;
                case "5" : stateString = "断连申请"; break;
                default : Debug.Log(stateString); break;
            }

            // Debug.Log("发送反馈码：" + stateCode + "状态：" + stateString + "内容：" + obj.ToString());
        }

        // 发送 独立循环线程
        void sendUpdating() {

            Debug.Log("线程启动成功");

            while(appRuning == true) {

                // 实时时间
                nowTimeSample = new TimeSpan(DateTime.Now.Ticks).TotalMilliseconds;
                
                // 接收列表里 检测超时弃收包
                if (recvDic.Count > 0) {

                    List<string> ipstrlist = new List<string>(recvDic.Keys);
                    
                    for (int i = 0; i < recvDic.Count; i += 1) {
                    
                        string ip = ipstrlist[i];
                    
                        BasePacket currentBasePacket = getCurrentBasePacket(ip);
                    
                        if (currentBasePacket == null) continue;

                        if (currentBasePacket.recvMiniCount == currentBasePacket.packetHeader.c && currentBasePacket.recvMiniSize == currentBasePacket.packetHeader.s) {

                            continue;
                        }

                        int timeGap = (int)(nowTimeSample - currentBasePacket.recvTimeSample);
                    
                        if (timeGap > abortTimeMs) {

                            recvDic[ip][currentBasePacket.packetHeader.i] = null;

                            Debug.Log("超时" + abortTimeMs + "ms，清除" + currentBasePacket.packetHeader.i + "包");

                        }
                    }
                }

                // 如果待发列表无数据，跳过
                if (sendDic.Count <= 0) return;

                // 待发列表里有多个待发包
                List<string> iplist = new List<string>(sendDic.Keys);

                // 多用户循环
                for (int i = 0; i < iplist.Count; i += 1) {

                    // 键/值
                    string ip = iplist[i];

                    Packet currentPacket = getCurrentPacket(ip);

                    // 如果该 IP 对应的包为空，跳过
                    if (currentPacket == null) continue;

                    int timeGap = (int)(nowTimeSample - currentPacket.sendTimeSample);
                    
                    // 如果该 IP 对应的包头已被收到，判断小包收发情况
                    if (currentPacket.packetHeaderRecvState == true) {

                        if (currentPacket.miniPacketListRecvState == true) {

                            //删除字典内的数据
                            sendDic[ip][currentPacket.packetHeader.i] = null;

                            continue;
                        }

                        // 重复发未被收到的小包
                        if (timeGap % repeatMiniTimeMs == 0) {

                            for (int o = 0; o < currentPacket.miniPacketCheckList.Length; o += 1) {

                                string hasStr = currentPacket.miniPacketCheckList[o];

                                if (hasStr == null) {

                                    // 发送未被收到的小包
                                    socket.SendTo(currentPacket.miniPacketList[i], 0, currentPacket.miniPacketList[i].Length, 0, currentPacket.toIpEndPoint);

                                }
                            }
                        }

                    } else {

                        // 包头未被对方收到，隔一段时间重发包头
                        if (timeGap > abortTimeMs) {

                            sendDic[ip] = null;

                            Debug.Log("超时" + abortTimeMs + "ms，清空" + ip + "所有待发包");

                            continue;
                        }

                        if (timeGap % repeatHeaderTimeMs == 0) {

                            socket.SendTo(currentPacket.headerBytes, 0, currentPacket.headerBytes.Length, 0, currentPacket.toIpEndPoint);

                            // Debug.Log("正在发送包头" + currentPacket.packetHeader.i);

                        }
                    }
                }
                
                Thread.Sleep(1);
            }
        }

        // 接收线程（阻塞版）
        void recvUpdating(object asyncBoolState) {

            bool isAsync = (bool)asyncBoolState;

            while(appRuning == true && !isAsync) {

                if (socket == null || socket.Available == 0) {

                    continue;
                    
                }

                byte[] recvBytes = new byte[300];

                try {

                    int recvBytesLength = socket.ReceiveFrom(recvBytes, ref recvEndPoint);

                    if (recvBytesLength <= 0) continue;

                } catch (Exception ex) {

                    string msg = ex.Message;

                    continue;

                }

                // 接收消息的字节数组 转换为字符串
                string recvString = Encoding.UTF8.GetString(recvBytes, 0, recvBytes.Length);

                string stateCode = recvString[0].ToString();

                string dataString = recvString.Substring(1);

                string[] ipSplit = recvEndPoint.ToString().Split(char.Parse(":"));

                string ipstr = ipSplit[0];

                int ipport = int.Parse(ipSplit[1]);

                // 判断首位字符串
                // "0" 收到新包头 转 PacketHeader 类型
                // "1" 收到包头确认声明 转 int 类型（对应的是 headerId）
                // "2" 收到小包 转 MiniPacket 类型
                // "3" 收到小包确认 转 int 类型（对应的是 小包的 mid）
                // "4" 收到小包齐全声明 转 int 类型（对应的是 headerId）
                // "5" 收到弃包声明 转 int 类型（对应的是 headerId）
                // 非以上的数，则是恶意修改源码，认定为非法攻击，将 ip 加入待审查名单
                switch(stateCode) {

                    case "0": invokeEvent<string, string, int>("addHeader", dataString, ipstr, ipport); break;

                    case "1": invokeEvent<string, string, int>("headerRecieved", dataString, ipstr, ipport); break;
                    
                    case "2": invokeEvent<string, string, int>("jointPacket", dataString, ipstr, ipport); break;
                    
                    case "3": invokeEvent<string, string, int>("miniPacketRecieved", dataString, ipstr, ipport); break;
                    
                    case "4": invokeEvent<string, string, int>("fullPacketRecieved", dataString, ipstr, ipport); break;
                    
                    case "5": invokeEvent<string, string, int>("abortPacket", dataString, ipstr, ipport); break;

                    default: break;
                }

                Thread.Sleep(1);

            }

            while(appRuning == true && isAsync) {

                if (socket == null || socket.Available == 0) {

                    continue;
                    
                }

                byte[] recvBytes = new byte[300];

                try {

                    socket.BeginReceiveFrom(recvBytes, 0, recvBytes.Length, 0, ref recvEndPoint, recvCallBack, recvBytes);

                } catch (Exception ex) {

                    string msg = ex.Message;

                    continue;

                }

                Thread.Sleep(1);

            }
        }

        // 异步接收时的回调线程
        void recvCallBack(IAsyncResult iar) {

            byte[] recvBytes = iar.AsyncState as byte[];

            // 接收消息的字节数组 转换为字符串
            string recvString = Encoding.UTF8.GetString(recvBytes, 0, recvBytes.Length);

            string stateCode = recvString[0].ToString();

            string dataString = recvString.Substring(1);

            string[] ipSplit = recvEndPoint.ToString().Split(char.Parse(":"));

            string ipstr = ipSplit[0];

            int ipport = int.Parse(ipSplit[1]);

            // 判断首位字符串
            // "0" 收到新包头 转 PacketHeader 类型
            // "1" 收到包头确认声明 转 int 类型（对应的是 headerId）
            // "2" 收到小包 转 MiniPacket 类型
            // "3" 收到小包确认 转 int 类型（对应的是 小包的 mid）
            // "4" 收到小包齐全声明 转 int 类型（对应的是 headerId）
            // "5" 收到弃包声明 转 int 类型（对应的是 headerId）
            // 非以上的数，则是恶意修改源码，认定为非法攻击，将 ip 加入待审查名单
            switch(stateCode) {

                case "0": invokeEvent<string, string, int>("addHeader", dataString, ipstr, ipport); break;

                case "1": invokeEvent<string, string, int>("headerRecieved", dataString, ipstr, ipport); break;
                
                case "2": invokeEvent<string, string, int>("jointPacket", dataString, ipstr, ipport); break;
                
                case "3": invokeEvent<string, string, int>("miniPacketRecieved", dataString, ipstr, ipport); break;
                
                case "4": invokeEvent<string, string, int>("fullPacketRecieved", dataString, ipstr, ipport); break;
                
                case "5": invokeEvent<string, string, int>("abortPacket", dataString, ipstr, ipport); break;

                default: break;
            }

            socket.EndReceiveFrom(iar, ref recvEndPoint);
        }

        // 监听事件
        void eventListening() {

            // Debug.Log("监听事件的线程是" + Thread.CurrentThread.Name);

            on<string, string, int>("connectOnce", onConnenctOnce);

            on<string, string, int>("addHeader", addHeader);

            on<string, string, int>("headerRecieved", headerRecieved);
            
            on<string, string, int>("jointPacket", jointPacket);
            
            on<string, string, int>("miniPacketRecieved", miniPacketRecieved);
            
            on<string, string, int>("fullPacketRecieved", fullPacketRecieved);
            
            on<string, string, int>("abortPacket", abortPacket);
        }

        // 初次连接事件
        void onConnenctOnce(string dataString, string remoteIp, int remotePort) {

            // 加入 IP / Port 键值对
            ipToPortDic[remoteIp] = remotePort;

            Debug.Log("ConnectOnce From" + remoteIp + ":" + remotePort + "; Msg :" + dataString);

        }

        // 0 收到新包头 转 PacketHeader 类型
        void addHeader(string dataString, string ip, int port) {

            double t1 = nowTimeSample;

            PacketHeader packetHeader = JsonUtility.FromJson<PacketHeader>(dataString);

            if (!recvDic.ContainsKey(ip)) {

                recvDic.Add(ip, new Dictionary<int, BasePacket>());

            }

            ipToPortDic[ip] = port;

            BasePacket bp = new BasePacket(packetHeader, ip, port);

            int headerId = packetHeader.i;

            // 反馈收到新包头
            responseState("1", headerId, ip, port);

            if (recvDic[ip] == null || !recvDic[ip].ContainsKey(headerId)) {

                recvDic[ip] = new Dictionary<int, BasePacket>();

                recvDic[ip].Add(headerId, bp);

                // Debug.Log("正在处理包头");
                
            }

            Debug.Log("收到反馈码 0 新包头 : " + packetHeader.n + packetHeader.i + "，转码耗时" + (nowTimeSample - t1));

            // 删除旧包头
            Packet existPacket = getCurrentPacket(ip);

            if (existPacket == null) return;

            int existHeaderId = existPacket.packetHeader.i;

            // 如果包头和小包都确认收完，则从发送字典里删除
            if (existPacket.packetHeaderRecvState == true && existPacket.miniPacketListRecvState == true) {

                Debug.Log("删除旧包头" + existHeaderId);

                sendDic[ip][existHeaderId] = null;

            }

        }

        // 1 收到包头确认 转 int 类型（对应的是 headerId）
        void headerRecieved(string dataString, string ip, int port) {
            
            int headerId = int.Parse(dataString);

            if (sendDic.ContainsKey(ip)) {

                if (sendDic[ip].ContainsKey(headerId)) {

                    Debug.Log("收到反馈码 1 包头 id " + headerId + "已被对方接收");

                    Packet currentPacket = sendDic[ip][headerId];

                    currentPacket.packetHeaderRecvState = true;

                    EndPoint ep = (EndPoint)new IPEndPoint(IPAddress.Parse(ip), port);

                    // 开始全发小包
                    for (int i = 0; i < currentPacket.miniPacketList.Count; i += 1) {

                        socket.SendTo(currentPacket.miniPacketList[i], 0, currentPacket.miniPacketList[i].Length, 0, ep);

                        // Debug.Log("正在发小包" + i);
                    }

                } else {

                    // Debug.Log("收到反馈码 1 包头 id : " + headerId + "，但该包头已处理过");

                }

            } else {

                // Debug.LogAssertion("收到反馈码 1 包头 id : " + headerId + "，但这是一个奇怪的IP地址");

            }
        }

        // 2 收到小包 转 MiniPacket 类型 回复收到小包序号
        void jointPacket(string dataString, string ip, int port) {

            // 接收到的小包字符串转码成 MiniPacket
            MiniPacket minipacket =  JsonUtility.FromJson<MiniPacket>(dataString);

            // 发送确认收到小包序号
            responseState("3", minipacket.i, ip, port);

            BasePacket currentBasePacket = getCurrentBasePacket(ip);

            // 如果包存在，计算拼接
            if (currentBasePacket != null) {

                Debug.Log("收到反馈码 2 小包 id : " + minipacket.i);

                // 如果未计算过该 小包
                if (currentBasePacket.jointStr[minipacket.i] == null) {

                    currentBasePacket.recvMiniCount += 1;

                    currentBasePacket.recvMiniSize += minipacket.n.Length;

                    // 在字符组加入该小包
                    currentBasePacket.jointStr[minipacket.i] = minipacket.n;

                    // Debug.Log("将小包" + minipacket.i + "加入拼接组");

                } else {

                    // Debug.Log("小包" + minipacket.i + "已存在拼接组");

                }

                if (currentBasePacket.recvMiniCount == currentBasePacket.packetHeader.c) {

                    // Debug.LogAssertion("小包已完整");

                    if (currentBasePacket.fullStr.Length == currentBasePacket.recvMiniSize) {

                        // 不用拼接，直接发齐全声明
                        responseState("4", currentBasePacket.packetHeader.i, ip, port);

                        Debug.Log("补发" + currentBasePacket.packetHeader.i + "齐全声明");

                    } else {

                        // 收到所有小包，开始拼接，接完发齐全声明，并触发事件
                        foreach(string s in currentBasePacket.jointStr) {

                            currentBasePacket.fullStr += s;

                        }

                        // 触发自定义事件
                        invokeEvent<string, string>(currentBasePacket.packetHeader.n, currentBasePacket.fullStr, ip);

                        // Debug.LogAssertion("触发事件" + currentBasePacket.packetHeader.n + " : " + currentBasePacket.fullStr);

                        // 触发完删除旧包头
                        recvDic[ip].Remove(currentBasePacket.packetHeader.i);

                        // 发送小包齐全声明
                        responseState("4", currentBasePacket.packetHeader.i, ip, port);

                        // Debug.Log("收到" + currentBasePacket.packetHeader.i + "的所有小包");

                    }

                } else {

                    // Debug.LogAssertion(" recvCount " + currentBasePacket.recvMiniCount + " header.count " + currentBasePacket.packetHeader.c + " / recvSize " + currentBasePacket.recvMiniSize + " headerSize " + currentBasePacket.packetHeader.s);

                }

            } else {

                // Debug.Log("收到反馈码 2，但该包已不存在");

            }
        }

        // 3 收到小包确认 转 int 类型（对应的是 小包的 mid）
        void miniPacketRecieved(string dataString, string ip, int port) {

            int mid = int.Parse(dataString);

            Packet currentPacket = getCurrentPacket(ip);

            if (currentPacket != null) {

                // Debug.Log("收到反馈码 3 小包 id 确认 : " + mid);

                currentPacket.miniPacketCheckList[mid] = "1";

            } else {

                // Debug.Log("收到反馈码 3 小包 ：" + mid + "，但该包已不存在");

            }
        }

        // 4 收到小包齐全确认 转 int 类型（对应的是 headerId）
        void fullPacketRecieved(string dataString, string ip, int port) {

            int headerId = int.Parse(dataString);
            
            Packet existPacket = getCurrentPacket(ip);

            if (existPacket != null) {

                Debug.Log("收到反馈码 4 包" + headerId + "已被完全收到 : " + headerId + "，耗时" + (nowTimeSample - existPacket.sendTimeSample));

                existPacket.miniPacketListRecvState = true;

                sendDic[ip].Remove(headerId);

            } else {

                Debug.Log("收到反馈码 4 包" + headerId + "已被完全收到并处理完成");

            }
        }

        // 5 收到断连声明 转 int 类型（对应的是 headerId）
        void abortPacket(string dataString, string ip, int port) {

            Debug.Log("收到反馈码 5");

            Debug.Log("收到弃包声明，包头为" + dataString);

        }

        // 退出程序
        public void quitCuteUDP() {

            appRuning = false;

            try {

                Thread.CurrentThread.Abort();

            } catch (ThreadAbortException ex) {

                Debug.Log(ex.Message);

            }

            sendThread.Abort();

            recvThread.Abort();

            removeListener<string, string, int>("connectOnce", onConnenctOnce);

            removeListener<string, string, int>("addHeader", addHeader);

            removeListener<string, string, int>("headerRecieved", headerRecieved);
            
            removeListener<string, string, int>("jointPacket", jointPacket);
            
            removeListener<string, string, int>("miniPacketRecieved", miniPacketRecieved);
            
            removeListener<string, string, int>("fullPacketRecieved", fullPacketRecieved);
            
            removeListener<string, string, int>("abortPacket", abortPacket);
        }

        // 处理字典的私有方法（不重要）
        Packet getCurrentPacket(string _ip) {

            if (_ip == "" || _ip == null) return null;

            if (!sendDic.ContainsKey(_ip)) return null;

            Dictionary<int, Packet> packetValueDic = sendDic[_ip];

            if (packetValueDic == null) return null;

            // 如果该 IP 无待发数据，跳过
            if (packetValueDic.Count <= 0) return null;

            // 处理该 IP 对应的 包
            int[] ik = new int[packetValueDic.Keys.Count];

            packetValueDic.Keys.CopyTo(ik, 0);

            if (!packetValueDic.ContainsKey(ik[0])) return null;

            return packetValueDic[ik[0]];
        }

        BasePacket getCurrentBasePacket(string _ip) {

            if (_ip == "" || _ip == null) return null;

            if (!recvDic.ContainsKey(_ip)) return null;

            Dictionary<int, BasePacket> packetValueDic = recvDic[_ip];

            if (packetValueDic == null) return null;

            // 如果该 IP 无待发数据，跳过
            if (packetValueDic.Count <= 0) return null;

            // 处理该 IP 对应的 包
            int[] ik = new int[packetValueDic.Keys.Count];

            packetValueDic.Keys.CopyTo(ik, 0);

            if (!packetValueDic.ContainsKey(ik[0])) return null;

            return packetValueDic[ik[0]];
        }
    }
}