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
    public volatile static Queue<Action<string, string>> actionQueue = new Queue<Action<string, string>>();
    public volatile static Queue<string> actionParam1 = new Queue<string>();
    public volatile static Queue<string> actionParam2 = new Queue<string>();

    void Awake() {

        if (instance == null) instance = this;

        DontDestroyOnLoad(this);

        cuteUDP = new CuteUDP("127.0.0.1", 11000, 10000);

        initPrivateVoid();

    }

    void Update() {

        if (SceneManager.GetActiveScene().name == "InitGame") {

            SceneManager.LoadScene("Title");
            
        }

        if (actionQueue.Count > 0) {

            Action<string, string> act = actionQueue.Dequeue();

            string dataString = actionParam1.Dequeue();

            string remoteIp = actionParam2.Dequeue();

            act.Invoke(dataString, remoteIp);

        }
    }

    void initPrivateVoid() {

        cuteUDP.on<string, string>("loginCheck", (string dataString, string remoteIp) => {

            Action<string, string> act = CuteUDPEvent.onLoginCheck;
            
            actionQueue.Enqueue(act);
            
            actionParam1.Enqueue(dataString);
            
            actionParam2.Enqueue(remoteIp);
        
        });
    }

    void OnApplicationQuit() {

        if (cuteUDP != null)

            cuteUDP.quitCuteUDP();

        Debug.Log("退出CuteUDP");

    }
}
