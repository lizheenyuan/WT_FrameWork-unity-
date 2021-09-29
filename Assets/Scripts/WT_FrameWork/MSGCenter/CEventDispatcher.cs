using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.WT_FrameWork.SingleTon;
using Assets.Scripts.WT_FrameWork.Util;
using UnityEngine;

namespace Assets.Scripts.WT_FrameWork.MSGCenter
{
    public delegate void CEventListenerDelegate(CBaseEvent cet);

    public class CEventDispatcher : WT_Mono_Singleton<CEventDispatcher>
    {
        public override void Init()
        {
            base.Init();
            msgQueue=new Queue<CBaseEvent>();
        }

        public static CEventDispatcher Get()
        {
            return GetInstance();
        }

        private Hashtable listeners = new Hashtable();

        public void AddEventListener(string eventName, CEventListenerDelegate listener)
        {
            CEventListenerDelegate ceventListenerDelegate = this.listeners[eventName] as CEventListenerDelegate;
            ceventListenerDelegate = (CEventListenerDelegate) Delegate.Combine(ceventListenerDelegate, listener);
            this.listeners[eventName] = ceventListenerDelegate;
        }

        public void RemoveEventListener(string eventName, CEventListenerDelegate listener)
        {
            CEventListenerDelegate ceventListenerDelegate = this.listeners[eventName] as CEventListenerDelegate;
            if (ceventListenerDelegate != null)
            {
                ceventListenerDelegate = (CEventListenerDelegate) Delegate.Remove(ceventListenerDelegate, listener);
            }
            this.listeners[eventName] = ceventListenerDelegate;
        }

        public void RemoveAllEventListener(string eventName)
        {
            lock (this.listeners)
            {
                if (this.listeners.ContainsKey(eventName))
                {
                    //            this.listeners.Remove(this.listeners[eventName]);
                    this.listeners[eventName] = null;
                }
            }

        }

        public void RemoveAll()
        {
            this.listeners.Clear();
        }

        private Queue<CBaseEvent> msgQueue;
        public void DispatchEvent(CBaseEvent evt)
        {
            Debug.Log(evt.ToString());
            if (this.listeners[evt.EventName] == null)
            {
                Debug.Log(evt.EventName + "为空。");
                return;
            }
           
                try
                {
                    msgQueue.Enqueue(evt);
                    //Loom.RunAsync(
                    //    () =>
                    //    {
                    //        Loom.QueueOnMainThread(() =>
                    //        {
                    //            celd(evt);
                    //        });
                    //    }
                    //    );
                    
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.StackTrace + " \nmsg:" + ex.Message);
                }
            
        }

        void Update()
        {
            if (msgQueue!=null&&msgQueue.Count>0)
            {
                try
                {
                    var evt=msgQueue.Dequeue();
                    CEventListenerDelegate celd = this.listeners[evt.EventName] as CEventListenerDelegate;
                    celd?.Invoke(evt);
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message+e.StackTrace);
                }
            }
        }
    }
}