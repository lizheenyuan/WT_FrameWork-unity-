/******************************************************************************************
 * Name：C# Socket客户端通信协议
 * Author: LSG
 * CreateTime: 2015-09-07
 * {断线重新连接问题未处理，检测连接状态未处理，C#自带的状态有问题}   --> 已解决
 * *****************************************修改历史***************************************
 * zdr-1511:增加析构函数，进行sokcet资源的释放.(调用：shotdown,close)
 *          删除构造函数
 * 
 * zdr-1511: 修改断线重新连接问题,                   
 *          1, SendData
 *          修改发送数据前的连接的判断,实时更新连接状态，这样避免在CtrlConnect时，出现错误(如：明知连接失败，但就是设置连接不成功)
 *          在异常处理中增加disconnect进行断开连接,增加fCtrlConnect=false
 *          
 *          2，设置CtrlConnect处理
 *          调用connet前，调用socket,即重新建立Socket,增加对异常语句的处理 
 *           
 * zdr-1603: 处理自动查询，增加断线重连功能
 * 
 * zdr-1511: 连接状态监测问题
 *          1，读取CtrlConnect状态处理
 *          主要办法是：通过发送数据得到当前连接状态 
 *          
 * LSG-20160530：程序优化
 * 1、自动发送间隔改为以毫秒为单位
 * 2、去除属性AutoConnectTime,改为阶梯性的连接，初始连接0.5秒，连接成功后，变为10秒检测一次，
 *    连接失败后，每次连接时间递增1500毫秒，直到10秒
 * LSG-20160803：处理物理连接断线问题
 * 1、处理物理连接断线问题
 * 2、处理第一次设备连接不成功、后续无法正常连接问题
 ******************************************************************************************/

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
using UnityEngine;

//zdr-1603:  若需要启用此语句，则需要把所有(共计6处) ：  Console.WriteLine    替换为：Debug.Log， 以便在unity控制台中查看

//using Interface3D.Log;

namespace Assets.Scripts.WT_FrameWork.Protocol
{
    //flagRS 0:接收数据  1:发送数据
    public delegate void OnDevReadSendEvent(int devID, string devName, int flagRS, string strRSData);

    public class WTClientSocket
    {
        private static Exception socketexception;
        private readonly System.Timers.Timer autoConnectTimer; //zdr-1601: 定义自动连接定时器
        private Thread autoSendThread;


        //zdr: 设备断线重连问题
        private bool bAutoConnect = true; //断线重连，默认为：真
        private readonly int conectTestInterval = 500; //lzy-1903连接测试时间间隔 最大999ms
        public readonly System.Timers.Timer connectTestTimer; //lzy-1903: 心跳检测计时器，用于有回传的设备
        private bool isFirstConnect;
        private long connectTimes;
        protected DateTime dt_lastRecMsgTime; //最后一次收到消息的时间
        protected string fAutoSendBuffer = "";

        private int fAutoSendDelayTime = 150; //自动发送延时时间，单位为秒 默认为0.1秒
        private Socket fClientSocket; //Socket
        private bool fCtrlConnect; //Socket连接

        private string fReceiveBuffer; //接收字符串
        private readonly int iAutoConnectTime = 1500; //检测断线重连的时间间隔， 初始为500毫秒，最大为10000毫秒
        private bool isConnectionSuccessful;
        public Action<bool> OnConnectStateChanged;


        private readonly byte[] receiveBuffer = new byte[1024]; //接收缓冲区
        private readonly ManualResetEvent timeoutObject = new ManualResetEvent(false);

        //zdr-1603: 默认构造函数, 
        public WTClientSocket()
        {
            //zdr-1603: 增加定时器； 在指定的时间内，定时检测设备是否断线，若断线开始自动重连
            //zdr-1601:  创建定时器，根据设置时间，定时检测并重连； iAutoConnectTime默认值为5秒
            autoConnectTimer = new System.Timers.Timer(iAutoConnectTime); //实例化Timer类，设置间隔时间   
            autoConnectTimer.Elapsed += AutoConnectEvent; //到达时间的时候执行自动重连事件；   
            autoConnectTimer.AutoReset = true; //设置是执行一次（false）还是一直执行(true)；   
            autoConnectTimer.Enabled = true; //是否执行System.Timers.Timer.Elapsed事件； 

            connectTestTimer = new System.Timers.Timer(conectTestInterval);
            connectTestTimer.Elapsed += ConnectTestTimer_Elapsed;
            connectTestTimer.AutoReset = true;
            connectTestTimer.Enabled = UseConectTest;
            isFirstConnect = true;
        }

        public int DevID { get; set; }

        public string DevName { get; set; }

        public bool AutoSend { get; set; }

        public bool AutoConnect
        {
            get { return bAutoConnect; }
            set
            {
                bAutoConnect = value;
                if (!value)
                {
                    autoConnectTimer.Enabled = false;
                }
                else
                {
                    autoConnectTimer.Interval = 500;
                    autoConnectTimer.Enabled = true;
                }
            }
        }


        public string ServerIPAddress { get; set; }

        public int ServerPort { get; set; }

        public bool CtrlConnect
        {
            get
            {
                //zdr-1511: 修改Code,实时反馈Socket连接状态
                var blockingState = false;

                //zdr-1511: 判断连接前，首先判断，Socket是否存在，如果不存在，直接返回false
                if (fClientSocket == null)
                {
                    fCtrlConnect = false;
                    return false;
                }

                try
                {
                    blockingState = fClientSocket.Blocking;

                    //zdr-1511: 确定连接的当前状态，请进行非阻止、零字节的 Send 调用
                    var tmp = new byte[1];
                    fClientSocket.Blocking = false;

                    //zdr-1511:如果该调用成功返回或引发 WAEWOULDBLOCK 错误代码 (10035)，则该套接字仍然处于连接状态；
                    //        :否则，该套接字不再处于连接状态
                    var i = fClientSocket.Send(tmp, 0, 0);
                }
                catch (SocketException e)
                {
                    //zdr-1511: 只是显示连接是否成功，不用处理
                    // 10035 == WSAEWOULDBLOCK
                    if (e.NativeErrorCode.Equals(10035))
                    {
                        Debug.Log("Still Connected, but the Send would block");
                    }
                    else
                    {
                        if (fCtrlConnect) CtrlConnect = false;
                        Debug.Log("Disconnected"); //zdr-1603:  去掉这个显示
                        return fCtrlConnect;
                    }

                    //zdr-1511: 下面这个语句必须放在这个Socket异常中，进行处理，
                    try
                    {
                        fClientSocket.Blocking = blockingState;
                    }
                    catch
                    {
                        if (fCtrlConnect) CtrlConnect = false;
                    }
                }
                catch (ObjectDisposedException e)
                {
                    if (fCtrlConnect) CtrlConnect = false;
                }
                catch (Exception e)
                {
                    //zdr-1511: 当有fClientSocket.Blocking异常时，跳转到该处理地方，
                    //        : 不对fClientSocket.Blocking进行处理，继续操作
                    e.ToString(); //zdr-1511: 这句话其实没有用，但这句话的副作用是可以，去掉“e没有使用的警告”
                }

                //zdr-1511: 获取连接状态
                try
                {
                    fCtrlConnect = fClientSocket.Connected;
                }
                catch
                {
                    if (fCtrlConnect) CtrlConnect = false;
                }


                //zdr-1511:重新读取连接状态，理论上为上次的状态值，但由于刚刚发送数据，所以可做实时状态监测
                //fCtrlConnect = fClientSocket.Connected;
                return fCtrlConnect;
            }
            set
            {
                connectTestTimer.Enabled = value;
                if (fCtrlConnect != value)
                {
                   
                    if (value)
                    {
                        var cur = DateTime.Now;
                        IPAddress serverIP;
                        IPEndPoint ipe;
                        try
                        {
                            //zdr-1603: 判断IP是否为： 空
                            if (ServerIPAddress == null)
                            {
                                fCtrlConnect = false;
                                return;
                            }

                            fClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                            fClientSocket.Blocking = true;

                            fClientSocket.SendTimeout = 1000;
                            serverIP = IPAddress.Parse(ServerIPAddress);
                            ipe = new IPEndPoint(serverIP, ServerPort);

                            fClientSocket.Connect(ipe); //zdr-1511: 如果连接出错，则在异常处理(即：Catch中)进行关闭Socket

                            fCtrlConnect = true;
                            fClientSocket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None,
                                ReceiveData, fClientSocket);
                            //单独线程自动查询设备状态
                            autoSendThread = new Thread(AutoQueryState);
                            autoSendThread.IsBackground = true;
                            autoSendThread.Start();
                            dt_lastRecMsgTime = DateTime.Now;
                            //if (OnConnectStateChanged != null) OnConnectStateChanged(true);
                        }
                        catch (SocketException e)
                        {
                            Debug.Log(DevName + " ip:" + ServerIPAddress + e.Message);
                            if (fCtrlConnect)
                                CtrlConnect = false;
                            else
                                fClientSocket.Close();
                            //fClientSocket.Shutdown(SocketShutdown.Both);  //zdr-1512: 注销掉此语句                        
                            //zdr-1511: 这两句是关闭socket之意
                            //throw new Exception("Error 10001：" + e.Message);
                        }
                        catch (ObjectDisposedException e)
                        {
                            Debug.Log(e.Message);
                            if (fCtrlConnect) CtrlConnect = false;
                            fClientSocket.Close();
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e.Message);
                            if (fCtrlConnect) CtrlConnect = false;
                        }
                    }
                    else
                    {
                        try
                        {
                            fCtrlConnect = false; //zdr-1511: 添加false
                            if (autoSendThread != null && CtrlConnect == false) autoSendThread.Abort();
                            fClientSocket.Shutdown(SocketShutdown.Both);
                            fClientSocket.Close(); //zdr-1511: 添加close   
                            fClientSocket = null;
                            //if (OnConnectStateChanged != null) OnConnectStateChanged(false);
                        }
                        catch (Exception ex)
                        {
                            //throw new Exception("Error 10002：" + e.Message);
                            Debug.Log(ex.Message);
                        }
                    }
                }
            }
        }

        public int DataSendType { get; set; } = 0;

        public int AutoSendDelayTime
        {
            get { return fAutoSendDelayTime; }
            set
            {
                //if (fCtrlConnect)
                //{
                //    fCtrlConnect = !fCtrlConnect;
                //}
                //lzy 对最小查询时间做出限制
                if (value <= 150)
                {
                    fAutoSendDelayTime = 150;
                    return;
                }

                fAutoSendDelayTime = value;
            }
        }

        public bool UseConectTest { get; set; } = true;

        protected virtual void ReSolveReceiverData(string strReceiveData)
        {
        } //解析收到数据，虚方法，子类实现 

        public event OnDevReadSendEvent onDevReadSendEvent; //发送、接收内容事件，用于记录发送、接收内容

        private void ConnectTestTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!UseConectTest)
            {
                OnConnectStateChanged?.Invoke(CtrlConnect);
                connectTestTimer.Enabled = UseConectTest;
                return;
            }
            //var dt_Now = DateTime.Now;

            if (CtrlConnect)
            {
                var t1 = new TimeSpan(DateTime.Now.Ticks);
                var t2 = new TimeSpan(dt_lastRecMsgTime.Ticks);
                //Debug.Log("Timespan:" + t1.Subtract(t2).Milliseconds + "  TNow:" + DateTime.Now.ToString("O")+ "   Tlast:" + dt_lastRecMsgTime.ToString("O") + " " + "  dt:"+ conectTestInterval);
                //两个监视周期没有得到消息表示没有连接
                if (t1.Subtract(t2).Seconds >= conectTestInterval * 2/1000f)
                {
                    OnConnectStateChanged?.Invoke(false);
                    SendData(fAutoSendBuffer);
                    //CtrlConnect = false;
                    //dt_lastRecMsgTime = DateTime.Now;
                }
                else if (t1.Subtract(t2).Seconds >= conectTestInterval/1000f)
                {
                    SendData(fAutoSendBuffer);
                    OnConnectStateChanged?.Invoke(true);
                    //Debug.Log(2);
                }
                else if (t1.Subtract(t2).Seconds < conectTestInterval/1000f)
                {
                    OnConnectStateChanged?.Invoke(true);
                    //Debug.Log(3);
                }
            }
            else
            {
                OnConnectStateChanged?.Invoke(false);
            }
           
           
        }

        //zdr-1603: 实现设备断线自动重连功能
        private void AutoConnectEvent(object sender, ElapsedEventArgs e)
        {
            if (!CtrlConnect)
            {
                connectTimes++;
                CtrlConnect = true;
            }
        }

        //zdr-1511: 析构函数，主要是关闭socket连接，其余变量为托管资源，由系统自己回收
        ~WTClientSocket()
        {
            try
            {
                //zdr-1511:断开连接，
                fClientSocket.Shutdown(SocketShutdown.Both);
                fClientSocket.Close();
                UseConectTest = false;
                if (connectTestTimer!=null)
                {
                    connectTestTimer.Enabled = false;
                }

                autoSendThread?.Abort();
                //zdr-1511:释放空间, 查资料显示:C#和Java一样，new内存后，系统自动回收资源，不需要delete      
            }
            catch (ObjectDisposedException e)
            {
                e.ToString(); //zdr-1511: 这句话其实没有用，但这句话的副作用是可以，去掉“e没有使用的警告”
            }
            catch (NullReferenceException e)
            {
                e.ToString(); //zdr-1511: 这句话其实没有用，但这句话的副作用是可以，去掉“e没有使用的警告”
            }
            catch (SocketException e)
            {
                e.ToString(); //zdr-1511: 这句话其实没有用，但这句话的副作用是可以，去掉“e没有使用的警告”
            }
        }

        //发送数据
        public void SendData(string sendStr)
        {
            var bOrgAutoSendState = true;

            //zdr-1511: 发送数据前，判断数据是否连接
            if (CtrlConnect)
                try
                {
                    bOrgAutoSendState = AutoSend; //zdr-1603: 保存原来的发送状态

                    AutoSend = false; //停止自动发送,

                    byte[] sendBuf;
                    if (DataSendType == 0) //16进制发送
                        sendBuf = StrToToHexByte(sendStr);
                    else //字符串发送
                        sendBuf = Encoding.Default.GetBytes(sendStr);
                    fClientSocket.Send(sendBuf);
                    if (onDevReadSendEvent != null)
                        onDevReadSendEvent(DevID, DevName, 1, sendStr);

                    AutoSend = bOrgAutoSendState; //zdr-1603: 数据发送完毕，恢复原来自动发送状态
                }
                catch (SocketException e)
                {
                    // 10035 == WSAEWOULDBLOCK
                    if (e.NativeErrorCode.Equals(10035))
                    {
                        //Console.WriteLine("Still Connected, but the Send would block");                    
                    }

                    AutoSend = bOrgAutoSendState; //恢复原来自动发送状态
                    Console.WriteLine(e.Message);
                    Debug.Log(e.Message);
                    //zdr-1511: 如果发送有异常，做一下处理，断开连接，设置connect为false
                    fClientSocket.Close();
                    CtrlConnect = false;
                }
        }

        public void ReceiveData(IAsyncResult ar)
        {
            fReceiveBuffer = "";
            var cskTemp = ar.AsyncState as Socket;
            var sbTemp = new StringBuilder();
            try
            {
                var length = cskTemp.EndReceive(ar);

                if (DataSendType == 0) //16进制接收
                {
                    for (var i = 0; i < length; i++) sbTemp.Append(receiveBuffer[i].ToString("X2"));
                    fReceiveBuffer = sbTemp.ToString();
                }
                else //字符串接收
                {
                    fReceiveBuffer = Encoding.Default.GetString(receiveBuffer, 0, length);
                }

                if (fReceiveBuffer != "")
                {
                    if (onDevReadSendEvent != null) onDevReadSendEvent(DevID, DevName, 0, fReceiveBuffer);
                    dt_lastRecMsgTime = DateTime.Now;
                    ReSolveReceiverData(fReceiveBuffer);
                    //Debug.Log(DevName + "计时重置:   " + fReceiveBuffer);
                }
                fClientSocket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ReceiveData,
                    cskTemp);
               
            }
            catch (SocketException e)
            {
                //zdr-1511: 发现接收数据，socket出现异常，断开连接，
                CtrlConnect = false;
                Debug.Log(e.Message);
                Console.WriteLine(e.Message);
            }
            catch (Exception e) //zdr-1603: 笼统的异常，都在此句中执行
            {
                CtrlConnect = false;
                Debug.Log(e.StackTrace);
                Console.WriteLine(e.Message);
            }
        }

        private void ConnectCallBack(IAsyncResult ar)
        {
            try
            {
                isConnectionSuccessful = false;
                var socket = ar.AsyncState as Socket;

                if (socket != null)
                {
                    socket.EndConnect(ar);
                    isConnectionSuccessful = true;
                }
            }
            catch (Exception ex)
            {
                isConnectionSuccessful = false;
                socketexception = ex;
            }
            finally
            {
                timeoutObject.Set();
            }
        }

        private void AutoQueryState()
        {
            while (true)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (!CtrlConnect) Thread.Sleep(fAutoSendDelayTime);
                    else
                    {
                        break;
                    }
                }

                if (!CtrlConnect)
                {
                    Debug.Log("exit AutoQueryState");
                    return;
                }
                if (AutoSend && fAutoSendBuffer != "") SendData(fAutoSendBuffer);
                Thread.Sleep(fAutoSendDelayTime);
            }
        }

        //字符串转16进制数组
        public static byte[] StrToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if (hexString.Length % 2 != 0)
                hexString += " ";
            var returnBytes = new byte[hexString.Length / 2];
            for (var i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
    }
}