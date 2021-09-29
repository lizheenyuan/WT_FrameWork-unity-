/***********************************************随笔说明***********************************************
 * Author:              ZhangDongRui (En: Keri)
 * FileName:         CH98Net.cs 
 * Description :     CH98设备控制
 * Range of application:  C#平台下的ch98网口通信
 * 
 * Comments:       本脚本(CH98Net.cs)参考delphi中"CH98Ctrl"单元模块， 和网络资源编写
 * 
 * 
 * 
 * 
 *************** **************************参考示例Code说明*******************************************
            TCH98Dev ch98 = new TCH98Dev();
           ch98.DevID = 1;                                                       //zdr:设置ID号
           ch98.DevName = "ch98-1";                                //zdr: 设置名字 
           ch98.ServerIPAddress = "192.168.1.62";       //zdr-1603: CH98设备IP地址
           ch98.ServerPort = 60000;                                    //zdr-1603: CH98设备端口号            
           ch98.AutoSendDelayTime =2;                         //zdr-1603: 设置自动查询命令的时间间隔，单位，秒；
           ch98.AutoSend = true;                                        //zdr-1603: 默认开启自动向设备发送命令功能      

           //zdr-1603: 注意这三个变量的设置关系，如果AutoConnect设置为真，那么即使CtrlConnect设置为false，自动重连也会设置CtrlConnect为：true；使设备连接
           //ch98.AutoConnect = false;                           //zdr-1603:  默认值为：true，表示断线自动重连； 设置false, 则需要手动设置CtrlConnect，进行连接
           ch98.AutoConnectTime = 4;                            //zdr-1603:   默认值为：5秒, 检测断线重连的时间，间隔；
           ch98.CtrlConnect = true;                                  //zdr-1603: 当自动重连AutoConnect，设置为false，需要设置该值为真，使之连接设备
   
           ch98.OnDevStateEvent += new TDevStateEvent(fDevStateEvent);
            
           ch98.Reset();                                                        //zdr-1603: CH98设备重置
           //zdr-1603: 添加和删除故障测试
           ch98.AddFault("K2+K5+K30+");
           ch98.SetFault();                                                  //zdr-1603: 添加完之后，重新设置故障，使之生效
           ch98.RemoveFault("K5+");
           ch98.SetFault();                                                  //zdr-1603: 去除之后，重新设置故障，使之生效
 * 
 * 
 ******************************************************************************************************/
using System;


using System.Collections;  //zdr-1511: 数组列表的命名空间
using System.Threading;
using Assets.Scripts.Public;
//zdr-1512: 线程所在命名空间
using Assets.Scripts.WT_FrameWork.Protocol;

//zdr-1603: 加载公共函数接口： pubFunction


namespace Assets.Scripts.WT_FrameWork.Protocol     //zdr-11603: 命名空间严格来说，需要定义的有层次感，即. 模块结构感； 根据移植程序需要，可以改变命名空间的结构
{

    //zdr-1603: 开关投入状态：   0 开 1 合；     定义委托，具体实现过程，在调用TCH98Dev对象中，进行处理
    public delegate void TDevStateEvent(int aDevID, string devName, bool In1State, bool In2State, bool In3State, bool In4State,
                                                                                                                                            bool In5State, bool In6State, bool In7State, bool In8State, bool InputChanged);


    public class TCH98Dev : WTClientSocket
    {

        //zdr-1603: 定义属性字段
       // private string fFaultList;       //故障列表形如：K1+表示
        //private string fReceiveData1, fReceiveData2;//收到数据 二次相同才认为数据正确
        private byte fInputState; //输入状态

        private string fReceiveBuffer;   //zdr-1603: 接收-收到的字符串

        public TDevStateEvent OnDevStateEvent = null;      //设备状态回传事件
        ArrayList FaultList = new ArrayList();    //zdr-1603:　默认大小为16， 动态增长， 当然初始化时，也可以进行大小的指定，如： new ArrayList(10); 

       
        //zdr-1603: 默认构造函数
        public TCH98Dev()
        {
            //zdr-1603: 初始化相关变量值
            FaultList.Clear();

            //zdr-1603: 设置CH98查询命令字符串
            string strCmd = "01" + "0400";
            strCmd = strCmd + PubFunction.GetDataCRC(strCmd);
            fAutoSendBuffer = strCmd;
            AutoSend = false;   //zdr-1603: 自动查询命令是关闭的，需要手动开启           

        }


        //zdr-1603: 析构函数
        ~TCH98Dev()
        {
            
            //zdr-1603: 释放对象资源

        }

        //zdr-1603: ----------------------------------接口实现如下-------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //zdr-1603: 处理接收数据接口
        protected override void ReSolveReceiverData(string strReceiveData)    //zdr-1603:  完整的回传数据形如： 010501569077， 共计12位
        {
            string strData;
            string sCh98ResponseData = "";
            string sCRCValue = "";

            fReceiveBuffer += strReceiveData;

            //zdr-1603:　从最后面查找，可以保证数据是最新的
            int iPosition = fReceiveBuffer.LastIndexOf("0501");     //zdr-1603: 索引从0开始，没有找到返回-1

            if (iPosition < 0)
            {
                fReceiveBuffer = "";
                return;
            }

            iPosition = iPosition - 2;   //zdr: 因为“0501”，前面还有一字节的ID号，所以位置还要前移2位

            strData = fReceiveBuffer.Substring(iPosition , fReceiveBuffer.Length - iPosition);
            

            if (strData.Length >= 12)
            {
                //zdr-1603: 获取完整回传数据
                sCh98ResponseData = strData.Substring(0, 12);

                sCRCValue = PubFunction.GetDataCRC(sCh98ResponseData.Substring(0,8));

                fReceiveBuffer = fReceiveBuffer.Substring(iPosition   + 12,  fReceiveBuffer.Length - iPosition  - 12);

                if (sCRCValue != sCh98ResponseData.Substring(8, 4))  //zdr-1603: 比较校验和是否相等
                {                    
                    return;
                }

                //zdr-1603: 解析获取到的回传数据
                InterpretData(sCh98ResponseData);//传递 完整字符数据传

            }


            //zdr-1603: 控制字符串大小
            if (fReceiveBuffer.Length >= 400) fReceiveBuffer.Substring(400, fReceiveBuffer.Length - 400);


        }


        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //zdr-1603: 解析获取98的回传数据
        private void InterpretData(string sResponseData)
        {
            bool temp0,temp1,temp2,temp3,temp4,temp5,temp6,temp7,inputState;

            string sData = sResponseData.Substring(6,2);

            //zdr-1603:  16进制字串，转10进制
            int iData = Convert.ToInt32(sData, 16);//zdr-1603: 也可以用下面的转换代替此句
            //int iData = int.Parse(sData, System.Globalization.NumberStyles.AllowHexSpecifier);

            string sBin = Convert.ToString(iData, 2);

            string strTemp = "00000000";
            int iLength = 8 - sBin.Length;
            sBin = strTemp.Substring(0, iLength) + sBin;    

            if (sBin.Substring(7, 1) == "1") temp0 = true; else temp0 = false;
            if (sBin.Substring(6, 1) == "1") temp1 = true; else temp1 = false;
            if (sBin.Substring(5, 1) == "1") temp2 = true; else temp2 = false;
            if (sBin.Substring(4, 1) == "1") temp3 = true; else temp3 = false;
            if (sBin.Substring(3, 1) == "1") temp4 = true; else temp4 = false;
            if (sBin.Substring(2, 1) == "1") temp5 = true; else temp5 = false;
            if (sBin.Substring(1, 1) == "1") temp6 = true; else temp6 = false;
            if (sBin.Substring(0, 1) == "1") temp7 = true; else temp7 = false;

            //zdr-1603: 输入状态监测
            if (fInputState == (byte)iData) inputState = false; else inputState = true;

            fInputState = (byte)iData;

            //zdr-1603: 状态回传函数调用, 其中：DevID, DevName为继承WTClientSocket中的属性变量
            if (OnDevStateEvent != null)
            {
                OnDevStateEvent(DevID, DevName, temp0, temp1, temp2, temp3, temp4, temp5, temp6, temp7, inputState);
            }
            
           
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //zdr-1603: 查询功能命令，交给WTClientSocket来进行处理，在此仅仅赋值fAutoSendBuffer查询字符串
        private void QueryState()      //zdr-1603: 此接 暂时作为保留，权限设置：private没有给任何地方使用，
        {

            string strCmd = "01" + "0400";
            strCmd = strCmd + PubFunction.GetDataCRC(strCmd);
            fAutoSendBuffer = strCmd;
    
        }


        //zdr-1603: CH98设备重置
        public void Reset()
        {
            //fFaultList = "";
            FaultList.Clear();

            string strCmd = "01" + "0000";
            strCmd = strCmd + PubFunction.GetDataCRC(strCmd);

            SendData(strCmd);

        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //zdr-1603: CH98设置故障
        public void SetFault()
        {
            string  strCmd = "01" + "0106";
            string M1Vaulue, M2Vaulue, M3Vaulue;

            M1Vaulue = "";
            M2Vaulue = "";
            M3Vaulue = "";
            
            //zdr-1603: 定义开关
            string[] sFaultPoint_1 = new String[] { "K1+", "K2+", "K3+", "K4+", "K5+", "K6+", "K7+", "K8+", "K9+", "K10+", "K11+", "K12+", "K13+", "K14+", "K15+", "K16+" };
            string[] sFaultPoint_2 = new String[] { "K17+", "K18+", "K19+", "K20+", "K21+", "K22+", "K23+", "K24+", "K25+", "K26+", "K27+", "K28+", "K29+", "K30+", "K31+", "K32+" };
            string[] sFaultPoint_3 = new String[] { "K33+", "K34+", "K35+", "K36+", "K37+", "K38+", "K39+", "K40+", "K41+", "K42+", "K43+", "K44+", "K45+", "K46+", "K47+", "K48+" };


            //zdr-1603: 存储开关状态变量值
            for (int i1= 0; i1 < sFaultPoint_1.Length; i1++)
                if (FaultList.Contains(sFaultPoint_1[i1])) M1Vaulue = "1" + M1Vaulue; else M1Vaulue = "0" + M1Vaulue;

            for (int i2 = 0; i2 < sFaultPoint_2.Length; i2++)
                if (FaultList.Contains(sFaultPoint_2[i2])) M2Vaulue = "1" + M2Vaulue; else M2Vaulue = "0" + M2Vaulue;

            for (int i3 = 0; i3 < sFaultPoint_3.Length; i3++)
                if (FaultList.Contains(sFaultPoint_3[i3])) M3Vaulue = "1" + M3Vaulue; else M3Vaulue = "0" + M3Vaulue;


            //zdr-1603: M1Vaulue, M2Vaulue, M3Vaulue的值形如：0100101000001100
            //zdr-1603: 转换M1Vaulue，M2Vaulue，M3Vaulue为4位16进制形式，并且转换后的字符串相加
            string strData = string.Format("{0:X4}", Convert.ToInt32(M1Vaulue, 2))  +  string.Format("{0:X4}", Convert.ToInt32(M2Vaulue, 2))  +  string.Format("{0:X4}", Convert.ToInt32(M3Vaulue, 2));

            strCmd += strData;


            strCmd = strCmd + PubFunction.GetDataCRC(strCmd);
            SendData(strCmd);

        }


        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //zdr-1603: 在设备中，删除参数指定的故障列表； 返回值表示，删除故障列表的个数
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



        ///----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //zdr-1603: 根据参数中指定的故障，在设置故障列表中查找，如果找到则返回索引下标，否则返回-1
        //zdr-1603: 当故障点出现："K4+K5+"故障时，原delphi函数的处理变得无意义，而且该函数用户不大，几乎不用； 在此只做单点故障的查找处理
        public int  FindFault(string strFault)   //zdr-1603: 参数形如："K1+", "K2+"......, 注意大小写的区别
        {
            return FaultList.IndexOf(strFault);  //zdr-1603: 查找不到，返回-1； 找到返回下标索引； 注： 且，下标索引是从0开始的，
            //return -1;
        }


        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //zdr-1603: 把参数中，指定的故障列表，增加到设备故障列表中

       public void AddFault(string StrFault)
        {
           string sTemp = StrFault ;

           //zdr-1603: 在此利用字符串的替换分割，达到去除每个故障的目的，与：原delphi中AddFault的实现异曲同工
           sTemp = sTemp.Replace("+", "+,");
           string[] sTemp2 = sTemp.Split(',');

           foreach (string i in sTemp2) 
               if (i != "") FaultList.Add(i);

        }

     

    }

}
