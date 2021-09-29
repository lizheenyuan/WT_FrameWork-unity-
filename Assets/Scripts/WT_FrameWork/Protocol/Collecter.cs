/***********************************************随笔说明***********************************************
 * Author:     LSG
 * Create Time: 2016-03-23
 * FileName:         Collecter.cs 
 * Description :     深圳昶为科技MR系统采集模块通讯协议
 *******************************************************************************************************/
using System;
using System.Collections;
using System.Linq;
using System.Text;
using Assets.Scripts.Public;
using Assets.Scripts.WT_FrameWork.MSGCenter;
using Assets.Scripts.WT_FrameWork.UIFramework.Manager;
using UnityEngine;


namespace Assets.Scripts.WT_FrameWork.Protocol
{

    public enum CollecterType
    {
        RD1051And1069,
        MR0808,
        MR0404,
    }

	public delegate void CollecterStateEvent(int aDevID, string devName, bool[] inState, string errorCode); 
    /// <summary>
    /// 采集模块通讯底层，适用于深圳昶为科技MR系统采集模块
    /// </summary>
    public class Collecter : WTClientSocket
    {
        private int devAddr = 1;

        private bool[] _devState;//收到的状态
        private bool[] _faultState;//发送的状态

        private string fReceiveBuffer; //接收数据缓存

        ArrayList FaultList = new ArrayList();


        public CollecterType collecterType = CollecterType.RD1051And1069; //采集模块类型 0:1051  1：MR-0808-KN  2：EMR-D0404


        public int DevAddr
        {
            get
            {
                return devAddr;
            }
            set
            {
                devAddr = value;
            }
        }

        public event CollecterStateEvent OnCollecterStateEvent, OnCollecterSetFaultEvent;      //设备状态回传事件

        public Collecter()
        {           
            QueryState();
        }

        public Collecter(CollecterType collecterType)
        {
            this.collecterType = collecterType;
            QueryState();
        }
        /// <summary>
        /// lzy 新增构造 
        /// </summary>
        /// <param name="dev_id"></param>
        /// <param name="dev_Name"></param>
        /// <param name="collecterType"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="autosenddelay">默认150 小于150会改为150</param>
        public Collecter(int dev_id,string dev_Name, CollecterType collecterType,string ip,int port,int autosenddelay=150)
        {
            this.DevName = dev_Name;
            this.DevID = dev_id;
            this.collecterType = collecterType;
            this.ServerIPAddress = ip;
            this.ServerPort = port;
            this.AutoSendDelayTime = autosenddelay;
            QueryState();
            switch (collecterType)
            {
                case CollecterType.RD1051And1069:
                    _devState = new bool[16];
                    _faultState=new bool[8];
                    break;
                case CollecterType.MR0808:
                    _devState = new bool[8];
                    _faultState = new bool[8];
                    break;
                case CollecterType.MR0404:
                    _devState = new bool[4];
                    _faultState = new bool[4];
                    break;
            }
            
            if (OnCollecterStateEvent!=null)
            {
                OnCollecterStateEvent = null;
            }

            if (OnCollecterSetFaultEvent!=null)
            {
                OnCollecterSetFaultEvent = null;
            }

            if (OnConnectStateChanged==null)
            {
                OnConnectStateChanged += OnConStateChanged;
            }
            
        }

        public void AddDevStateEvent(bool rec,bool bfault)
        {
            if (rec)
            {
                OnCollecterStateEvent += OnRd1051RecMsg;
            }

            if (bfault)
            {
                OnCollecterSetFaultEvent += OnSetFault;
            }

            onDevReadSendEvent += OnDevRedSend;
            
        }
        /// <summary>
        /// "msg_" + DevName + "_statechanged" bool "constate"
        /// </summary>
        /// <param name="b"></param>
        private void OnConStateChanged(bool b)
        {
            if (!b)
            {
                for (int i = 0; i < _devState.Length; i++)
                {
                    _devState[i] = false;
                }

                OnCollecterStateEvent?.Invoke(DevID,DevName, _devState,"");
                for (int i = 0; i < _faultState.Length; i++)
                {
                    _faultState[i] = false;
                }

                OnCollecterSetFaultEvent?.Invoke(DevID, DevName, _faultState, "");
            }
            Hashtable ht = new Hashtable();
            ht.Add("constate", b);
            CBaseEvent cbe_data = new CBaseEvent("msg_" + DevName + "_statechanged", ht, this);
            GameRoot.EventDispatcher.DispatchEvent(cbe_data);
            //Debug.Log(DevName+" changed "+b);
        }

        /// <summary>
        /// 消息名： "msg_" + devname + "_readsend"   参数: str "strdata"    int "flag"
        /// </summary>
        /// <param name="devid"></param>
        /// <param name="devname"></param>
        /// <param name="flagrs"></param>
        /// <param name="strrsdata"></param>
        private void OnDevRedSend(int devid, string devname, int flagrs, string strrsdata)
        {
            Hashtable ht = new Hashtable();
            ht.Add("strdata", devname+"("+DateTime.Now.ToString("hh:mm:ss")+")   :"+ strrsdata);
            ht.Add("flag",flagrs);
            CBaseEvent cbe_data = new CBaseEvent("msg_" + devname + "_readsend", ht, this);
            GameRoot.EventDispatcher.DispatchEvent(cbe_data);
        }

        private void OnSetFault(int adevid, string devname, bool[] instate, string errorcode)
        {
            //当前所有状态消息
            Hashtable hb_all = new Hashtable();
            hb_all.Add("all_fault", instate);
            CBaseEvent cbe_all = new CBaseEvent("msg_cur_" + devname + "_fault_all", hb_all, this);
            GameRoot.EventDispatcher.DispatchEvent(cbe_all);
        }

        /// <summary>
        /// msg_cur_rd1051_state_i 为具体某一位当前消息 一直发送 state
        /// "msg_cur_"+devname+"_state_all" 为当前1051所有状态 一直发送 all_state
        /// msg_rd1051_state_changed_i 为某一位发生变化的消息 state 为当前状态 发送一次 state
        /// </summary>
        /// <param name="adevid"></param>
        /// <param name="devname"></param>
        /// <param name="instate"></param>
        /// <param name="errorcode"></param>
        private void OnRd1051RecMsg(int adevid, string devname, bool[] instate, string errorcode)
        {
            if (string.IsNullOrEmpty(errorcode))
            {
                //添加回传事件逻辑
                if (_devState != null)
                {
                    //某一位发生变化的消息
                    for (int i = 0; i < instate.Length; i++)
                    {
                        if (_devState[i] != instate[i])
                        {
                            Hashtable hb_changed = new Hashtable();
                            hb_changed.Add("state", instate[i]);
                            CBaseEvent cbe_changed = new CBaseEvent("msg_"+devname+"_state_changed_" + i, hb_changed, this);
                            GameRoot.EventDispatcher.DispatchEvent(cbe_changed);
                        }
                    }
                    _devState = instate;

                    //当前所有状态消息
                    Hashtable hb_all = new Hashtable();
                    hb_all.Add("all_state", instate);
                    CBaseEvent cbe_all = new CBaseEvent("msg_cur_"+devname+"_state_all", hb_all, this);
                    GameRoot.EventDispatcher.DispatchEvent(cbe_all);

                    //某一位当前状态
                    for (int i = 0; i < instate.Length; i++)
                    {
                        Hashtable hb = new Hashtable();
                        hb.Add("state", instate[i]);
                        CBaseEvent cbe = new CBaseEvent("msg_cur_"+devname+"_state_" + i, hb, this);
                        GameRoot.EventDispatcher.DispatchEvent(cbe);
                    }
                }
            }
        }
        private void QueryState()      //生成自动查询字符串 适用于RD1069   MR8088KZ不能适用（区别为:输入数量只有8位strInNum = 0008）
        {
            string strDeviceAddr =  DevAddr.ToString("X2");    //设备地址(已改)
            string strFunctionCode = "02";  //功能码
            string strPhysicsBeginAddr = "0000";    //请求数据的物理起始地址
            string strInNum = "";
            if (collecterType == 0)
            {
                strInNum = "0010";                 //请求输入数量，16个位
            }
            else if(collecterType == CollecterType.MR0808)
            {
                strInNum = "0008";                 //请求输入数量，8个位
            }
            string strCmd = strDeviceAddr + strFunctionCode + strPhysicsBeginAddr + strInNum;
            strCmd = strCmd + PubFunction.GetDataCRC(strCmd);
            fAutoSendBuffer = strCmd;
        }

        public void SendRequestData()   //手工发送
        {
            SendData(fAutoSendBuffer);
        } 

        protected override void ReSolveReceiverData(string strReceiveData)
        {
            fReceiveBuffer += strReceiveData;
            /***********RD1051***************
            //正确帧信息为 设备地址(1字节) + 功能码(1字节) + 字节数量(1字节) + 输入状态(2字节) + CRC校验(2字节) 
            //错误帧信息为 设备地址(1字节) + 功能码(1字节) + 错误代码(1字节) + CRC校验(2字节)
            *******************************/
            /***********MR-0808-KZ***************
            //正确帧信息为 设备地址(1字节) + 功能码(1字节) + 字节数量(1字节) + 输入状态(1字节) + CRC校验(2字节) 
            //错误帧信息为 设备地址(1字节) + 功能码(1字节) + 错误代码(1字节) + CRC校验(2字节)
            *******************************/
            int minFrameByteNum = 5;  //错误帧为5字节
            int rightFrameByteNum = 7; //正确帧大小为7字节
            if(collecterType == CollecterType.RD1051And1069)
            {
                rightFrameByteNum = 7; //正确帧大小为7字节
            }
            else if(collecterType == CollecterType.MR0808)
            {
                rightFrameByteNum = 6; //MR-0808-KZ时，回传字节长度为6
            }
            bool frameIsRight = fReceiveBuffer.Substring(2, 2) == "02";
            string frameContent;
            if (frameIsRight)
            {                
                //长度不够正常帧，清空缓存并返回
                if (fReceiveBuffer.Length != rightFrameByteNum * 2)
                {
                    fReceiveBuffer = "";
                    return;
                }
                //获取正常帧数据
                frameContent = fReceiveBuffer.Substring(0, rightFrameByteNum * 2);
                //缓存中去除最先的一个正常帧数据
                fReceiveBuffer = fReceiveBuffer.Substring(rightFrameByteNum * 2, fReceiveBuffer.Length - rightFrameByteNum * 2);
                string strCRC = "";
                if(collecterType == CollecterType.RD1051And1069)
                {
                    strCRC = frameContent.Substring(10, 4);
                    //校验帧数据正确性
                    if (strCRC == PubFunction.GetDataCRC(frameContent.Substring(0, 10)))
                    {
                        InterpretData(frameContent);
                    }
                    else
                    {
                        return;
                    }
                }
                else if(collecterType == CollecterType.MR0808)
                {
                    strCRC = frameContent.Substring(8, 4);
                    //校验帧数据正确性
                    if (strCRC == PubFunction.GetDataCRC(frameContent.Substring(0, 8)))
                    {
                        InterpretData(frameContent);
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else if(fReceiveBuffer.Length >= minFrameByteNum * 2)
            {
                //获取异常应答帧数据
                frameContent = fReceiveBuffer.Substring(0, minFrameByteNum * 2);
                //缓存中去除最先的一个异常应答帧数据
                fReceiveBuffer = fReceiveBuffer.Substring(minFrameByteNum * 2, fReceiveBuffer.Length - minFrameByteNum * 2);
                string errorCode = frameContent.Substring(4, 2);
                bool[] inEmpty = new bool[0];
                
                if (OnCollecterStateEvent != null)
                {
                    OnCollecterStateEvent(DevID,DevName,inEmpty,errorCode);
                }
            }
            else
            {
                fReceiveBuffer = "";
                return;
            }
            //缓存中还有数据，递归调用
            if (fReceiveBuffer.Length >= minFrameByteNum * 2)
            {
                ReSolveReceiverData("");
            }

        }

        private void InterpretData(string frameContent)
        {
            string sInContent = "";
            if (collecterType == CollecterType.RD1051And1069)
            {
                sInContent = frameContent.Substring(8, 2) + frameContent.Substring(6, 2);
                string sBin = Convert.ToString(Convert.ToInt32(sInContent, 16), 2);
                bool[] bInstates = new bool[16];
                for (int i = 0; i < bInstates.Length; i++)
                {
                    if (i < sBin.Length)
                    {
                        bInstates[i] = sBin.Substring(sBin.Length - i - 1, 1) == "1";
                    }
                    else
                    {
                        bInstates[i] = false;
                    }
                }
                if (OnCollecterStateEvent != null)
                {
                    OnCollecterStateEvent(DevID, DevName, bInstates, "");
                }
            }
            else if (collecterType == CollecterType.MR0808)
            {
                sInContent = frameContent.Substring(6, 2);
                string sBin = Convert.ToString(Convert.ToInt32(sInContent, 16), 2);
                bool[] bInstates = new bool[8];
                for (int i = 0; i < bInstates.Length; i++)
                {
                    if (i < sBin.Length)
                    {
                        bInstates[i] = sBin.Substring(sBin.Length - i - 1, 1) == "1";
                    }
                    else
                    {
                        bInstates[i] = false;
                    }
                }
                if (OnCollecterStateEvent != null)
                {
                    OnCollecterStateEvent(DevID, DevName, bInstates, "");
                }
            }

        }

        public void AddFault(string StrFault)
        {
            string sTemp = StrFault;

            //zdr-1603: 在此利用字符串的替换分割，达到去除每个故障的目的，与：原delphi中AddFault的实现异曲同工
            sTemp = sTemp.Replace("+", "+,");
            string[] sTemp2 = sTemp.Split(',');

            foreach (string i in sTemp2)
                if (i != "") FaultList.Add(i);

        }

        public int RemoveFault(string strFault)        //zdr-1603: delphi原来函数名：ResetFault，寓意不明显，改为现在函数名：RemoveFault
        {
            int iResetFaultNum = 0;
            string sTemp = strFault;

            //zdr-1603: 在此利用字符串的替换分割，达到筛选单个故障，
            sTemp = sTemp.Replace("+", "+,");
            string[] sTemp2 = sTemp.Split(',');

            //zdr-1603: 去掉参数中列出的故障
            int Index = -1;
            foreach (string i in sTemp2)
            {
                Index = FaultList.IndexOf(i);
                if ((i != "") && (Index != -1))
                {
                    iResetFaultNum++;

                    FaultList.RemoveAt(Index);

                }

            }

            return iResetFaultNum;
        }

        public void RemoveAllFault()
        {
            FaultList.Clear();
        }

        public void SetFault()
        {
            if(collecterType == CollecterType.RD1051And1069)//设置故障 RD1069适用
            {
                string strDeviceAddr = devAddr.ToString("X2");    //设备地址
                string strFunctionCode = "0F";  //功能码
                string strPhysicsBeginAddr = "0010";    //请求数据的物理起始地址
                string strInNum = "000801";                 //请求输入数量，8位 + 01长度
                string strValue = GenerateErrorString();
                string strCmd = strDeviceAddr + strFunctionCode + strPhysicsBeginAddr + strInNum + strValue;
                strCmd = strCmd + PubFunction.GetDataCRC(strCmd);
                SendData(strCmd);
            }
            else if(collecterType == CollecterType.MR0808)//设置MR8088-KN系列故障
            {
                string strDeviceAddr = devAddr.ToString("X2");    //设备地址
                string strFunctionCode = "0F";  //功能码
                string strPhysicsBeginAddr = "0000";    //请求数据的物理起始地址
                string strInNum = "000801";                 //请求输入数量，8位 + 01长度
                string strValue = GenerateErrorString();
                string strCmd = strDeviceAddr + strFunctionCode + strPhysicsBeginAddr + strInNum + strValue;
                strCmd = strCmd + PubFunction.GetDataCRC(strCmd);
                SendData(strCmd);
            }
            else if(collecterType == CollecterType.MR0404)//设置D0404系列故障
            {
                string strDeviceAddr = devAddr.ToString("X2");    //设备地址
                string strFunctionCode = "0F";  //功能码
                string strPhysicsBeginAddr = "0300";    //请求数据的物理起始地址
                string strInNum = "000401";                 //请求输入数量，8位 + 01长度
                string strValue = GenerateErrorString();
                string strCmd = strDeviceAddr + strFunctionCode + strPhysicsBeginAddr + strInNum + strValue;
                strCmd = strCmd + PubFunction.GetDataCRC(strCmd);
                SendData(strCmd);
            }
            //发送后的通知事件
            if (OnCollecterSetFaultEvent!=null)
            {
                OnCollecterSetFaultEvent(DevID,DevName,_faultState,"");
            }
        } 

        private string GenerateErrorString()
        {
            string strBin = "";
            string[] sFaultPoint = new String[] { "K1+", "K2+", "K3+", "K4+", "K5+", "K6+", "K7+", "K8+" };
            for(int i = sFaultPoint.Length - 1;i >= 0; i--)
            {
                if (FaultList.Contains(sFaultPoint[i]))
                {
                    strBin += "1";
                   
                }
                else
                {
                    strBin += "0";
                }
                switch (collecterType)
                {
                    case CollecterType.RD1051And1069:
                        _faultState[i] = FaultList.Contains(sFaultPoint[i]);
                        break;
                    case CollecterType.MR0808:
                        _faultState[i] = FaultList.Contains(sFaultPoint[i]);
                        break;
                }
            }
            string returnValue = Convert.ToInt32(strBin, 2).ToString("X2");
            return returnValue;
        }

        ~Collecter()
        {
            OnCollecterStateEvent = null;
            OnConnectStateChanged = null;
        }
    }

}
