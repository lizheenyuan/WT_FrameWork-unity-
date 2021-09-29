/******************************************************************************************
* Name：C# UDP客户端通信协议
* Author: LSG
* CreateTime: 2016-04-13
* 
* ****************************************************************************************/
using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Assets.Scripts.Protocol
{
    ////flagRS 0:接收数据  1:发送数据
    //public delegate void OnDevReadSendEvent(int devID, string devName, int flagRS, string strRSData);
    public class UdpState
    {
        public UdpClient udpClient;
        public IPEndPoint ipEndPoint;
        private const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public int counter = 0;
    }
    class WTUDPClient
    {
        #region 私有变量
        private int localPort = 1100; 
        private string remoteIP = "127.0.0.1";
        private int remotePort = 1101;
        private bool ctrlConnect;
        private int dataSendType = 0;  //数据发送接收格式 0: 16进制发送    1：字符串发送

        private UdpClient udpSend;
        private UdpClient udpReceive;

        private UdpState udpSendState = null;
        private UdpState udpReceiveState = null;

        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);

        private Thread receiveThread;

        private int autoConnectTime = 10;     //检测断线重连的时间间隔， 默认10秒
        private System.Timers.Timer autoConnectTimer;    

        #endregion

        protected string HeartBeatRequestMessage;
        protected string HeartBeatResponseMessage;

        #region 对外属性
        public int DevID { get; set; }
        public string DevName { get; set; }
        public int LocalPort
        {
            get { return localPort; }
            set { localPort = value; }
        }
        public string RemoteIP
        {
            get { return remoteIP; }
            set { remoteIP = value; }
        }
        public int RemotePort
        {
            get { return remotePort; }
            set { remotePort = value; }
        }
        public bool CtrlConnect
        {
            get { return ctrlConnect; }
            set
            {
                if(!ctrlConnect && value)
                {
                    Connect();
                }
                if (!value)
                {
                    DisConnect();
                }
            }
        }
        public int DataSendType
        {
            get { return dataSendType; }
            set { dataSendType = value; }
        }
        public bool AutoConnect { get; set; }
        public int AutoCennectTime
        {
            get { return autoConnectTime; }
            set { autoConnectTime = value; }
        }
        public event OnDevReadSendEvent onDevReadSendEvent;
        #endregion

        public WTUDPClient()
        {
            autoConnectTimer = new System.Timers.Timer(autoConnectTime * 1000);   //实例化Timer类，设置间隔时间   
            autoConnectTimer.Elapsed += new System.Timers.ElapsedEventHandler(AutoConnectEvent); //到达时间的时候执行自动重连事件；   
            autoConnectTimer.AutoReset = true;   //设置是执行一次（false）还是一直执行(true)；   
            autoConnectTimer.Enabled = true;     //是否执行System.Timers.Timer.Elapsed事件；   
        }

        protected virtual void ReSolveReceiverData(string strReceiveData) { } //解析收到数据，虚方法，子类实现 

        private void AutoConnectEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!CtrlConnect)
            {
                CtrlConnect = true;
            }

        }
        private void Connect()
        {
            try
            {
                IPEndPoint ipepLocal = new IPEndPoint(IPAddress.Any, localPort);
                IPEndPoint ipepRemote = new IPEndPoint(IPAddress.Parse(remoteIP).Address, remotePort);
                udpSend = new UdpClient();
                udpReceive = new UdpClient(ipepLocal);

                udpSendState = new UdpState();
                udpSendState.udpClient = udpSend;
                udpSendState.ipEndPoint = ipepRemote;

                udpReceiveState = new UdpState();
                udpReceiveState.udpClient = udpReceive;
                udpReceiveState.ipEndPoint = ipepLocal;
                ctrlConnect = true;
                //接收线程
                if(receiveThread == null)
                {
                    receiveThread = new Thread(new ThreadStart(ReceiveMsg));
                }
                receiveThread.Start(); 
            }
            catch(Exception e)
            {
                throw new Exception(DevName + "创建UDP连接时发生错误，可能是如下原因：" + e.Message);
            }

        }

        private void DisConnect()
        {
            try
            {
                if(udpSend != null)
                {
                    udpSend.Close();
                    udpSend = null;
                }
                if(udpReceive != null)
                {
                    udpReceive.Close();
                    udpReceive = null;
                }
                if(udpSendState != null)
                {
                    udpSendState = null;
                }
                if(udpReceiveState != null)
                {
                    udpReceiveState = null;
                }
                ctrlConnect = false;
            }
            catch (Exception e)
            {
                ctrlConnect = false;
                throw new Exception(DevName + "断开UDP连接时发生错误，可能是如下原因：" + e.Message);
            }
        }

        private void ReceiveMsg()
        {
            while (ctrlConnect)
            {
                lock (this)
                {
                    IAsyncResult iar = udpReceive.BeginReceive(new AsyncCallback(ReceiveCallback), udpReceiveState);
                    receiveDone.WaitOne();
                    Thread.Sleep(100);
                }
            }
        }

        private void ReceiveCallback(IAsyncResult iar)
        {
            UdpState udpReceiveState = iar.AsyncState as UdpState;
            if (iar.IsCompleted)
            {
                Byte[] receiveBytes = udpReceiveState.udpClient.EndReceive(iar, ref udpReceiveState.ipEndPoint);
                string receiveString = "";
                if (dataSendType == 0)
                {
                    StringBuilder sbTemp = new StringBuilder();
                    for(int i = 0; i < receiveBytes.Length; i++)
                    {
                        sbTemp.Append(receiveBytes[i].ToString("X2"));
                    }
                    receiveString = sbTemp.ToString();
                }
                else 
                {
                    receiveString = Encoding.ASCII.GetString(receiveBytes);
                }                
                receiveDone.Set();                
                if (onDevReadSendEvent != null)
                {
                    onDevReadSendEvent(DevID,DevName,0,receiveString);
                }
                //子类解析数据
                ReSolveReceiverData(receiveString);
                //暂未处理心跳数据粘包问题
                if (HeartBeatResponseMessage != string.Empty && HeartBeatRequestMessage == receiveString)
                {
                    SendHeartBeat();
                }                
            }
        }

        private void SendHeartBeat()
        {
            if(!ctrlConnect) { return; }
            udpSend.Connect(udpSendState.ipEndPoint);
            udpSendState.udpClient = udpSend;
            Byte[] sendBytes;
            if(dataSendType == 0)
            {
                sendBytes = StrToToHexByte(HeartBeatResponseMessage);
            }
            else
            {
                sendBytes = Encoding.Default.GetBytes(HeartBeatResponseMessage);
            }
            udpSend.BeginSend(sendBytes, sendBytes.Length, new AsyncCallback(SendCallback), udpSendState);
            sendDone.WaitOne();

            if (onDevReadSendEvent != null)
            {
                onDevReadSendEvent(DevID, DevName, 1, HeartBeatResponseMessage);
            }
        }

        public void SendData(string sendData)
        {
            if (!ctrlConnect) { return; }
            udpSend.Connect(udpSendState.ipEndPoint);
            udpSendState.udpClient = udpSend;
            Byte[] sendBytes;
            if (dataSendType == 0)
            {
                sendBytes = StrToToHexByte(sendData);
            }
            else
            {
                sendBytes = Encoding.Default.GetBytes(sendData);
            }
            udpSend.BeginSend(sendBytes, sendBytes.Length, new AsyncCallback(SendCallback), udpSendState);
            sendDone.WaitOne();
            if (onDevReadSendEvent != null)
            {
                onDevReadSendEvent(DevID, DevName, 1, sendData);
            }
        }
        

        /*public void SendByte(byte[] sendData)
        {
            udpSend.Connect(udpSendState.ipEndPoint);
            udpSendState.udpClient = udpSend;
            udpSend.BeginSend(sendData, sendData.Length, new AsyncCallback(SendCallback), udpSendState);
            sendDone.WaitOne();
        }*/

        private void SendCallback(IAsyncResult iar)
        {
            UdpState udpState = iar.AsyncState as UdpState;
            udpState.udpClient.EndSend(iar);
            sendDone.Set();
        }

        //字符串转16进制数组
        private static byte[] StrToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

    }
}
