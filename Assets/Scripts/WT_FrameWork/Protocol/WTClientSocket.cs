/******************************************************************************************
 * Name��C# Socket�ͻ���ͨ��Э��
 * Author: LSG
 * CreateTime: 2015-09-07
 * {����������������δ�����������״̬δ����C#�Դ���״̬������}   --> �ѽ��
 * *****************************************�޸���ʷ***************************************
 * zdr-1511:������������������sokcet��Դ���ͷ�.(���ã�shotdown,close)
 *          ɾ�����캯��
 * 
 * zdr-1511: �޸Ķ���������������,                   
 *          1, SendData
 *          �޸ķ�������ǰ�����ӵ��ж�,ʵʱ��������״̬������������CtrlConnectʱ�����ִ���(�磺��֪����ʧ�ܣ��������������Ӳ��ɹ�)
 *          ���쳣����������disconnect���жϿ�����,����fCtrlConnect=false
 *          
 *          2������CtrlConnect����
 *          ����connetǰ������socket,�����½���Socket,���Ӷ��쳣���Ĵ��� 
 *           
 * zdr-1603: �����Զ���ѯ�����Ӷ�����������
 * 
 * zdr-1511: ����״̬�������
 *          1����ȡCtrlConnect״̬����
 *          ��Ҫ�취�ǣ�ͨ���������ݵõ���ǰ����״̬ 
 *          
 * LSG-20160530�������Ż�
 * 1���Զ����ͼ����Ϊ�Ժ���Ϊ��λ
 * 2��ȥ������AutoConnectTime,��Ϊ�����Ե����ӣ���ʼ����0.5�룬���ӳɹ��󣬱�Ϊ10����һ�Σ�
 *    ����ʧ�ܺ�ÿ������ʱ�����1500���룬ֱ��10��
 * LSG-20160803�������������Ӷ�������
 * 1�������������Ӷ�������
 * 2�������һ���豸���Ӳ��ɹ��������޷�������������
 ******************************************************************************************/

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
using UnityEngine;

//zdr-1603:  ����Ҫ���ô���䣬����Ҫ������(����6��) ��  Console.WriteLine    �滻Ϊ��Debug.Log�� �Ա���unity����̨�в鿴

//using Interface3D.Log;

namespace Assets.Scripts.WT_FrameWork.Protocol
{
    //flagRS 0:��������  1:��������
    public delegate void OnDevReadSendEvent(int devID, string devName, int flagRS, string strRSData);

    public class WTClientSocket
    {
        private static Exception socketexception;
        private readonly System.Timers.Timer autoConnectTimer; //zdr-1601: �����Զ����Ӷ�ʱ��
        private Thread autoSendThread;


        //zdr: �豸������������
        private bool bAutoConnect = true; //����������Ĭ��Ϊ����
        private readonly int conectTestInterval = 500; //lzy-1903���Ӳ���ʱ���� ���999ms
        public readonly System.Timers.Timer connectTestTimer; //lzy-1903: ��������ʱ���������лش����豸
        private bool isFirstConnect;
        private long connectTimes;
        protected DateTime dt_lastRecMsgTime; //���һ���յ���Ϣ��ʱ��
        protected string fAutoSendBuffer = "";

        private int fAutoSendDelayTime = 150; //�Զ�������ʱʱ�䣬��λΪ�� Ĭ��Ϊ0.1��
        private Socket fClientSocket; //Socket
        private bool fCtrlConnect; //Socket����

        private string fReceiveBuffer; //�����ַ���
        private readonly int iAutoConnectTime = 1500; //������������ʱ������ ��ʼΪ500���룬���Ϊ10000����
        private bool isConnectionSuccessful;
        public Action<bool> OnConnectStateChanged;


        private readonly byte[] receiveBuffer = new byte[1024]; //���ջ�����
        private readonly ManualResetEvent timeoutObject = new ManualResetEvent(false);

        //zdr-1603: Ĭ�Ϲ��캯��, 
        public WTClientSocket()
        {
            //zdr-1603: ���Ӷ�ʱ���� ��ָ����ʱ���ڣ���ʱ����豸�Ƿ���ߣ������߿�ʼ�Զ�����
            //zdr-1601:  ������ʱ������������ʱ�䣬��ʱ��Ⲣ������ iAutoConnectTimeĬ��ֵΪ5��
            autoConnectTimer = new System.Timers.Timer(iAutoConnectTime); //ʵ����Timer�࣬���ü��ʱ��   
            autoConnectTimer.Elapsed += AutoConnectEvent; //����ʱ���ʱ��ִ���Զ������¼���   
            autoConnectTimer.AutoReset = true; //������ִ��һ�Σ�false������һֱִ��(true)��   
            autoConnectTimer.Enabled = true; //�Ƿ�ִ��System.Timers.Timer.Elapsed�¼��� 

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
                //zdr-1511: �޸�Code,ʵʱ����Socket����״̬
                var blockingState = false;

                //zdr-1511: �ж�����ǰ�������жϣ�Socket�Ƿ���ڣ���������ڣ�ֱ�ӷ���false
                if (fClientSocket == null)
                {
                    fCtrlConnect = false;
                    return false;
                }

                try
                {
                    blockingState = fClientSocket.Blocking;

                    //zdr-1511: ȷ�����ӵĵ�ǰ״̬������з���ֹ�����ֽڵ� Send ����
                    var tmp = new byte[1];
                    fClientSocket.Blocking = false;

                    //zdr-1511:����õ��óɹ����ػ����� WAEWOULDBLOCK ������� (10035)������׽�����Ȼ��������״̬��
                    //        :���򣬸��׽��ֲ��ٴ�������״̬
                    var i = fClientSocket.Send(tmp, 0, 0);
                }
                catch (SocketException e)
                {
                    //zdr-1511: ֻ����ʾ�����Ƿ�ɹ������ô���
                    // 10035 == WSAEWOULDBLOCK
                    if (e.NativeErrorCode.Equals(10035))
                    {
                        Debug.Log("Still Connected, but the Send would block");
                    }
                    else
                    {
                        if (fCtrlConnect) CtrlConnect = false;
                        Debug.Log("Disconnected"); //zdr-1603:  ȥ�������ʾ
                        return fCtrlConnect;
                    }

                    //zdr-1511: �������������������Socket�쳣�У����д���
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
                    //zdr-1511: ����fClientSocket.Blocking�쳣ʱ����ת���ô���ط���
                    //        : ����fClientSocket.Blocking���д�����������
                    e.ToString(); //zdr-1511: ��仰��ʵû���ã�����仰�ĸ������ǿ��ԣ�ȥ����eû��ʹ�õľ��桱
                }

                //zdr-1511: ��ȡ����״̬
                try
                {
                    fCtrlConnect = fClientSocket.Connected;
                }
                catch
                {
                    if (fCtrlConnect) CtrlConnect = false;
                }


                //zdr-1511:���¶�ȡ����״̬��������Ϊ�ϴε�״ֵ̬�������ڸոշ������ݣ����Կ���ʵʱ״̬���
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
                            //zdr-1603: �ж�IP�Ƿ�Ϊ�� ��
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

                            fClientSocket.Connect(ipe); //zdr-1511: ������ӳ��������쳣����(����Catch��)���йر�Socket

                            fCtrlConnect = true;
                            fClientSocket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None,
                                ReceiveData, fClientSocket);
                            //�����߳��Զ���ѯ�豸״̬
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
                            //fClientSocket.Shutdown(SocketShutdown.Both);  //zdr-1512: ע���������                        
                            //zdr-1511: �������ǹر�socket֮��
                            //throw new Exception("Error 10001��" + e.Message);
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
                            fCtrlConnect = false; //zdr-1511: ���false
                            if (autoSendThread != null && CtrlConnect == false) autoSendThread.Abort();
                            fClientSocket.Shutdown(SocketShutdown.Both);
                            fClientSocket.Close(); //zdr-1511: ���close   
                            fClientSocket = null;
                            //if (OnConnectStateChanged != null) OnConnectStateChanged(false);
                        }
                        catch (Exception ex)
                        {
                            //throw new Exception("Error 10002��" + e.Message);
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
                //lzy ����С��ѯʱ����������
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
        } //�����յ����ݣ��鷽��������ʵ�� 

        public event OnDevReadSendEvent onDevReadSendEvent; //���͡����������¼������ڼ�¼���͡���������

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
                //������������û�еõ���Ϣ��ʾû������
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

        //zdr-1603: ʵ���豸�����Զ���������
        private void AutoConnectEvent(object sender, ElapsedEventArgs e)
        {
            if (!CtrlConnect)
            {
                connectTimes++;
                CtrlConnect = true;
            }
        }

        //zdr-1511: ������������Ҫ�ǹر�socket���ӣ��������Ϊ�й���Դ����ϵͳ�Լ�����
        ~WTClientSocket()
        {
            try
            {
                //zdr-1511:�Ͽ����ӣ�
                fClientSocket.Shutdown(SocketShutdown.Both);
                fClientSocket.Close();
                UseConectTest = false;
                if (connectTestTimer!=null)
                {
                    connectTestTimer.Enabled = false;
                }

                autoSendThread?.Abort();
                //zdr-1511:�ͷſռ�, ��������ʾ:C#��Javaһ����new�ڴ��ϵͳ�Զ�������Դ������Ҫdelete      
            }
            catch (ObjectDisposedException e)
            {
                e.ToString(); //zdr-1511: ��仰��ʵû���ã�����仰�ĸ������ǿ��ԣ�ȥ����eû��ʹ�õľ��桱
            }
            catch (NullReferenceException e)
            {
                e.ToString(); //zdr-1511: ��仰��ʵû���ã�����仰�ĸ������ǿ��ԣ�ȥ����eû��ʹ�õľ��桱
            }
            catch (SocketException e)
            {
                e.ToString(); //zdr-1511: ��仰��ʵû���ã�����仰�ĸ������ǿ��ԣ�ȥ����eû��ʹ�õľ��桱
            }
        }

        //��������
        public void SendData(string sendStr)
        {
            var bOrgAutoSendState = true;

            //zdr-1511: ��������ǰ���ж������Ƿ�����
            if (CtrlConnect)
                try
                {
                    bOrgAutoSendState = AutoSend; //zdr-1603: ����ԭ���ķ���״̬

                    AutoSend = false; //ֹͣ�Զ�����,

                    byte[] sendBuf;
                    if (DataSendType == 0) //16���Ʒ���
                        sendBuf = StrToToHexByte(sendStr);
                    else //�ַ�������
                        sendBuf = Encoding.Default.GetBytes(sendStr);
                    fClientSocket.Send(sendBuf);
                    if (onDevReadSendEvent != null)
                        onDevReadSendEvent(DevID, DevName, 1, sendStr);

                    AutoSend = bOrgAutoSendState; //zdr-1603: ���ݷ�����ϣ��ָ�ԭ���Զ�����״̬
                }
                catch (SocketException e)
                {
                    // 10035 == WSAEWOULDBLOCK
                    if (e.NativeErrorCode.Equals(10035))
                    {
                        //Console.WriteLine("Still Connected, but the Send would block");                    
                    }

                    AutoSend = bOrgAutoSendState; //�ָ�ԭ���Զ�����״̬
                    Console.WriteLine(e.Message);
                    Debug.Log(e.Message);
                    //zdr-1511: ����������쳣����һ�´����Ͽ����ӣ�����connectΪfalse
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

                if (DataSendType == 0) //16���ƽ���
                {
                    for (var i = 0; i < length; i++) sbTemp.Append(receiveBuffer[i].ToString("X2"));
                    fReceiveBuffer = sbTemp.ToString();
                }
                else //�ַ�������
                {
                    fReceiveBuffer = Encoding.Default.GetString(receiveBuffer, 0, length);
                }

                if (fReceiveBuffer != "")
                {
                    if (onDevReadSendEvent != null) onDevReadSendEvent(DevID, DevName, 0, fReceiveBuffer);
                    dt_lastRecMsgTime = DateTime.Now;
                    ReSolveReceiverData(fReceiveBuffer);
                    //Debug.Log(DevName + "��ʱ����:   " + fReceiveBuffer);
                }
                fClientSocket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ReceiveData,
                    cskTemp);
               
            }
            catch (SocketException e)
            {
                //zdr-1511: ���ֽ������ݣ�socket�����쳣���Ͽ����ӣ�
                CtrlConnect = false;
                Debug.Log(e.Message);
                Console.WriteLine(e.Message);
            }
            catch (Exception e) //zdr-1603: ��ͳ���쳣�����ڴ˾���ִ��
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

        //�ַ���ת16��������
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