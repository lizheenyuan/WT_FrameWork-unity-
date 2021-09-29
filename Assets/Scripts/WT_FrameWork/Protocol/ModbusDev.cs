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
 ******************************************************************************************************/

using System;
using System.Threading;
using Assets.Scripts.Public;

//zdr-1603: 加载公共函数接口： pubFunction


namespace Assets.Scripts.WT_FrameWork.Protocol
{

    //zdr-1603: 开关投入状态：   0 开 1 合；     定义委托，具体实现过程，在调用TCH98Dev对象中，进行处理
    public delegate void CVolAhEvent(int aDevID, string devName,int iDevAddr, float fVorA);

    public class CModbusDev : WTClientSocket
    {
        private int iDevAddr;
        private string fReceiveBuffer;   //zdr-1603: 接收-收到的字符串
        public CVolAhEvent OnVolAhEvent = null;      //设备状态回传事件

        private int devNum = 2;  //级联设备数目


        ////zdr-1604: 定义一个显示电压or电流的属性
//         public float VorAValue
//         {
//             get
//             {
//                 if (base.CtrlConnect) { return fVorAValue; }
//                 else { return 0f; }
//             }
//             set
//             {
//                 fVorAValue = value;
//             }
//         }



          //zdr-1603: 默认构造函数
        public CModbusDev()
        {

            //zdr-1604: 设备自动查询命令写的不是很完美，漏洞在于：m_iDevAddr是可以变化的，
            //                     好的处理办法是：重新处理构造函数中的自动查询命令，把参数设置为属性类型，即改变iDevAddr时，fAutoSendBuffer也会随之改变


            iDevAddr =1;  //zdr-1603: 设备默认地址，为：1           

            
            //zdr-1603: 设置自动查询命令
            string strCmd = string.Format("{0:X2}", iDevAddr)  + "0300000002";
            strCmd = strCmd + PubFunction.GetDataCRC(strCmd);
            fAutoSendBuffer = strCmd;
            Thread changeDevAddrThread = new Thread(new ThreadStart(ChangeDevAddrThread));
            changeDevAddrThread.Start();

        }

        public CModbusDev(int devNum)
        {
            this.devNum = devNum;
            Thread changeDevAddrThread = new Thread(new ThreadStart(ChangeDevAddrThread));
            changeDevAddrThread.Start();
        }


        //zdr-1603: 默认析构函数
        ~CModbusDev()
        {

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

                //zdr-1604: 设置自动查询命令
                string strCmd = string.Format("{0:X2}", iDevAddr) + "0300000002";        //zdr-1604: 与构造不同的是： iDevAddr可以由外部改变； 并且去掉了 AutoSend = false; 
                strCmd = strCmd + PubFunction.GetDataCRC(strCmd);
                fAutoSendBuffer = strCmd;

            }
        }

        private void ChangeDevAddrThread()
        {
            while (true)
            {
                if(DevAddr == devNum)
                {
                    DevAddr = 1;
                }
                else
                {
                    DevAddr++;
                }
                Thread.Sleep(AutoSendDelayTime);
            }
        }
        //zdr-1603: ----------------------------------接口实现如下---------------------------------------------

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //zdr-1603: 构造并且发送命令, strData为16进制的字符串
        private void BuildAndSendCMD(int iCmdNum, int iStartAddr, int iAddrLen , string strData = "")
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
        public void QueryVAPFOfDev()    //zdr-1603: 适合数显单相表
        {

            //zdr-1603: 单相表，比较简单，主要根据设备地址，来进行区别
            //zdr: 单相电压表：发送查询命令： 01 03 00 00 00 02 C4 0B,  回传命令： 01 03 04 43 65 80 00 9E 68
            //zdr: 单相电流表：发送查询命令： 01 03 00 00 00 02 C4 0B,  回传命令： 01 03 04 00 00 00 00 FA 33


            //zdr-1603:下面的命令码参考，tg007构造查询命令
            int CmdNum = 0X03;      //zdr-1603: 读取数据命令码
            int StartAddr = 0X00;    //zdr-1603:   0000
            int AddrLen   = 0X02;    //zdr-1603:    00x2

            BuildAndSendCMD(CmdNum, StartAddr, AddrLen);

        }


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
            if (ModbusDevResponseData.Length < 18) return;


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
            sVorAData = ModbusDevResponseData.Substring(6, 8);    //01 03 04 "43 65 80 00" 9E 68
            float fVorAValue = MathFloat(sVorAData);

            //zdr-1603: 状态回传函数调用, 其中：DevID, DevName为继承WTClientSocket中的属性变量
            if (OnVolAhEvent != null)
            {
                OnVolAhEvent(DevID, DevName, iDevAddr, fVorAValue);
            }

            //zdr-1603: 控制字符串大小
            if (fReceiveBuffer.Length >= 200) fReceiveBuffer.Substring(200, fReceiveBuffer.Length - 200);
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
