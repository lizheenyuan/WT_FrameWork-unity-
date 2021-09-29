/***********************************************随笔说明***********************************************
 * Author:              ZhangDongRui (En: Keri)
 * FileName:         ModbudDev.cs 
 * Description :     数显电测表设备通信
 * Range of application:  C#平台下的“数显电测表”的网口通信
 * 
 * Comments:       1, 本脚本(ModbusDev.cs)参考《数显电测仪表RS-485通信协议》  和 网络资源编写
 *                                2, 参考波特率配置如： 9600,N,8,1
 *                                3, 本脚查询命令，和回应信息的分析   ：-->    只是针对数显单相表；三相表Code可留作后续扩充
 * 
 * 
 *************** **************************参考示例Code说明*******************************************
           CModbusDev ModbusDev_V = new CModbusDev();
           ModbusDev_V.DevAddr = 1;                                                     //zdr-1603:  数显表设备地址，
           ModbusDev_V.DevID = 1;                                                          //zdr:设置ID号    
           ModbusDev_V.DevName = "ModbusDev-V-1";               //zdr: 设置名字 
           ModbusDev_V.ServerIPAddress = "192.168.1.62";          //zdr-1603: 设备IP地址
           ModbusDev_V.ServerPort = 4196;                                         //zdr-1603: 设备端口号
           ModbusDev_V.AutoSendDelayTime =1000;                      //zdr-1603: 设置自动查询命令的时间间隔，单位，毫秒；
           ModbusDev_V.AutoSend = true;                                           //zdr-1603: 默认关闭; 开启自动查询功能 ,  需设置该变量为: true


           //zdr-1603: 注意这三个变量的设置关系，如果AutoConnect设置为真，那么即使CtrlConnect设置为false，自动重连也会设置CtrlConnect为：true；使设备连接
           ModbusDev_V.AutoConnect = false;                                   //zdr-1603:  true，表示断线自动重连； 设置false, 则需要手动设置CtrlConnect，进行连接
           ModbusDev_V.AutoConnectTime = 4;                                //zdr-1603:   默认值为：5秒, 检测断线重连的时间，间隔；
           ModbusDev_V.CtrlConnect = true;                                      //zdr-1603: 当自动重连AutoConnect，设置为false，需要设置该值为真，使之连接设备

           ModbusDev_V.OnVolAhEvent += new CVolAhEvent(fVolAhEvent);   //zdr-1603: 回调事件定义 
 * 
 * 
 * 新修改部分
 *******************************************************************************************************
 * 2018/04/20 lzw&lzy&LXY
 * 修改了构造函数中直接可以确定数显表个数和位置
 * public CModbusDev(string Devs)            “  表1   表2    表3   表4”
 * “Devs”例如 1101 即一共四块数显表 即为    电流  电流  电压  电流 
 *       设备 地址                                 iDevAddr:   01     02     03    04
 * 其中电流1即为DevAdd=1，电流2为DevAdd=2，以此类推。。。
 * 其中AutoSendDelayTime为查询一块表的时间间隔 如上表数为四个，AutoSendDelayTime=1000ms 四块表查询一遍需要4000ms，如需要数据及时更新可根据需求设置AutoSendDelayTime的值。
 ******************************************************************************************************/

using System;
using System.Collections;
using System.Threading;
using Assets.Scripts.Public;
using Assets.Scripts.WT_FrameWork.MSGCenter;
using Assets.Scripts.WT_FrameWork.UIFramework.Manager;
using UnityEngine;

//zdr-1603: 加载公共函数接口： pubFunction


namespace Assets.Scripts.WT_FrameWork.Protocol.New
{

    //zdr-1603: 开关投入状态：   0 开 1 合；     定义委托，具体实现过程，在调用TCH98Dev对象中，进行处理
    public delegate void CVolAhEvent(int aDevID, string devName, int iDevAddr, float fVorA,DevType devType);
    public enum DevType
    {
        UnKnow = 255,
        DVoltmeter = 0,
        Ammeter = 1,
        AVoltmeter=2
    }

    public struct S_Meter
    {
        public int devId;
        public string devName;
        public int devAddr;
        public float meter_val;
        public DevType dt;
    }

    public class CModbusDev : WTClientSocket
    {
        private int iDevAddr;
        //lzy-18-04 添加标识表的字符队列  0:电压表 1：电流表
        private string dev_array;
        private string fReceiveBuffer;   //zdr-1603: 接收-收到的字符串
        public CVolAhEvent OnVolAhEvent = null;      //设备状态回传事件
        private int devNum = 1;  //级联设备数目
        //zdr-1603: 默认构造函数
        public CModbusDev()
        {
            //lzy-18-04 添加标识表的字符队列 默认一个电压表
            dev_array = "0";
            devNum = 1;
            //zdr-1604: 设备自动查询命令写的不是很完美，漏洞在于：m_iDevAddr是可以变化的，
            //                     好的处理办法是：重新处理构造函数中的自动查询命令，把参数设置为属性类型，即改变iDevAddr时，fAutoSendBuffer也会随之改变
            iDevAddr = 1;  //zdr-1603: 设备默认地址，为：1                 
            //zdr-1603: 设置自动查询命令
            string strCmd = string.Format("{0:X2}", iDevAddr) + "03000A0002";
            strCmd = strCmd + PubFunction.GetDataCRC(strCmd);
            fAutoSendBuffer = strCmd;
            Thread changeDevAddrThread = new Thread(new ThreadStart(ChangeDevAddrThread));
            changeDevAddrThread.Start();

        }
        /// <summary>
        /// lzy
        /// </summary>
        /// <param name="Devs">
        /// 0->V  1->A       101->VAV
        /// </param>
        public CModbusDev(string Devs) : this(Devs.Length)
        {
            dev_array = string.IsNullOrEmpty(Devs) ? "0" : Devs;
            devNum = dev_array.Length;
            OnVolAhEvent = null;
            OnVolAhEvent += OnGetAorV;
            if (OnConnectStateChanged == null)
            {
                OnConnectStateChanged += OnConStateChanged;
            }
            

        }

        public CModbusDev(string Devs,string dName): this(Devs.Length)
        {
            DevName = dName;
            dev_array = string.IsNullOrEmpty(Devs) ? "0" : Devs;
            devNum = dev_array.Length;
            if (OnVolAhEvent==null)
            {
                OnVolAhEvent += OnGetAorV;
            }
            if (OnConnectStateChanged == null)
            {
                OnConnectStateChanged += OnConStateChanged;
            }
        }
        public void AddDevStateEvent()
        {
            onDevReadSendEvent += OnDevRedSend;
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
            ht.Add("strdata", devname + "(" + DateTime.Now.ToString("hh:mm:ss") + ")   :" + strrsdata);
            ht.Add("flag", flagrs);
            CBaseEvent cbe_data = new CBaseEvent("msg_" + devname + "_readsend", ht, this);
            GameRoot.EventDispatcher.DispatchEvent(cbe_data);
        }
        private void OnConStateChanged(bool b)
        {
            Hashtable ht = new Hashtable();
            ht.Add("constate", b);
            CBaseEvent cbe_data = new CBaseEvent("msg_" + DevName + "_statechanged", ht, this);
            GameRoot.EventDispatcher.DispatchEvent(cbe_data);
            //Debug.Log("meter :"+b);
        }
        private void OnGetAorV(int adevid, string devname, int idevaddr, float fvora, DevType devtype)
        {
            //idevaddr -= 1;
            Hashtable ht = new Hashtable();
            S_Meter s_meter = new S_Meter(){devId = adevid,devAddr = idevaddr,devName = devname,dt = devtype,meter_val = fvora};
            ht.Add("s_meter", s_meter);
            CBaseEvent cbe_data = new CBaseEvent(GetMeterValueMsgByAddr(idevaddr), ht, this);
            //Debug.Log("cmd dispatch:"+ GetMeterValueMsgByAddr(idevaddr));
            GameRoot.EventDispatcher.DispatchEvent(cbe_data);
        }

        protected CModbusDev(int devNum)
        {
            this.devNum = devNum;
            
            Thread changeDevAddrThread = new Thread(new ThreadStart(ChangeDevAddrThread));
            changeDevAddrThread.Start();
        }
        //zdr-1603: 默认析构函数
        ~CModbusDev()
        {
            OnVolAhEvent = null;
            OnConnectStateChanged = null;
        }

        //zdr-1604:  新增加自动查询命令，主要是改变构造函数中iDevAddr = 1;写死的状态
        //zdr-1604: 因为构造函数随着实例化而执行，解决办法有二：一个是：写个带参的构造函数，传递进去iDevAddr； 另外一种是：写一个关于  iDevAddr  的属性类型
        //zdr-1604: 前一种方法，写一个带参的构造函数，还不够灵活，如果在一个IP带动多台设备，且根据iDevAddr来区别不同设备的命令时，这个局限性就暴漏出来了
        //zdr-1604: 综上所述，另外再写一个  iDevAddr  的属性类型，来实现一个IP下的不同设备查询
        public int DevAddr
        {
            get { return iDevAddr; }
            set
            {
                iDevAddr = value;
                //lzy-18-04 转换设备类型
                DevType d_type = (DevType)int.Parse(dev_array[value-1].ToString());
                string strCmd = "";
                switch (d_type)
                {
                    //众犇 AC 03000A0002 DC 0300100002
                    case DevType.DVoltmeter:
                        strCmd = string.Format("{0:X2}", iDevAddr) + "0300100002";
                        break;
                    //新电流用 03004C0002 老电流用 0300090006 WT_SX01  0300000002  只要是万特的毫安表（WT_SX01） 这三个都能用
                    case DevType.Ammeter:
                        strCmd = string.Format("{0:X2}", iDevAddr) + "0300090006";
                        break;
                    case DevType.AVoltmeter:
                        strCmd = string.Format("{0:X2}", iDevAddr) + "03000A0002";
                        break;

                }
                strCmd = strCmd + PubFunction.GetDataCRC(strCmd);
                fAutoSendBuffer = strCmd;
            }
        }

        public int DevNum
        {
            get { return devNum; }
        }

        public string DevArray
        {
            get { return dev_array; }
        }

        public string GetMeterValueMsgByAddr(int addr)
        {
            if (addr>devNum)
            {
                return null;
            }

            string dtype = dev_array[addr] == '0' ? "V" : "A";
            return $"msg_cur_{DevName}_{addr}_({dtype})";
        }

        public string GetMeterName(int addr)
        {
            if (addr > devNum)
            {
                return null;
            }
            return DevName + "_" + addr;
        }

        private DevType GetDevType(char d_type)
        {
            return (DevType)int.Parse(d_type.ToString());
        }
        //lzy-18-04 修改查询方法
        private void ChangeDevAddrThread()//不断切换查询的地址在1和2来回切换
        {
            int cur_QueryNo = 0;
            while (true)
            {
                if (cur_QueryNo < int.MaxValue)
                {
                    ++cur_QueryNo;
                    DevAddr = cur_QueryNo % devNum != 0 ? (cur_QueryNo % devNum) : devNum;
                }
                else
                {
                    cur_QueryNo = 0;
                }
                Thread.Sleep(AutoSendDelayTime);
            }
        }
        //zdr-1603: ----------------------------------接口实现如下---------------------------------------------

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //zdr-1603: 构造并且发送命令, strData为16进制的字符串
        private void BuildAndSendCMD(int iCmdNum, int iStartAddr, int iAddrLen, string strData = "")
        {
            //构造命令部分, 
            string strCMD = string.Format("{0:X2}", iDevAddr) + string.Format("{0:X2}", iCmdNum) +
                                            string.Format("{0:X4}", iStartAddr) + string.Format("{0:X4}", iAddrLen) + strData;

            strCMD = strCMD + PubFunction.GetDataCRC(strCMD);

            //zdr-1603: 发送命令道设备
            SendData(strCMD);
        }


        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //zdr-1603: 发送查询电压，电流命令
        //public void QueryVAPFOfDev()    //zdr-1603: 适合数显单相表
        //{
        //    //zdr-1603: 单相表，比较简单，主要根据设备地址，来进行区别
        //    //zdr: 单相电压表：发送查询命令： 01 03 00 00 00 02 C4 0B,  回传命令： 01 03 04 43 65 80 00 9E 68
        //    //zdr: 单相电流表：发送查询命令： 01 03 00 00 00 02 C4 0B,  回传命令： 01 03 04 00 00 00 00 FA 33
        //    //zdr-1603:下面的命令码参考，tg007构造查询命令
        //    int CmdNum = 0X03;      //zdr-1603: 读取数据命令码
        //    int StartAddr = 0X00;    //zdr-1603:   0000
        //    int AddrLen = 0X02;    //zdr-1603:    00x2
        //    BuildAndSendCMD(CmdNum, StartAddr, AddrLen);
        //}
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //zdr-1603: 解析设备回传数据, 参考TG007，书写回传数据
        //zdr-1603:  例子：单相电压表：完整的回传数据形如：“ 01 03 04 43 65 80 00 9E 68” ， 共计   9字节( 即：18 位)
        //zdr-1603: 最后2字节位：CRC， CRC前面的4字节，电表电流值，或者电压值， 例如：43 65 80 00，表示电压值
        protected override void ReSolveReceiverData(string strReceiveData)
        {
            string ModbusDevResponseData = "";
            string sCRCValue = "";
            string sVorAData = "";
            //float fVorAValue = 0f;
            fReceiveBuffer += strReceiveData;
            //zdr-1603:　从最后面查找，可以保证数据是最新的
            int iPosition = fReceiveBuffer.LastIndexOf(string.Format("{0:X2}", iDevAddr) + "0304");     //zdr-1603: 索引从0开始，没有找到返回-1
            if (iPosition < 0)
            {
                fReceiveBuffer = "";
                return;
            }
            ModbusDevResponseData = fReceiveBuffer.Substring(iPosition, fReceiveBuffer.Length - iPosition);
            //zdr-1603: 接收数据不完整，直接返回
            if (ModbusDevResponseData.Length < 18)
                return;
            //zdr-1603: ---------------------------> 开始解析：电压，电流
            //zdr-1603: 获取完整回传数据
            ModbusDevResponseData = ModbusDevResponseData.Substring(0, 18);   //  形如： ModbusDevResponseData = 010304436580009E68
            sCRCValue = PubFunction.GetDataCRC(ModbusDevResponseData.Substring(0, 14));
            //zdr-1603:  接收字符串瘦身
            fReceiveBuffer = fReceiveBuffer.Substring(iPosition + 18, fReceiveBuffer.Length - iPosition - 18);
            if (sCRCValue != ModbusDevResponseData.Substring(14, 4))  //zdr-1603: 校验和是否相等
            {
                return;      //zdr-1603: 接收数据有误，丢弃掉此次数据解析
            }

            //zdr-1603: get V， or  A
            //  sVorAData = ModbusDevResponseData.Substring(6, 8);    //01 03 04 "43 65 80 00" 9E 68
            //float fVorAValue = MathFloat(sVorAData);

            //18-04 lzy 通过Type得到VorA 的value
            float fVorAValue = 0;
            int d_no = int.Parse(ModbusDevResponseData[1].ToString());
            DevType c_dev = GetDevType(dev_array[d_no-1]);
            switch (c_dev)
            {
                //老电流用MathFloat（6，8） 新电流用GetAmmeterValue（6，4）
                case DevType.Ammeter:
                    fVorAValue = MathFloat(ModbusDevResponseData.Substring(6, 8));
                    break;
                case DevType.DVoltmeter:
                    fVorAValue = GetVoltmeterDCValue(ModbusDevResponseData.Substring(6, 4));           
                    break;
                case DevType.AVoltmeter:
                    fVorAValue = MathFloat(ModbusDevResponseData.Substring(6, 8));
                    break;
            }

            //zdr-1603: 状态回传函数调用, 其中：DevID, DevName为继承WTClientSocket中的属性变量
            if (OnVolAhEvent != null)
            {
                OnVolAhEvent(DevID, DevName, d_no-1, fVorAValue, c_dev);
            }

            if (d_no==1)
            {
                Debug.Log("A:  "+fVorAValue);
            }
            //zdr-1603: 控制字符串大小
            if (fReceiveBuffer.Length >= 200)
                fReceiveBuffer.Substring(200, fReceiveBuffer.Length - 200);
        }

        private float GetVoltmeterACValue(string v_val)
        {
            UInt32 v = Convert.ToUInt32(v_val,16);
            byte[] bts = StrToToHexByte(v_val);
            byte[] bts1 = BitConverter.GetBytes(v);
            return BitConverter.ToSingle(BitConverter.GetBytes(v), 0);
        }

        private float GetAmmeterValue(string a_val)
        {
            return Convert.ToInt32(a_val, 16) / 1000.0f;
        }
        private float GetVoltmeterDCValue(string a_val)
        {
            return Convert.ToInt32(a_val, 16) / 100.0f;
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //zdr-1603: Modbus浮点数读取处理， 从网络上摘抄
        //zdr-1603: 网络能人倍出，这个code写的清爽宜人，赞起
        private float MathFloat(string strFloatData)      //zdr-1603:  参数为16进制字符串，共8位
        {
            int x1, x2;    //x1,x2为读取到浮点数的2个16位寄存器的整形数据

            int fuhao, fuhaoRest, exponent, exponentRest;
            float value, weishu;

            strFloatData = strFloatData.Trim();  //zdr: 移除字符串的前后空格

            x1 = Convert.ToInt32(strFloatData.Substring(0, 4), 16);
            x2 = Convert.ToInt32(strFloatData.Substring(4, 4), 16);
            fuhao = x1 / 32768;
            fuhaoRest = x1 % 32768;
            exponent = fuhaoRest / 128;
            exponentRest = fuhaoRest % 128;
            weishu = (float)(exponentRest * 65536 + x2) / 8388608;
            value = (float)Math.Pow(-1, fuhao) * (float)Math.Pow(2, exponent - 127) * (weishu + 1);
            return value;
        }



    }
}
