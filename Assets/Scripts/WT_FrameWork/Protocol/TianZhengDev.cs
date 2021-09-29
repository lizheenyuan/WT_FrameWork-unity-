/***********************************************随笔说明***********************************************
* Author:              ZhangDongRui (En: Keri)
* FileName:         TianZhengDev.cs 
* Description :     天正断路器设备控制
* Range of application:  C#平台下的“天正断路器设备”的开关
* 
* Comments:       1, 本脚本(TianZhengDev.cs)参考《TGM2E Modbus通信说明 Rev.D.xls》  和 网络资源编写
*                                2, 参考波特率配置如： 9600,E,8,1
*                                3, 本脚主要针对，天正-TGM2E断路器设备的开关分合闸操作，即查询开关状态的编码
* 
* 
*************** **************************参考示例Code说明*******************************************     
           CTianZhengDev TianZhengDev = new CTianZhengDev();
           TianZhengDev.DevAddr = 1;                                                       //zdr-1604: 天正断路器设备地址，
           TianZhengDev.DevID = 1;                                                            //zdr:设置ID号 , 由客户定义   
           TianZhengDev.DevName = "TianZhengDev";                    //zdr: 设置名字  , 由客户定义
           TianZhengDev.ServerIPAddress = "192.168.1.62";           //zdr-1603: 设备IP地址
           TianZhengDev.ServerPort = 4196;                                          //zdr-1603: 设备端口号
           TianZhengDev.AutoSendDelayTime = 1000;                            //zdr-1603: 设置自动查询命令的时间间隔，单位，毫秒；
           TianZhengDev.AutoSend = true;                                            //zdr-1603: 默认关闭; 需要开启自动查询功能 ,  则设置该变量为: true


           //zdr-1603: 注意这三个变量的设置关系，如果AutoConnect设置为真，那么即使CtrlConnect设置为false，自动重连也会设置CtrlConnect为：true；使设备连接
           TianZhengDev.AutoConnect = false;                                   //zdr-1603:  true，表示断线自动重连； 设置false, 则需要手动设置CtrlConnect，进行连接
           TianZhengDev.AutoConnectTime = 4;                                //zdr-1603:   默认值为：5秒检测断线重连的时间，间隔；
           TianZhengDev.CtrlConnect = true;                                      //zdr-1603: 当自动重连AutoConnect，设置为false，需要设置该值为真，使之连接设备

           TianZhengDev.OnSwitchStateEvent += new COnSwitchStateEvent(OnSwitchStateEvent);   //zdr-1603: 回调事件定义

           //zdr-1603: 向设备发送开关-分闸命令
           TianZhengDev.SwitchOpen();
           //zdr-1603: 向设备发送开关-合闸命令
           TianZhengDev.SwitchClose();
* 
* 
******************************************************************************************************/


using Assets.Scripts.Public;

//zdr-1603: 加载公共函数接口： pubFunction


namespace Assets.Scripts.WT_FrameWork.Protocol
{

    //zdr-1603: 开关投入状态：   0 开 1 合；     定义委托，具体实现过程，在调用TCH98Dev对象中，进行处理
    public delegate void COnSwitchStateEvent(int aDevID, string devName, int iSwitchState);

    public class CTianZhengDev : WTClientSocket
    {
        private int m_iDevAddr;
        private string fReceiveBuffer;   //zdr-1603: 接收-收到的字符串
        public COnSwitchStateEvent OnSwitchStateEvent = null;      //设备状态回传事件



          //zdr-1603: 默认构造函数
        public CTianZhengDev()
        {


            //zdr-1604: 设备自动查询命令写的不是很完美，漏洞在于：m_iDevAddr是可以变化的，
            //                     好的处理办法是：重新处理构造函数中的自动查询命令，把参数设置为属性类型，即改变iDevAddr时，fAutoSendBuffer也会随之改变

            m_iDevAddr = 1;  //zdr-1603: 设备默认地址，为：1           

            //zdr-1603: 设置自动查询命令  , 示例命令如下： 01 03 00 70 00 01 85 D1， 可以查询断路器开关的状态
            string strCmd = string.Format("{0:X2}", m_iDevAddr) + "0300700001";
            strCmd = strCmd + PubFunction.GetDataCRC(strCmd);
            fAutoSendBuffer = strCmd;
            AutoSend = false;   //zdr-1603: 默认关闭自动查询功能； 

        }


        //zdr-1603: 默认析构函数
        ~CTianZhengDev()
        {

        }


        //zdr-1604:  新增加自动查询命令，主要是改变构造函数中m_iDevAddr = 1;写死的状态
        //zdr-1604: 因为构造函数随着实例化而执行，解决办法有二：一个是：写个带参的构造函数，传递进去m_iDevAddr； 另外一种是：写一个关于  iDevAddr  的属性类型
        //zdr-1604: 前一种方法，写一个带参的构造函数，还不够灵活，如果在一个IP带动多台设备，且根据m_iDevAddr来区别不同设备的命令时，这个局限性就暴漏出来了
        //zdr-1604: 综上所述，另外再写一个  iDevAddr  的属性类型，来实现一个IP下的不同设备查询
        public int DevAddr
        {
            get { return m_iDevAddr; }
            set
            {
                m_iDevAddr = value;

                //zdr-1604: 设置自动查询命令  , 示例命令如下： 01 03 00 70 00 01 85 D1， 可以查询断路器开关的状态
                string strCmd = string.Format("{0:X2}", m_iDevAddr) + "0300700001";        //zdr-1604: 与构造不同的是： m_iDevAddr可以由外部改变； 并且去掉了 AutoSend = false; 
                strCmd = strCmd + PubFunction.GetDataCRC(strCmd);
                fAutoSendBuffer = strCmd;

            }
        }



        //zdr-1603: ----------------------------------接口实现如下---------------------------------------------

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //合闸: 55FFH,     分闸: FFAAH
        //zdr-1603: 发送开关，合，命令， 示例命令如：  01 06 00 80 55 FF F7 32
        public void SwitchClose()    //zdr-1603: 天正(TGM2E)断路器开关--合
        {
            //构造命令部分, 
            string strCMD = string.Format("{0:X2}", m_iDevAddr) + "06008055FF";

            strCMD = strCMD + PubFunction.GetDataCRC(strCMD);

            //zdr-1603: 发送命令道设备
            SendData(strCMD);

        }


        //zdr-1603: 发送开关，分，命令， 示例命令如：   01 06 00 80 FF AA 49 AD
        public void SwitchOpen()    //zdr-1603: 天正(TGM2E)断路器开关--分
        {
            //构造命令部分, 
            string strCMD = string.Format("{0:X2}", m_iDevAddr) + "060080FFAA";

            strCMD = strCMD + PubFunction.GetDataCRC(strCMD);

            //zdr-1603: 发送命令道设备
            SendData(strCMD);

        }

       
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //zdr-1603: 解析 "天正(TGM2E)断路器" 回传命令
        //zdr-1603:  例子，回传数据形如：01 03 02 00 00 B8 44
        //zdr-1603: 最后2字节位：CRC；  CRC前面的2字节，表示开关状态： 0断开,1闭合,0FFH未知
        protected override void ReSolveReceiverData(string strReceiveData)   
        {
            string TianZhengDevResponseData = "";
            string sCRCValue    =  "";
            string sSwichState =  "";
            int iSwitchValue     =  0xFF ;

            fReceiveBuffer += strReceiveData;

            //zdr-1603:　从最后面查找，可以保证数据是最新的
            int iPosition = fReceiveBuffer.LastIndexOf(string.Format("{0:X2}", m_iDevAddr) + "0302");     //zdr-1603: 索引从0开始，没有找到返回-1

            if (iPosition < 0)      //zdr-1603： 在此只处理回传命令中含有：0302的命令，其他命令格式不予处理
            {
                fReceiveBuffer = "";
                return;
            }

            TianZhengDevResponseData = fReceiveBuffer.Substring(iPosition, fReceiveBuffer.Length - iPosition);

            //zdr-1603: 接收数据不完整，直接返回
            if (TianZhengDevResponseData.Length < 14) return;


            //zdr-1603: ---------------------------> 开始解析：电压，电流
            //zdr-1603: 获取完整回传数据
            TianZhengDevResponseData = TianZhengDevResponseData.Substring(0, 14);   //  形如： TianZhengDevResponseData = 0103020000B844
            sCRCValue = PubFunction.GetDataCRC(TianZhengDevResponseData.Substring(0, 10));

            //zdr-1603:  接收字符串瘦身
            fReceiveBuffer = fReceiveBuffer.Substring(iPosition + 14, fReceiveBuffer.Length - iPosition - 14);

            if (sCRCValue != TianZhengDevResponseData.Substring(10, 4))  //zdr-1603: 校验和是否相等
            {
                return;      //zdr-1603: 接收数据有误，丢弃掉此次数据解析
            }

            //zdr-1603: 获取开关状态
            sSwichState = TianZhengDevResponseData.Substring(6, 4);    //010302  "0000" B844,  sSwichState = "0000"
            //zdr-1603:  16进制字串，转10进制
            iSwitchValue = int.Parse(sSwichState, System.Globalization.NumberStyles.AllowHexSpecifier);


            //zdr-1603: 状态回传函数调用, 其中：DevID, DevName为继承WTClientSocket中的属性变量
            if (OnSwitchStateEvent != null)
            {
                OnSwitchStateEvent(DevID, DevName, iSwitchValue);
            }

            //zdr-1603: 控制字符串大小
            if (fReceiveBuffer.Length >= 200) fReceiveBuffer.Substring(200, fReceiveBuffer.Length - 200);
        }


        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------


    }
}
