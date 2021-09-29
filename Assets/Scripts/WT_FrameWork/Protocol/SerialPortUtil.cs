using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Protocol
{
    //flagRS 0:��������  1:��������
    public delegate void OnDevReadSendEvent(int devID, string devName, int flagRS, string strRSData);
    
    /// <summary>
    /// ���ڿ���������
    /// </summary>
    public class SerialPortUtil
    {
        /// <summary>
        /// �����¼��Ƿ���Ч false��ʾ��Ч
        /// </summary>
        public bool ReceiveEventFlag = false;
        //������
        protected byte EndFlag=0x16;
        //�滻���������ַ�
        protected string EndFlagReplace=",";
        /// <summary>
        /// ����Э��ļ�¼�����¼�
        /// </summary>
        public event DataReceivedEventHandler DataReceived;
        public event SerialErrorReceivedEventHandler Error;

        private Thread receiveThread;
        

        protected virtual void ReSolveReceiverData(string strReceiveData)
        {
//            Debug.Log(strReceiveData);
        } //�����յ����ݣ��鷽��������ʵ�� 

        public OnDevReadSendEvent onDevReadSendEvent;//���͡����������¼������ڼ�¼���͡���������

        #region ��������
        private int devID;
        private string devName;
        private string _portName = "COM1";//���ںţ�Ĭ��COM1
        private SerialPortBaudRates _baudRate = SerialPortBaudRates.BaudRate_9600;//������
        private Parity _parity = Parity.None;//У��λ
        private StopBits _stopBits = StopBits.One;//ֹͣλ
        private SerialPortDatabits _dataBits = SerialPortDatabits.EightBits;//����λ

        private SendType dataSendType = SendType.HexType;

        private SerialPort comPort = new SerialPort();

        /// <summary>
        /// �豸ID
        /// </summary>
        public int DevID
        {
            get { return devID; }
            set { devID = value; }
        }

        /// <summary>
        /// �豸����
        /// </summary>
        public string DevName
        {
            get { return devName; }
            set { devName = value; }
        }

        /// <summary>
        /// ���ں�
        /// </summary>
        public string PortName
        {
            get { return _portName; }
            set { _portName = value; }
        }

        /// <summary>
        /// ������
        /// </summary>
        public SerialPortBaudRates BaudRate
        {
            get { return _baudRate; }
            set { _baudRate = value; }
        }

        /// <summary>
        /// ��żУ��λ
        /// </summary>
        public Parity Parity
        {
            get { return _parity; }
            set { _parity = value; }
        }

        /// <summary>
        /// ����λ
        /// </summary>
        public SerialPortDatabits DataBits
        {
            get { return _dataBits; }
            set { _dataBits = value; }
        }

        /// <summary>
        /// ���ݷ��ͽ���ģʽ��16���Ʒ��ͻ��ַ�������
        /// </summary>
        public SendType DataSendType
        {
            get { return dataSendType; }
            set { dataSendType = value; }
        }

        /// <summary>
        /// ֹͣλ
        /// </summary>
        public StopBits StopBits
        {
            get { return _stopBits; }
            set { _stopBits = value; }
        }

        #endregion

        #region ���캯��

        /// <summary>
        /// �������캯����ʹ��ö�ٲ������죩
        /// </summary>
        /// <param name="baud">������</param>
        /// <param name="par">��żУ��λ</param>
        /// <param name="sBits">ֹͣλ</param>
        /// <param name="dBits">����λ</param>
        /// <param name="name">���ں�</param>
        public SerialPortUtil(string name, SerialPortBaudRates baud, Parity par, SerialPortDatabits dBits, StopBits sBits)
        {
            _portName = name;
            _baudRate = baud;
            _parity = par;
            _dataBits = dBits;
            _stopBits = sBits;

            comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
            comPort.ErrorReceived += new SerialErrorReceivedEventHandler(comPort_ErrorReceived);

            
        }

        /// <summary>
        /// �������캯����ʹ���ַ����������죩
        /// </summary>
        /// <param name="baud">������</param>
        /// <param name="par">��żУ��λ</param>
        /// <param name="sBits">ֹͣλ</param>
        /// <param name="dBits">����λ</param>
        /// <param name="name">���ں�</param>
        public SerialPortUtil(string name, string baud, string par, string dBits, string sBits)
        {
            _portName = name;
            _baudRate = (SerialPortBaudRates)Enum.Parse(typeof(SerialPortBaudRates), baud);
            _parity = (Parity)Enum.Parse(typeof(Parity), par);
            _dataBits = (SerialPortDatabits)Enum.Parse(typeof(SerialPortDatabits), dBits);
            _stopBits = (StopBits)Enum.Parse(typeof(StopBits), sBits);

            comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
            comPort.ErrorReceived += new SerialErrorReceivedEventHandler(comPort_ErrorReceived);
        }

        /// <summary>
        /// Ĭ�Ϲ��캯��
        /// </summary>
        public SerialPortUtil()
        {
            _portName = "COM1";
            _baudRate = SerialPortBaudRates.BaudRate_9600;
            _parity = Parity.None;
            _dataBits = SerialPortDatabits.EightBits;
            _stopBits = StopBits.One;
		    
            comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
            comPort.ErrorReceived += new SerialErrorReceivedEventHandler(comPort_ErrorReceived);
        }

        #endregion

        /// <summary>
        /// �˿��Ƿ��Ѿ���
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return comPort.IsOpen;
            }
        }

        /// <summary>
        /// �򿪶˿�
        /// </summary>
        /// <returns></returns>
        public virtual void OpenPort()
        {
            
            if (comPort.IsOpen) comPort.Close();

            comPort.PortName = _portName;
            comPort.BaudRate = (int)_baudRate;
            comPort.Parity = _parity;
            comPort.DataBits = (int)_dataBits;
            comPort.StopBits = _stopBits;
			comPort.ReadTimeout = 50;
			comPort.WriteTimeout = 50;

			comPort.Open();

            receiveThread = new Thread(Receive);
            receiveThread.Start();
        }

        public void OpenPort(string name, SerialPortBaudRates baud, Parity par, SerialPortDatabits dBits, StopBits sBits)
        {
            if (comPort.IsOpen) comPort.Close();

            comPort.PortName = name;
            comPort.BaudRate = (int)baud;
            comPort.Parity = par;
            comPort.DataBits = (int)dBits;
            comPort.StopBits = sBits;
            comPort.ReadTimeout = 50;
            comPort.WriteTimeout = 50;

            comPort.Open();

            receiveThread = new Thread(Receive);
            receiveThread.Start();
        }

        /// <summary>
        /// ����������
        /// </summary>
        /// <returns>����16�����ַ��� </returns>
        public string ReadData()
        {
            string rxString = "";
            if (comPort.IsOpen)
            { 
                try
                {
                    byte tempB = (byte)comPort.ReadByte();

                    while (tempB != 255)
                    {

                        if (tempB!=EndFlag)
                        {
                            rxString += Convert.ToString(tempB, 16).PadLeft(2, '0');
                        }
                        else
                        {
                            rxString += EndFlagReplace;
                        }
                        

                        tempB = (byte)comPort.ReadByte();

                    }
                }
                catch (Exception)
                {

                }
            }
            return rxString;
            
        }

        private void Receive()
        {
            while (true)
            {
                string data = ReadData();
                if(data != "")
                {
                    ReSolveReceiverData(data);
                    if (data != "" && onDevReadSendEvent != null)
                    {
                        onDevReadSendEvent(DevID, DevName, 0, data);
                    }
                }
                Thread.Sleep(1);
            }
        }

        

        /// <summary>
        /// �رն˿�
        /// </summary>
        public virtual void ClosePort()
        {
           
            if (comPort.IsOpen)
            {
                receiveThread.Abort();
                comPort.Close();
            }
          
        }

        /// <summary>
        /// �������Դ�����������Ľ��պͷ��ͻ�����������
        /// </summary>
        public void DiscardBuffer()
        {
            comPort.DiscardInBuffer();
            comPort.DiscardOutBuffer();
        }

        /// <summary>
        /// ���ݽ��մ���Unity�ݲ�֧�ָ��¼�
        /// </summary>
        void comPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //��ֹ�����¼�ʱֱ���˳�
            if (ReceiveEventFlag) return;

            #region ���ݽ����ֽ����ж��Ƿ�ȫ����ȡ���
            List<byte> _byteData = new List<byte>();
            if (comPort.BytesToRead > 0)
            {
                byte[] readBuffer = new byte[comPort.ReadBufferSize + 1];
                int count = comPort.Read(readBuffer, 0, comPort.ReadBufferSize);
                for (int i = 0; i < count; i++)
                {
                    _byteData.Add(readBuffer[i]);

                }
            }
            #endregion

            //�ַ�ת��
            string readString = System.Text.Encoding.Default.GetString(_byteData.ToArray(), 0, _byteData.Count);
            //����������¼�Ĵ���
            if (DataReceived != null)
            {
                DataReceived(new DataReceivedEventArgs(readString));
            }
        }

        /// <summary>
        /// ����������Unity�ݲ�֧�ָ��¼�
        /// </summary>
        void comPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Debug.Log("Error");
            if (Error != null)
            {
                Error(sender, e);
            }
        }

        #region ����д�����

        /// <summary>
        /// д������
        /// </summary>
        /// <param name="msg"></param>
        public void WriteData(string msg)
        {
            if (!(comPort.IsOpen)) comPort.Open();
            comPort.Write(msg);
            if(onDevReadSendEvent != null)
            {
                onDevReadSendEvent(DevID,DevName,1,msg);
            }
        }

        /// <summary>
        /// д������
        /// </summary>
        /// <param name="msg">д��˿ڵ��ֽ�����</param>
        public void WriteData(byte[] msg)
        {
            if (!(comPort.IsOpen)) comPort.Open();

            comPort.Write(msg, 0, msg.Length);
        }

        /// <summary>
        /// д������
        /// </summary>
        /// <param name="msg">����Ҫд��˿ڵ��ֽ�����</param>
        /// <param name="offset">������0�ֽڿ�ʼ���ֽ�ƫ����</param>
        /// <param name="count">Ҫд����ֽ���</param>
        public void WriteData(byte[] msg, int offset, int count)
        {
            if (!(comPort.IsOpen)) comPort.Open();

            comPort.Write(msg, offset, count);
        }

        /// <summary>
        /// ���ʹ�������
        /// </summary>
        /// <param name="SendData">��������</param>
        /// <param name="ReceiveData">��������</param>
        /// <param name="Overtime">�ظ�����</param>
        /// <returns></returns>
        public int SendCommand(byte[] SendData, ref  byte[] ReceiveData, int Overtime)
        {
            if (!(comPort.IsOpen)) comPort.Open();

            ReceiveEventFlag = true;        //�رս����¼�
            comPort.DiscardInBuffer();      //��ս��ջ�����                 
            comPort.Write(SendData, 0, SendData.Length);

            int num = 0, ret = 0;
            while (num++ < Overtime)
            {
                if (comPort.BytesToRead >= ReceiveData.Length) break;
                System.Threading.Thread.Sleep(1);
            }

            if (comPort.BytesToRead >= ReceiveData.Length)
            {
                ret = comPort.Read(ReceiveData, 0, ReceiveData.Length);
            }

            ReceiveEventFlag = false;       //���¼�
            return ret;
        }

        #endregion

        #region ���õ��б����ݻ�ȡ�Ͱ󶨲���

        /// <summary>
        /// ��װ��ȡ���ں��б�
        /// </summary>
        /// <returns></returns>
        public static string[] GetPortNames()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// ���ô��ں�
        /// </summary>
        /// <param name="obj"></param>
        public static void SetPortNameValues(ArrayList obj)
        {
            obj.Clear();
            foreach (string str in SerialPort.GetPortNames())
            {
                obj.Add(str);
            }
        }
        /// <summary>
        /// ���ò�����
        /// </summary>
        public static void SetBauRateValues(ArrayList obj)
        {
            obj.Clear();
            foreach (SerialPortBaudRates rate in Enum.GetValues(typeof(SerialPortBaudRates)))
            {
                obj.Add(((int)rate).ToString());
            }
        }

        /// <summary>
        /// ��������λ
        /// </summary>
        public static void SetDataBitsValues(ArrayList obj)
        {
            obj.Clear();
            foreach (SerialPortDatabits databit in Enum.GetValues(typeof(SerialPortDatabits)))
            {
                obj.Add(((int)databit).ToString());
            }
        }

        /// <summary>
        /// ����У��λ�б�
        /// </summary>
        public static void SetParityValues(ArrayList obj)
        {
            obj.Clear();
            foreach (string str in Enum.GetNames(typeof(Parity)))
            {
                obj.Add(str);
            }            
        }

        /// <summary>
        /// ����ֹͣλ
        /// </summary>
        public static void SetStopBitValues(ArrayList obj)
        {
            obj.Clear();
            foreach (string str in Enum.GetNames(typeof(StopBits)))
            {
                obj.Add(str);
            }
            //foreach (StopBits stopbit in Enum.GetValues(typeof(StopBits)))
            //{
            //    obj.Items.Add(((int)stopbit).ToString());
            //}   
        }

        #endregion
        #region ��ʽת��
        /// <summary>
        /// ת��ʮ�������ַ������ֽ�����
        /// </summary>
        /// <param name="msg">��ת���ַ���</param>
        /// <returns>�ֽ�����</returns>
        public static byte[] HexToByte(string msg)
        {
            msg = msg.Replace(" ", "");//�Ƴ��ո�

            //create a byte array the length of the
            //divided by 2 (Hex is 2 characters in length)
            byte[] comBuffer = new byte[msg.Length / 2];
            for (int i = 0; i < msg.Length; i += 2)
            {
                //convert each set of 2 characters to a byte and add to the array
                comBuffer[i / 2] = (byte)Convert.ToByte(msg.Substring(i, 2), 16);
            }

            return comBuffer;
        }

        /// <summary>
        /// ת���ֽ����鵽ʮ�������ַ���
        /// </summary>
        /// <param name="comByte">��ת���ֽ�����</param>
        /// <returns>ʮ�������ַ���</returns>
        public static string ByteToHex(byte[] comByte)
        {
            StringBuilder builder = new StringBuilder(comByte.Length * 3);
            foreach (byte data in comByte)
            {
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0'));
            }

            return builder.ToString().ToUpper();
        }
        public static string ByteToHex(byte[] comByte,int count)
        {
            StringBuilder builder = new StringBuilder(count * 2);
            foreach (byte data in comByte)
            {
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0'));
            }

            return builder.ToString().ToUpper();
        }
        #endregion

        /// <summary>
        /// ���˿������Ƿ����
        /// </summary>
        /// <param name="port_name"></param>
        /// <returns></returns>
        public static bool Exists(string port_name)
        {
            foreach (string port in SerialPort.GetPortNames()) if (port == port_name) return true;
            return false;
        }
        /// <summary>
        /// ��ʽ���˿��������
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static string Format(SerialPort port)
        {
            return String.Format("{0} ({1},{2},{3},{4},{5})",
                port.PortName, port.BaudRate, port.DataBits, port.StopBits, port.Parity, port.Handshake);
        }
    }

    public class DataReceivedEventArgs : EventArgs
    {
        public string DataReceived;
        public DataReceivedEventArgs(string m_DataReceived)
        {
            this.DataReceived = m_DataReceived;
        }
    }

    public delegate void DataReceivedEventHandler(DataReceivedEventArgs e);


    /// <summary>
    /// ��������λ�б�5,6,7,8��
    /// </summary>
    public enum SerialPortDatabits : int
    {
        FiveBits = 5,
        SixBits = 6,
        SeventBits = 7,
        EightBits = 8
    }

    /// <summary>
    /// ���ڲ������б�
    /// 75,110,150,300,600,1200,2400,4800,9600,14400,19200,28800,38400,56000,57600,
    /// 115200,128000,230400,256000
    /// </summary>
    public enum SerialPortBaudRates : int
    {
        BaudRate_75 = 75,
        BaudRate_110 = 110,
        BaudRate_150 = 150,
        BaudRate_300 = 300,
        BaudRate_600 = 600,
        BaudRate_1200 = 1200,
        BaudRate_2400 = 2400,
        BaudRate_4800 = 4800,
        BaudRate_9600 = 9600,
        BaudRate_14400 = 14400,
        BaudRate_19200 = 19200,
        BaudRate_28800 = 28800,
        BaudRate_38400 = 38400,
        BaudRate_56000 = 56000,
        BaudRate_57600 = 57600,
        BaudRate_115200 = 115200,
        BaudRate_128000 = 128000,
        BaudRate_230400 = 230400,
        BaudRate_256000 = 256000
    }

    //���ݷ��ͽ���ģʽ
    public enum SendType
    {
        HexType = 0,    //16���Ƹ�ʽ����
        StringType = 1  //�ַ�������
    }
}

