﻿using System;
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
    public volatile static Queue<Action<string>> actionQueue = new Queue<Action<string>>();
    public volatile static Queue<string> actionParam1 = new Queue<string>();

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

        PlayerDataScript.sid = CuteUDP.socketId;

        initPrivateVoid();
    }

    void Update() {

        if (SceneManager.GetActiveScene().name == "InitGame") {

            SceneManager.LoadScene("Login");
            
        }

        if (actionQueue.Count > 0) {

            Action<string> act = actionQueue.Dequeue();

            string dataString = actionParam1.Dequeue();

            act.Invoke(dataString);

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
        cuteUDP.on<string>("Login", (string dataString) => {

            addQueue(CuteUDPEvent.onLogin, dataString);
        
        });

        cuteUDP.on<string>("Register", (string dataString) => {

            addQueue(CuteUDPEvent.onLogin, dataString);
        
        });

        // 显示服务器
        cuteUDP.on<string>("ShowServer", (string dataString) => {

            addQueue(CuteUDPEvent.onShowServer, dataString);
            
        });

        // 显示自有角色
        cuteUDP.on<string>("ShowRoles", (string dataString) => {

            addQueue(CuteUDPEvent.onShowRoles, dataString);

        });

        // 创建角色成功
        cuteUDP.on<string>("CreateRole", (string dataString) => {

            addQueue(CuteUDPEvent.onCreateRole, dataString);

        });

        // 创建角色失败
        cuteUDP.on<string>("CreateRoleFail", (string dataString) => {

            addQueue(CuteUDPEvent.onCreateRoleFail, dataString);

        });

        // 删除角色
        cuteUDP.on<string>("DeleteRole", (string dataString) => {

            addQueue(CuteUDPEvent.onDeleteRole, dataString);

        });

        // 进入游戏
        cuteUDP.on<string>("EnterGame", (string dataString) => {

            addQueue(CuteUDPEvent.onEnterGame, dataString);

        });

        // 匹配等待
        cuteUDP.on<string>("CompareWait", (string dataString) => {

            addQueue(CuteUDPEvent.onCompareWait, dataString);

        });

        // 匹配成功
        cuteUDP.on<string>("CompareSuccess", (string dataString) => {

            addQueue(CuteUDPEvent.onCompareSuccess, dataString);

        });

        // 显示自定义房间
        cuteUDP.on<string>("ShowRoom", (string dataString) => {

            addQueue(CuteUDPEvent.onShowRoom, dataString);

        });

    }

    void addQueue(Action<string> act, string dataString) {

        actionQueue.Enqueue(act);
            
        actionParam1.Enqueue(dataString);

    }

    void OnApplicationQuit() {

        if (cuteUDP != null)

            cuteUDP.quitCuteUDP();

        Debug.Log("退出CuteUDP");

    }
}
