using System;
using Assets.Scripts.WT_FrameWork.MSGCenter;
using Assets.Scripts.WT_FrameWork.Protocol;
using Assets.Scripts.WT_FrameWork.Protocol.New;
using Assets.Scripts.WT_FrameWork.UIFramework.Manager;
using UnityEngine;
using UnityEngine.UI;
using CModbusDev = Assets.Scripts.WT_FrameWork.Protocol.New.CModbusDev;

namespace Assets.Scripts.WT_FrameWork.Dev
{
    public class DevItemMeter : DevItemBase
    {
        private Text t_cur_value;
        private int addr;

        public int Addr
        {
            get { return addr; }
            set { addr = value; }
        }

        private CModbusDev cmd;
        protected override void AddStatesListener()
        {
            GameRoot.EventDispatcher.AddEventListener("msg_" + cmd.DevName + "_statechanged", OnConStateChanged);
            Debug.Log("item meter:" + "msg_" + cmd.DevName + "_statechanged");
            GameRoot.EventDispatcher.AddEventListener(cmd.GetMeterValueMsgByAddr(addr), OnGetVaule);
            //Debug.Log("meter listen:"+cmd.GetMeterValueMsgByAddr(addr));
            GameRoot.EventDispatcher.AddEventListener("msg_" + cmd.DevName + "_readsend", OnGetReadSend);
        }
        private void OnGetReadSend(CBaseEvent cet)
        {
            if ((int)cet.Argments["flag"] == 1)
            {
                Debug.Log("send: " + cet.Argments["strdata"]);
            }
            else
            {
                //Debug.Log("rec: " + cet.Argments["strdata"]);
            }

        }
        public override void LoadDev(WTClientSocket c)
        {
           
            cmd = c as CModbusDev;
            //Debug.Log(cmd.DevName);
            t_cur_value = transform.Find("curvalue").GetComponent<Text>();
            nameText = transform.Find("dev_name").GetComponent<Text>();
            ipporText = transform.Find("ipport").GetComponent<Text>();
            devState = transform.Find("connect_state").GetComponent<Image>();
            nameText.text = c.DevName;
            ipporText.text = c.ServerIPAddress + ":" + c.ServerPort;
            nameText.text = cmd.GetMeterName(addr);
            AddStatesListener();
        }

        private void OnGetVaule(CBaseEvent cet)
        {
            S_Meter s_meter = (S_Meter)cet.Argments["s_meter"] ;
            string s = "";
            switch (s_meter.dt)
            {
                case DevType.UnKnow:
                    s = "数据解析失败";
                    break;
                case DevType.DVoltmeter:
                    s = s_meter.meter_val.ToString("F")+" V";
                    break;
                case DevType.Ammeter:
                    s = s_meter.meter_val.ToString("F") + " mA";
                    break;
                case DevType.AVoltmeter:
                    s = s_meter.meter_val.ToString("F") + " V";
                    break;
            }

            t_cur_value.text = s;
        }
    }
}
