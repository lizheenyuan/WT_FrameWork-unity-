using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Scripts.WT_FrameWork.MSGCenter;
using Assets.Scripts.WT_FrameWork.Protocol;
using New = Assets.Scripts.WT_FrameWork.Protocol.New;
using Assets.Scripts.WT_FrameWork.SingleTon;
using UnityEngine;

namespace Assets.Scripts.WT_FrameWork.Managers
{
    public class WT_CollecterManager : WT_Mono_Singleton<WT_CollecterManager>
    {
        #region 声明所用到的所有设备

        private Dictionary<string, Collecter> _devs1051;
        private Dictionary<string, Collecter> _devs1069;
        private Dictionary<string, Collecter> _devs0808;
        private Dictionary<string, New.CModbusDev> _devsMeters;


        public Dictionary<string,Collecter> Devs_1051
        {
            get { return _devs1051; }
        }

        public Dictionary<string, Collecter> Devs1069
        {
            get { return _devs1069; }
        }

        public Dictionary<string, Collecter> Devs0808
        {
            get { return _devs0808; }
        }

        public Dictionary<string, New.CModbusDev> DevsMeters
        {
            get { return _devsMeters; }
        }

        #endregion

        //private bool[] _rd1051State;

        public override void Init()
        {
            base.Init();
            _devs1051 = new Dictionary<string, Collecter>();
            _devs1069 = new Dictionary<string, Collecter>();
            _devs0808 = new Dictionary<string, Collecter>();
            _devsMeters = new Dictionary<string, New.CModbusDev>();
            ////在此添加需要的设备
            ////1051
            //_devs1051.Add("rd1051_1", new Collecter(0, "rd1051_1", CollecterType.RD1051And1069, Util.Util.GetSystemConfig("RD1051_1", "IP"), int.Parse(Util.Util.GetSystemConfig("RD1051_1", "Port"))));
            ////1069
            //_devs1069.Add("rd1069_1", new Collecter(0, "rd1069_1", CollecterType.RD1051And1069, Util.Util.GetSystemConfig("RD1069_1", "IP"), int.Parse(Util.Util.GetSystemConfig("RD1069_1", "Port"))));
            ////0808
            //_devs0808.Add("mrd0808_1", new Collecter(0, "mrd0808_1", CollecterType.MR0808, Util.Util.GetSystemConfig("MRD0808_1", "IP"), int.Parse(Util.Util.GetSystemConfig("MRD0808_1", "Port"))));
            ////电表
            //_devsMeters.Add("meter_1",new New.CModbusDev("210", "meter_1") {ServerIPAddress = Util.Util.GetSystemConfig("meter_1", "IP") ,ServerPort = int.Parse(Util.Util.GetSystemConfig("meter_1", "Port")) });
        }

        /// <summary>
        /// 所有声明设备进行工作
        /// 可便利全部设置 可单一设置  选择其一
        /// </summary>
        public void ConnectAllDev()
        {
            ////单一设置
            //_devs1051["rd1051_1"].CtrlConnect = true;
            //_devs1051["rd1051_1"].AutoConnect = true;
            //_devs1051["rd1051_1"].AutoSend = true;
            //_devs1051["rd1051_1"].AddDevStateEvent();//如果是1051或0808必须 否则无对应事件

            //_devs1069["rd1069_1"].CtrlConnect = true;
            //_devs1069["rd1069_1"].AutoConnect = true;
            //_devs1069["rd1069_1"].AutoSend = false;

            //全部设置
            foreach (Collecter collecter in Devs_1051.Values)
            {
                collecter.AddDevStateEvent(true, false);
                collecter.CtrlConnect = true;
                collecter.AutoConnect = true;
                collecter.AutoSend = true;
            }
            foreach (Collecter collecter in Devs1069.Values)
            {
                collecter.AddDevStateEvent(false, true);
                collecter.CtrlConnect = true;
                collecter.AutoConnect = true;
                collecter.AutoSend = false;
                //collecter.AutoSend = true;

            }
            foreach (Collecter collecter in Devs0808.Values)
            {
                collecter.AddDevStateEvent(true, true);
                collecter.CtrlConnect = true;
                collecter.AutoConnect = true;
                //collecter.AutoSend = false;
                collecter.AutoSend = true;

            }

            foreach (var meter in DevsMeters.Values)
            {
                meter.AddDevStateEvent();
                meter.AutoSendDelayTime = 500;
                meter.CtrlConnect = true;
                meter.AutoConnect = true;
                meter.AutoSend = true;
            }
            //AddDev_Event(Rd1051, OnRd1051RecMsg);
        }

        /// <summary>
        /// 已经换到Collect类中使用不用每次添加
        /// msg_cur_rd1051_state_i 为具体某一位当前消息 一直发送 state
        /// msg_cur_rd1051_state_all 为当前1051所有状态 一直发送 all_state
        /// msg_rd1051_state_changed_i 为某一位发生变化的消息 state 为当前状态 发送一次 state
        /// </summary>
        /// <param name="adevid"></param>
        /// <param name="devname"></param>
        /// <param name="instate"></param>
        /// <param name="errorcode"></param>
        //private void OnRd1051RecMsg(int adevid, string devname, bool[] instate, string errorcode)
        //{
        //    if (string.IsNullOrEmpty(errorcode))
        //    {
        //        //添加回传事件逻辑
        //        if (_rd1051State != null)
        //        {
        //            //某一位发生变化的消息
        //            for (int i = 0; i < instate.Length; i++)
        //            {
        //                if (_rd1051State[i] != instate[i])
        //                {
        //                    Hashtable hb_changed = new Hashtable();
        //                    hb_changed.Add("state", instate[i]);
        //                    CBaseEvent cbe_changed = new CBaseEvent("msg_rd1051_state_changed_" + i, hb_changed, this);
        //                    CEventDispatcher.GetInstance().DispatchEvent(cbe_changed);
        //                }
        //            }
        //            _rd1051State = instate;

        //            //当前所有状态消息
        //            Hashtable hb_all = new Hashtable();
        //            hb_all.Add("all_state", instate);
        //            CBaseEvent cbe_all = new CBaseEvent("msg_cur_rd1051_state_all", hb_all, this);
        //            CEventDispatcher.GetInstance().DispatchEvent(cbe_all);

        //            //某一位当前状态
        //            for (int i = 0; i < instate.Length; i++)
        //            {
        //                Hashtable hb = new Hashtable();
        //                hb.Add("state", instate[i]);
        //                CBaseEvent cbe = new CBaseEvent("msg_cur_rd1051_state_" + i, hb, this);
        //                CEventDispatcher.GetInstance().DispatchEvent(cbe);
        //            }
        //        }
        //    }
        //}
        /// <summary>
        /// 清空所有控制口，清空事件，断开设备
        /// </summary>
        public void DisConnectAllDev()
        {
            RemoveAllDevEvent();
            foreach (var mdev in Devs_1051.Values)
            {
                if (mdev!=null)
                {
                    mdev.AutoConnect = false;
                    mdev.AutoSend = false;
                    mdev.CtrlConnect = false;
                }
            }
            Devs_1051.Clear();
            foreach (var mdev in Devs1069.Values)
            {
                if (mdev != null)
                {
                    mdev.RemoveAllFault();
                    mdev.SetFault();
                    mdev.AutoConnect = false;
                    mdev.AutoSend = false;
                    mdev.CtrlConnect = false;
                }
            }
            Devs1069.Clear();
            foreach (var mdev in Devs0808.Values)
            {
                if (mdev != null)
                {
                    mdev.AutoSend = false;
                    mdev.RemoveAllFault();
                    mdev.SetFault();
                    mdev.AutoConnect = false;
                    mdev.CtrlConnect = false;
                }
            }
            Devs0808.Clear();
            foreach (var meter in DevsMeters.Values)
            {
                meter.AutoConnect = false;
                meter.AutoSend = false;
                meter.CtrlConnect = false;
            }
        }

        public void AddDev_Event(Collecter c, CollecterStateEvent cevent)
        {
            if (c != null && cevent != null && _devs1051.Values.Contains(c))
            {
                c.OnCollecterStateEvent += cevent;
            }
        }

        public void RemoveDev_Event(Collecter c, CollecterStateEvent cevent)
        {
            if (c != null && cevent != null && _devs1051.Values.Contains(c))
            {
                try
                {
                    c.OnCollecterStateEvent -= cevent;
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
            }
        }

        public void RemoveAllDevEvent()
        {
            foreach (Collecter dev in _devs1051.Values)
            {
                ClearAllEvents(dev, "OnCollecterStateEvent");
                ClearAllEvents(dev, "OnCollecterSetFaultEvent");
                ClearAllEvents(dev, "onDevReadSendEvent");
                ClearAllEvents(dev, "OnConnectStateChanged");
            }
            foreach (Collecter dev in _devs0808.Values)
            {
                ClearAllEvents(dev, "OnCollecterStateEvent");
                ClearAllEvents(dev, "OnCollecterSetFaultEvent");
                ClearAllEvents(dev, "onDevReadSendEvent");
                ClearAllEvents(dev, "OnConnectStateChanged");
            }
            foreach (Collecter dev in _devs1069.Values)
            {
                ClearAllEvents(dev, "OnCollecterStateEvent");
                ClearAllEvents(dev, "OnCollecterSetFaultEvent");
                ClearAllEvents(dev, "onDevReadSendEvent");
                ClearAllEvents(dev, "OnConnectStateChanged");
            }

            foreach (var dev in DevsMeters)
            {
                ClearAllEvents(dev, "OnCollecterStateEvent");
                ClearAllEvents(dev, "OnCollecterSetFaultEvent");
                ClearAllEvents(dev, "onDevReadSendEvent");
                ClearAllEvents(dev, "OnConnectStateChanged");
            }
        }

        /// <summary>        
        /// 清除事件绑定的函数       
        /// </summary>        
        /// <param name="objectHasEvents">拥有事件的实例</param>        
        /// <param name="eventName">事件名称</param>        
        public static void ClearAllEvents(object objectHasEvents, string eventName)
        {
            if (objectHasEvents == null)
            {
                return;
            }
            try
            {
                EventInfo[] events =
                    objectHasEvents.GetType()
                        .GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (events == null || events.Length < 1)
                {
                    return;
                }
                for (int i = 0; i < events.Length; i++)
                {
                    EventInfo ei = events[i];
                    if (ei.Name == eventName)
                    {
                        FieldInfo fi = ei.DeclaringType.GetField(eventName,
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (fi != null)
                        {
                            fi.SetValue(objectHasEvents, null);
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        public override void Release()
        {
            Debug.Log("dev end");
            DisConnectAllDev();
            base.Release();
           
        }
    }
}