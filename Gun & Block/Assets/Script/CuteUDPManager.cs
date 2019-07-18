using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CuteUDPApp;
using UnityEngine.UI;

public class CuteUDPManager : MonoBehaviour {
    public static CuteUDPManager instance;
    public static CuteUDP cuteUDP;
    public string serverIp;
    public int serverHallPort;
    public int serverBattlePort;
    public int currentPort;
    public int localPort;
    public volatile static Queue<Action<string, string>> actionQueue = new Queue<Action<string, string>>();
    public volatile static Queue<string> actionParam1 = new Queue<string>();
    public volatile static Queue<string> actionParam2 = new Queue<string>();

    void Awake() {

        if (instance == null) instance = this;

        DontDestroyOnLoad(this);

        serverIp = "127.0.0.1";

        // serverIp = "47.104.169.23";

        serverHallPort = 10000;

        serverBattlePort = 10001;

        currentPort = serverHallPort;

        localPort = 11001;

        cuteUDP = new CuteUDP(serverIp, currentPort, localPort);

        initPrivateVoid();

    }

    void Update() {

        if (SceneManager.GetActiveScene().name == "InitGame") {

            SceneManager.LoadScene("Login");
            
        }

        if (actionQueue.Count > 0) {

            Action<string, string> act = actionQueue.Dequeue();

            string dataString = actionParam1.Dequeue();

            string sid = actionParam2.Dequeue();

            act.Invoke(dataString, sid);

        }
    }

    void switchPort() {

        if (currentPort == serverHallPort) {

            currentPort = serverBattlePort;

        } else {

            currentPort = serverHallPort;

        }

        cuteUDP.remotePort = currentPort;

        cuteUDP.emitTo("SwitchPort", "", serverIp, currentPort);

    }

    void initPrivateVoid() {

        // 用Queue队列解决了Unity子线程无法调用主线程UI的方法
        // 服务器回传 登录
        cuteUDP.on<string, string>("LoginRecv", (string dataString, string sid) => {

            addQueue(CuteUDPEvent.onLoginRecv, dataString, sid);

        
        });

        // 显示服务器
        cuteUDP.on<string, string>("ShowServerRecv", (string dataString, string sid) => {

            addQueue(CuteUDPEvent.onShowServerRecv, dataString, sid);
            
        });

        // 显示自有角色
        cuteUDP.on<string, string>("ShowRoleRecv", (string dataString, string sid) => {

            addQueue(CuteUDPEvent.onShowRoleRecv, dataString, sid);

        });

        // 创建角色成功
        cuteUDP.on<string, string>("CreateRoleRecv", (string dataString, string sid) => {

            addQueue(CuteUDPEvent.onCreateRoleRecv, dataString, sid);

        });

        // 创建角色失败
        cuteUDP.on<string, string>("CreateRoleFailRecv", (string dataString, string sid) => {

            addQueue(CuteUDPEvent.onCreateRoleFailRecv, dataString, sid);

        });

        // 删除角色
        cuteUDP.on<string, string>("DeleteRoleRecv", (string dataString, string sid) => {

            addQueue(CuteUDPEvent.onDeleteRoleRecv, dataString, sid);

        });

        // 进入游戏
        cuteUDP.on<string, string>("EnterGameRecv", (string dataString, string sid) => {

            addQueue(CuteUDPEvent.onEnterGameRecv, dataString, sid);

        });

        // 匹配等待
        cuteUDP.on<string, string>("CompareWaitRecv", (string dataString, string sid) => {

            addQueue(CuteUDPEvent.onCompareWaitRecv, dataString, sid);

        });

        // 匹配成功
        cuteUDP.on<string, string>("CompareSuccessRecv", (string dataString, string sid) => {

            addQueue(CuteUDPEvent.onCompareSuccessRecv, dataString, sid);

        });

        // 显示自定义房间
        cuteUDP.on<string, string>("ShowRoomRecv", (string dataString, string sid) => {

            addQueue(CuteUDPEvent.onShowRoomRecv, dataString, sid);

        });

    }

    void addQueue(Action<string, string> act, string dataString, string sid) {

        actionQueue.Enqueue(act);
            
        actionParam1.Enqueue(dataString);
        
        actionParam2.Enqueue(sid);

    }

    void OnApplicationQuit() {

        if (cuteUDP != null)

            cuteUDP.quitCuteUDP();

        Debug.Log("退出CuteUDP");

    }
}
