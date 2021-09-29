using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.WT_FrameWork.MSGCenter.Interface;
using Assets.Scripts.WT_FrameWork.UIFramework.Manager;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace Assets.Scripts.WT_FrameWork.MSGCenter
{
    public class CMsgObjBase : MonoBehaviour,ICmessage
    {
        private CBaseEvent _cBaseEvent;
        private Hashtable _msgArgs;
        private Dictionary<string, CEventListenerDelegate> _eventDic;
        protected CEventDispatcher MsgCenter=>GameRoot.EventDispatcher;
        public CBaseEvent CBE {
            get
            {
                if (_cBaseEvent==null)
                {
                    _cBaseEvent=new CBaseEvent("",null,this);
                }
                return _cBaseEvent;
            }
        }

        public Hashtable MsgArgs
        {
            get
            {
                if (_msgArgs==null)
                {
                    _msgArgs=new Hashtable();
                    _msgArgs.Add("arg",0);
                    _msgArgs.Add("args",null);
                }
                return _msgArgs;
            }
        }

        public Dictionary<string, CEventListenerDelegate> EventDic
        {
            get
            {
                if (_eventDic==null)
                {
                    _eventDic = new Dictionary<string, CEventListenerDelegate>();
                }
                return _eventDic;
            }
        }

        public virtual void AddListenEvent(string eventName, Action<object, object[]> eAction)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                Debug.Log($"Msg_Err: eventName Can`t be null or Empty!");
                return;
            }

            if (eAction==null)
            {
                Debug.Log($"Msg_Err: add null event to '{eventName}' auto ignored");
                return;
            }
            if (EventDic.ContainsKey(eventName))
            {
                Debug.Log($"Msg_Err:{eventName}: has added, one msg name can only add once \r\n added {eventName} event failed!");
                return;
            }
            CEventListenerDelegate celd=new CEventListenerDelegate((cbe) =>
            {
                try
                {
                    if (cbe.Sender==this)
                    {
                        return;
                    }

                    if (cbe.Argments["args"] ==null)
                    {
                        eAction?.Invoke(cbe.Argments["arg"], cbe.Argments["args"] as object[]);
                    }
                    else
                    {
                        eAction?.Invoke(cbe.Argments["arg"], null);
                    }
                    
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message + "\r\n" + e.StackTrace);
                }

            });
            MsgCenter.AddEventListener(eventName, celd);
            EventDic.Add(eventName,celd);
        }

        public virtual void RemoveListenEvent(string eventName)
        {
            if (EventDic.ContainsKey(eventName))
            {
                try
                {
                    MsgCenter.RemoveEventListener(eventName, EventDic[eventName]);
                    EventDic.Remove(eventName);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message + "\r\n" + e.StackTrace);
                }
            }
            
        }

        public virtual void DispatchEvent(string eventname, object arg, object[] args)
        {
            if (string.IsNullOrEmpty(eventname))
            {
                Debug.Log($"DispatchEvent_Err: eventName Can`t be null or Empty!");
                return;
            }
            var cbe = CBE.Clone();
            cbe.EventName = eventname;
            cbe.Argments["arg"] = arg;
            cbe.Argments["args"] = args;
            MsgCenter.DispatchEvent(cbe);
        }

        public int RemoveAllEvent()
        {
            foreach (var cEventListenerDelegate in EventDic)
            {
                try
                {
                    MsgCenter.RemoveEventListener(cEventListenerDelegate.Key, cEventListenerDelegate.Value);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message + "\r\n" + e.StackTrace);
                }
            }

            int i = EventDic.Count;
            EventDic.Clear();
            return i;
        }
        protected virtual void Start() { }
        protected virtual void Awake() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
    }
}
