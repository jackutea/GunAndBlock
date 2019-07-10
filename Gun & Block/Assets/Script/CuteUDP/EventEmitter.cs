using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventEmitter {
   
    //包含所有事件的字典
    public Dictionary<string, Delegate> eventDictionary = new Dictionary<string, Delegate>();

    ///<summary>
    ///注册事件：on() 和 on T() 两种注册方式，通过 invokeEvent() 和 invokeEvent T() 触发事件
    ///</summary>
    public void on (string eventype, Action call) {

        if (!isListening(eventype, call))

            return;

        eventDictionary[eventype] = (Action)eventDictionary[eventype] + call;

    }
    //注册事件 T
    public void on<T> (string eventype, Action<T> call) {

        if (!isListening(eventype, call))

            return;

        eventDictionary[eventype] = (Action<T>)eventDictionary[eventype] + call;

    }
    //注册事件 T0, T1
    public void on<T0, T1> (string eventype, Action<T0, T1> call) {

        if (!isListening(eventype, call))

            return;

        eventDictionary[eventype] = (Action<T0, T1>)eventDictionary[eventype] + call;

    }
    //注册事件 T0, T1, T2
    public void on<T0, T1, T2> (string eventype, Action<T0, T1, T2> call) {

        if (!isListening(eventype, call))

            return;

        eventDictionary[eventype] = (Action<T0, T1, T2>)eventDictionary[eventype] + call;

    }

    //判断事件是否在字典
    public bool isListening(string eventype, Delegate call) {

        if (!eventDictionary.ContainsKey(eventype)) {

            eventDictionary.Add(eventype, null);

        }

        Delegate d = eventDictionary[eventype];

        if (d != null && d.GetType() == call.GetType()) {

            return false;

        }

        return true;
    }

    //移除事件
    public void removeListener(string eventype, Action call) {

        if (!eventDictionary.ContainsKey(eventype))

            return;

        eventDictionary[eventype] = (Action)eventDictionary[eventype] - call;

    }

    //移除事件 T
    public void removeListener<T>(string eventype, Action<T> call) {

        if (!eventDictionary.ContainsKey(eventype))

            return;

        eventDictionary[eventype] = (Action<T>)eventDictionary[eventype] - call;

    }

    //移除事件 T0, T1
    public void removeListener<T0, T1>(string eventype, Action<T0, T1> call) {

        if (!eventDictionary.ContainsKey(eventype))

            return;

        eventDictionary[eventype] = (Action<T0, T1>)eventDictionary[eventype] - call;

    }

    //移除事件 T0, T1, T2
    public void removeListener<T0, T1, T2>(string eventype, Action<T0, T1, T2> call) {

        if (!eventDictionary.ContainsKey(eventype))

            return;

        eventDictionary[eventype] = (Action<T0, T1, T2>)eventDictionary[eventype] - call;

    }

    //触发事件
    public void invokeEvent(string eventype) {

        Delegate d = null;

        if (!eventDictionary.TryGetValue(eventype, out d))

            return;

        if (d == null)

            return;

        Delegate[] calls = d.GetInvocationList();

        for (int i = 0; i < calls.Length; i += 1) {

            Action call = calls[i] as Action;

            if (call == null)

                continue;

            call();

        }
    }
    //触发事件 T
    public void invokeEvent<T>(string eventype, T arg) {
       
        Delegate d = null;
       
        if (!eventDictionary.TryGetValue(eventype, out d))
       
            return;
       
        if (d == null)
       
            return;
       
        Delegate[] calls = d.GetInvocationList();
       
        for (int i = 0; i < calls.Length; i += 1) {
       
            Action<T> call = calls[i] as Action<T>;
       
            if (call == null)
       
                continue;
       
            call(arg);
       
        }
    }
    //触发事件 T0, T1
    public void invokeEvent<T0, T1>(string eventype, T0 arg1, T1 arg2) {
        
        Delegate d = null;
        
        if (!eventDictionary.TryGetValue(eventype, out d))
        
            return;
        
        if (d == null)
        
            return;
        
        Delegate[] calls = d.GetInvocationList();
        
        for (int i = 0; i < calls.Length; i += 1) {
        
            Action<T0, T1> call = calls[i] as Action<T0, T1>;
        
            if (call == null)
        
                continue;
        
            call(arg1, arg2);
        
        }
    }

    //触发事件 T0, T1, T2
    public void invokeEvent<T0, T1, T2>(string eventype, T0 arg1, T1 arg2, T2 arg3) {

        Delegate d = null;
        
        if (!eventDictionary.TryGetValue(eventype, out d)) {
            
            Debug.Log(eventype + "未监听");
        
            return;
        }
        
        if (d == null)
        
            return;
        
        Delegate[] calls = d.GetInvocationList();
        
        for (int i = 0; i < calls.Length; i += 1) {
        
            Action<T0, T1, T2> call = calls[i] as Action<T0, T1, T2>;
        
            if (call == null)
        
                continue;
        
            call(arg1, arg2, arg3);
        
        }
    }
}
