using System;
using System.Security;
using System.Security.Cryptography;
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
        public static string socketId; // 24长度的md5
        public Thread sendThread;
        public Thread recvThread;
        public bool appRuning;

        // 服务端 配置
        string remoteIp;
        int localPort;
        public int remotePort;
        IPEndPoint recvIpEndPoint;
        EndPoint recvEndPoint;
        IPEndPoint sendIpEndPoint;
        EndPoint sendEndPoint;

        // CuteUDP 配置
        public static int perLength;
        double abortTimeMs; // 总超时弃发(默认5秒)
        public double nowTimeSample = new TimeSpan(DateTime.Now.Ticks).TotalMilliseconds; // 当前时间
        int repeatHeaderTimeMs; // 包头 重发延时
        int repeatMiniTimeMs; // 每个小包 重发延时 (默认为 0)
        public static int count = 0; // 包头自增计数

        // 发送
        Dictionary<string, Dictionary<int, Packet>> sendDic; // 待发数据列表 key = string ipOrSocketId

        // 接收
        Dictionary<string, Dictionary<int, BasePacket>> recvDic; // 待收数据列表

        // IP / Port 键值对
        Dictionary<string, IpInfo> soketIdToIpInfo;

        // CuteUDP 构造方法
        public CuteUDP(string remoteIp, int _remotePort, int _localPort) {

            initConfig();

            initSocket(remoteIp, _remotePort, _localPort);

            eventListening();

            // emitServer("connectOnce", "Hello World!");
            emitServer("connectOnce", "");

            if (Thread.CurrentThread.Name == null) {

                Thread.CurrentThread.Name = "CuteMainThread";

            }

            // Debug.Log("CuteUDP 构造时线程 ：" + Thread.CurrentThread.Name);

        }

        // 初始化 CuteUDP 配置
        void initConfig() {

            abortTimeMs = 5000;

            perLength = 128;

            repeatHeaderTimeMs = 20; // 包头 重发延时

            repeatMiniTimeMs = 5; // 每个小包 重发延时 (默认为 0)

            appRuning = true;

            sendDic = new Dictionary<string, Dictionary<int, Packet>>();

            recvDic = new Dictionary<string, Dictionary<int, BasePacket>>();

            soketIdToIpInfo = new Dictionary<string, IpInfo>();

        }

        // 初始化 Scoket
        void initSocket(string ip, int _remotePort, int _localPort) {

            remoteIp = ip;

            remoteIp = remoteIp.Trim();

            localPort = _localPort; // 本地端口

            remotePort = _remotePort; // 远程端口

            recvEndPoint = (EndPoint)new IPEndPoint(IPAddress.Any, 0);

            socketId = getSocketId();
            
            Debug.Log("当前 SocketId : " + socketId);
            
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

                    Debug.Log("端口" + (localPort - 1));

                } catch (Exception ex) {

                    string msg = ex.Message;

                    // Debug.Log(socketBindState);

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

        // 生成socketId
        string getSocketId() {

            string originStr = "";

            originStr += Dns.GetHostName();

            for (int i = 1; i < 5; i += 1) {

                originStr += (nowTimeSample + i).ToString();

            }

            originStr += new System.Random().Next(0, 1000000).ToString();

            originStr += new System.Random().Next(0, 1000000).ToString();

            MD5 md5 = new MD5CryptoServiceProvider();

            byte[] s = Encoding.UTF8.GetBytes(originStr);

            byte[] c = md5.ComputeHash(s);

            string socketIdStr = Convert.ToBase64String(c);

            return socketIdStr;

        }

        // 发送消息(发包头)
        public void emitTo(string eventName, string obj, string ip, int port = 0, string sid = "") {

            string ipOrSocketId;

            if (sid != "" && soketIdToIpInfo.ContainsKey(sid)) {

                ipOrSocketId = sid;

                ip = soketIdToIpInfo[sid].ip;

                port = soketIdToIpInfo[sid].port;

            } else {

                ipOrSocketId = ip;

                port = (port == 0) ? remotePort : port;

            }

            // 如果待发列表不存在该 IP ，添加
            if (!sendDic.ContainsKey(ipOrSocketId))

                sendDic.Add(ipOrSocketId, new Dictionary<int, Packet>());
            
            Packet packet = new Packet(eventName, obj, ip, port);

            if (sendDic[ipOrSocketId] != null) {

                if (!sendDic[ipOrSocketId].ContainsKey(packet.packetHeader.i)) {

                    // 添加包到队末
                    sendDic[ipOrSocketId].Add(packet.packetHeader.i, packet);

                    string str = Encoding.UTF8.GetString(packet.headerBytes);

                    socket.SendTo(packet.headerBytes, 0, packet.headerBytes.Length, 0, packet.toIpEndPoint);

                    Debug.Log("正在发送 :" + packet.packetHeader.i + "  至" + ip + "的" + eventName + " 至IP" + ip + ":" + port);

                    // Debug.Log("内容 :" + str);

                } else {

                    Debug.Log("该包头" + packet.packetHeader.i + "已在IP:" + ip + "的待发列表");

                }

            } else {

                // Debug.Log("该IP ：" + ip + "不存在待发内容");

                sendDic[ipOrSocketId] = new Dictionary<int, Packet>();

                // 添加包到队末
                sendDic[ipOrSocketId].Add(packet.packetHeader.i, packet);

                socket.SendTo(packet.headerBytes, 0, packet.headerBytes.Length, 0, packet.toIpEndPoint);

                Debug.Log("正在发送 :" + packet.packetHeader.i + "  至" + ip + "的" + eventName + " 至IP" + ip + ":" + port);
            }
        }

        // 回传消息
        public void emitBackTo(string eventName, string obj, string sid) {

            string ip = soketIdToIpInfo[sid].ip;

            int port = soketIdToIpInfo[sid].port;

            emitTo(eventName, obj, ip, port, sid);

        }

        // 向服务端发消息
        public void emitServer(string eventName, string obj) {

            emitTo(eventName, obj, remoteIp, remotePort);

        }

        // 向发送者反馈状态
        void responseState(string stateCode, int obj, string ip, int port) {

            EndPoint ep = (EndPoint)new IPEndPoint(IPAddress.Parse(ip), port);

            string sendStr = stateCode + socketId + obj.ToString();

            // Debug.Log(sendStr);

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
                    
                        string sid = ipstrlist[i];
                    
                        BasePacket currentBasePacket = getCurrentBasePacket(sid);
                    
                        if (currentBasePacket == null) continue;

                        if (currentBasePacket.recvMiniSize >= currentBasePacket.packetHeader.getArraySize()) {

                            continue;
                        }

                        int timeGap = (int)(nowTimeSample - currentBasePacket.recvTimeSample);
                    
                        if (timeGap > abortTimeMs) {

                            recvDic[sid].Remove(currentBasePacket.packetHeader.i);

                            if (recvDic[sid].Count == 0) {

                                recvDic.Remove(sid);

                            }

                            Debug.Log("超时" + abortTimeMs + "ms，清除" + currentBasePacket.packetHeader.i + "待收包");

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
                    string ipOrSocketId = iplist[i];

                    Packet currentPacket = getCurrentPacket(ipOrSocketId);

                    // 如果该 IP 对应的包为空，跳过
                    if (currentPacket == null) continue;

                    int timeGap = (int)(nowTimeSample - currentPacket.sendTimeSample);
                    
                    // 如果该 IP 对应的包头已被收到，判断小包收发情况
                    if (currentPacket.packetHeaderRecvState == true) {

                        if (currentPacket.miniPacketListRecvState == true) {

                            //删除字典内的数据
                            sendDic[ipOrSocketId].Remove(currentPacket.packetHeader.i);

                            if (sendDic[ipOrSocketId].Count == 0) {

                                sendDic.Remove(ipOrSocketId);

                            }

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

                            sendDic.Remove(ipOrSocketId);

                            Debug.Log("超时" + abortTimeMs + "ms，清空" + ipOrSocketId + "所有待发包");

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

                byte[] recvBytes = new byte[500];

                int l;

                try {

                    int recvBytesLength = socket.ReceiveFrom(recvBytes, ref recvEndPoint);

                    l = recvBytesLength;

                    if (recvBytesLength <= 0) continue;

                } catch (Exception ex) {

                    string msg = ex.Message;

                    continue;

                }

                // 接收消息的字节数组 转换为字符串
                string recvString = Encoding.UTF8.GetString(recvBytes, 0, l);

                string stateCode = recvString[0].ToString();

                string sid = recvString.Substring(1, 24);

                string dataString = recvString.Substring(25);

                string[] ipSplit = recvEndPoint.ToString().Split(char.Parse(":"));

                string ipstr = ipSplit[0];

                int ipport = int.Parse(ipSplit[1]);

                // Debug.Log("stateCode" + stateCode);
                // Debug.Log("sid" + sid);
                // Debug.Log("dataString" + dataString);

                // 判断首位字符串
                // "0" 收到新包头 转 PacketHeader 类型
                // "1" 收到包头确认声明 转 int 类型（对应的是 headerId）
                // "2" 收到小包 转 MiniPacket 类型
                // "3" 收到小包确认 转 int 类型（对应的是 小包的 mid）
                // "4" 收到小包齐全声明 转 int 类型（对应的是 headerId）
                // "5" 收到弃包声明 转 int 类型（对应的是 headerId）
                // 非以上的数，则是恶意修改源码，认定为非法攻击，将 ip 加入待审查名单
                switch(stateCode) {

                    case "0": invokeEvent<string, string, int, string>("addHeader", dataString, ipstr, ipport, sid); break;

                    case "1": invokeEvent<string, string, int, string>("headerRecieved", dataString, ipstr, ipport, sid); break;
                    
                    case "2": invokeEvent<string, string, int, string>("jointPacket", dataString, ipstr, ipport, sid); break;
                    
                    case "3": invokeEvent<string, string, int, string>("miniPacketRecieved", dataString, ipstr, ipport, sid); break;
                    
                    case "4": invokeEvent<string, string, int, string>("fullPacketRecieved", dataString, ipstr, ipport, sid); break;
                    
                    case "5": invokeEvent<string, string, int, string>("requestWrongMini", dataString, ipstr, ipport, sid); break;

                    default: break;
                }

                Thread.Sleep(1);

            }
        }

        // 监听事件
        void eventListening() {

            // Debug.Log("监听事件的线程是" + Thread.CurrentThread.Name);

            on<string, string>("connectOnce", onConnenctOnce);

            on<string, string, int, string>("addHeader", addHeader);

            on<string, string, int, string>("headerRecieved", headerRecieved);
            
            on<string, string, int, string>("jointPacket", jointPacket);
            
            on<string, string, int, string>("miniPacketRecieved", miniPacketRecieved);
            
            on<string, string, int, string>("fullPacketRecieved", fullPacketRecieved);
            
            on<string, string, int, string>("requestWrongMini", requestWrongMini);
        }

        // 初次测试连接事件
        void onConnenctOnce(string dataString, string sid) {

            string remoteIp = soketIdToIpInfo[sid].ip;

            int remotePort = soketIdToIpInfo[sid].port;

            Debug.Log("ConnectOnce From" + remoteIp + ":" + remotePort + "; Msg :" + dataString);

        }

        // 0 收到新包头 转 PacketHeader 类型
        void addHeader(string dataString, string ip, int port, string sid) {

            PacketHeader packetHeader = JsonUtility.FromJson<PacketHeader>(dataString);

            if (!recvDic.ContainsKey(sid)) {

                recvDic.Add(sid, new Dictionary<int, BasePacket>());

            }

            soketIdToIpInfo[sid] = new IpInfo(ip, port);

            BasePacket bp = new BasePacket(packetHeader, ip, port);

            int headerId = packetHeader.i;

            if (recvDic[sid] == null || !recvDic[sid].ContainsKey(headerId)) {

                recvDic[sid] = new Dictionary<int, BasePacket>();

                recvDic[sid].Add(headerId, bp);

                // Debug.Log("正在处理包头");
                
            }

            // 空包，直接触发
            if (packetHeader.a[0] == 0) {
                
                // 触发自定义事件
                invokeEvent<string, string>(packetHeader.n, "", sid);

                // 触发完删除旧包头
                recvDic[sid].Remove(packetHeader.i);

                if (recvDic[sid].Count == 0) {

                    recvDic.Remove(sid);

                }

                // 发送小包齐全声明
                responseState("4", packetHeader.i, ip, port);

                return;

            }

            // 反馈收到新包头
            responseState("1", headerId, ip, port);

        }

        // 1 收到包头确认 转 int 类型（对应的是 headerId）
        void headerRecieved(string dataString, string ip, int port, string sid) {
            
            int headerId = int.Parse(dataString);

            string ipOrSocketId = (sendDic.ContainsKey(sid)) ? sid : ip;

            if (sendDic.ContainsKey(ipOrSocketId)) {

                if (sendDic[ipOrSocketId].ContainsKey(headerId)) {

                    // Debug.Log("收到反馈码 1 包头 id " + headerId + "已被对方接收");

                    Packet currentPacket = sendDic[ipOrSocketId][headerId];

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
        void jointPacket(string dataString, string ip, int port, string sid) {

            // TODO 小包错乱问题
            .

            // 接收到的小包字符串转码成 MiniPacket
            MiniPacket minipacket =  JsonUtility.FromJson<MiniPacket>(dataString);

            BasePacket currentBasePacket = getCurrentBasePacket(sid);

            // 如果包存在，计算拼接
            if (currentBasePacket != null) {

                PacketHeader ph = currentBasePacket.packetHeader;

                int mid = minipacket.i;

                int declaredSize = ph.a[mid];

                Debug.LogWarning(mid);

                int recvSize = minipacket.n.Length;

                if (declaredSize != recvSize) {

                    Debug.Log("尺寸不匹配（收到/声明）：" + recvSize + "/" + declaredSize);

                    // TODO 请求补发小包序号
                    responseState("5", mid, ip, port);

                    return;

                }

                // 确认小包正确后，发送小包序号
                responseState("3", mid, ip, port);

                // 如果未计算过该 小包
                if (currentBasePacket.jointStr[mid] == null) {

                    currentBasePacket.recvMiniCount += 1;

                    currentBasePacket.recvMiniSize += minipacket.n.Length;

                    // 在字符组加入该小包
                    currentBasePacket.jointStr[mid] = minipacket.n;

                    // 确认小包正确后，发送小包序号
                    responseState("3", mid, ip, port);

                    Debug.Log("包头" + currentBasePacket.packetHeader.i +"小包 id ：" + mid + "小包内容" + minipacket.n);

                }

                // 小包已完整接收
                if (currentBasePacket.recvMiniSize >= currentBasePacket.packetHeader.getArraySize()) {

                    if (currentBasePacket.fullStr.Length >= currentBasePacket.recvMiniSize) {

                        // 不用拼接，直接发齐全声明
                        responseState("4", currentBasePacket.packetHeader.i, ip, port);

                        Debug.Log("补发" + currentBasePacket.packetHeader.i + "齐全声明");

                    } else {

                        // 收到所有小包，开始拼接，接完发齐全声明，并触发事件
                        foreach(string s in currentBasePacket.jointStr) {

                            currentBasePacket.fullStr += s;

                        }

                        // 触发自定义事件
                        invokeEvent<string, string>(currentBasePacket.packetHeader.n, currentBasePacket.fullStr, sid);

                        // 触发完删除旧包头
                        recvDic[sid].Remove(currentBasePacket.packetHeader.i);

                        if (recvDic[sid].Count == 0) {

                            recvDic.Remove(sid);

                        }

                        // 发送小包齐全声明
                        responseState("4", currentBasePacket.packetHeader.i, ip, port);

                    }
                }
            }
        }

        // 3 收到小包确认 转 int 类型（对应的是 小包的 mid）
        void miniPacketRecieved(string dataString, string ip, int port, string sid) {

            int mid = int.Parse(dataString);

            string ipOrSocketId = (sendDic.ContainsKey(sid)) ? sid : ip;

            Packet currentPacket = getCurrentPacket(ipOrSocketId);

            if (currentPacket != null) {

                // Debug.Log("收到反馈码 3 小包 id 确认 : " + mid);

                currentPacket.miniPacketCheckList[mid] = "1";

            } else {

                // Debug.Log("收到反馈码 3 小包 ：" + mid + "，但该包已不存在");

            }
        }

        // 4 收到小包齐全确认 转 int 类型（对应的是 headerId）
        void fullPacketRecieved(string dataString, string ip, int port, string sid) {

            int headerId = int.Parse(dataString);

            string ipOrSocketId = (sendDic.ContainsKey(sid)) ? sid : ip;
            
            Packet existPacket = getCurrentPacket(ipOrSocketId);

            if (existPacket != null) {

                Debug.Log("收到反馈码 4 包" + headerId + "已被完全收到 : " + headerId + "，耗时" + (nowTimeSample - existPacket.sendTimeSample));

                existPacket.miniPacketListRecvState = true;

                sendDic[ipOrSocketId].Remove(headerId);

                if (sendDic[ipOrSocketId].Count == 0) {

                    sendDic.Remove(ipOrSocketId);

                }

            } else {

                Debug.Log("收到反馈码 4 包" + headerId + "已被完全收到并处理完成");

            }
        }

        // 5 收到错误小包重发请求 mid
        void requestWrongMini(string dataString, string ip, int port, string sid) {

            int mid = int.Parse(dataString);

            Packet currentPacket = (sendDic.ContainsKey(ip)) ? getCurrentPacket(ip) : getCurrentPacket(sid);

            EndPoint ep = (EndPoint)new IPEndPoint(IPAddress.Parse(ip), port);

            socket.SendTo(currentPacket.miniPacketList[mid], 0, currentPacket.miniPacketList[mid].Length, 0, ep);

            Debug.Log("收到反馈码 5，正在补发 小包:" + mid);

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

            removeListener<string, string>("connectOnce", onConnenctOnce);

            removeListener<string, string, int, string>("addHeader", addHeader);

            removeListener<string, string, int, string>("headerRecieved", headerRecieved);
            
            removeListener<string, string, int, string>("jointPacket", jointPacket);
            
            removeListener<string, string, int, string>("miniPacketRecieved", miniPacketRecieved);
            
            removeListener<string, string, int, string>("fullPacketRecieved", fullPacketRecieved);
            
            removeListener<string, string, int, string>("requestWrongMini", requestWrongMini);
        }

        // 处理字典的私有方法（不重要）
        Packet getCurrentPacket(string ipOrSocketId) {

            if (ipOrSocketId == "" || ipOrSocketId == null) return null;

            if (!sendDic.ContainsKey(ipOrSocketId)) return null;

            Dictionary<int, Packet> packetValueDic = sendDic[ipOrSocketId];

            if (packetValueDic == null) return null;

            // 如果该 IP 无待发数据，跳过
            if (packetValueDic.Count <= 0) return null;

            // 处理该 IP 对应的 包
            int[] ik = new int[packetValueDic.Keys.Count];

            packetValueDic.Keys.CopyTo(ik, 0);

            if (!packetValueDic.ContainsKey(ik[0])) return null;

            return packetValueDic[ik[0]];
        }

        BasePacket getCurrentBasePacket(string ipOrSocketId) {

            if (ipOrSocketId == "" || ipOrSocketId == null) return null;

            if (!recvDic.ContainsKey(ipOrSocketId)) return null;

            Dictionary<int, BasePacket> packetValueDic = recvDic[ipOrSocketId];

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