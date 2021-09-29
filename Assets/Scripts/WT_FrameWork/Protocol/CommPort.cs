using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Assets.Scripts.Protocol
{
    //flagRS 0:接收数据  1:发送数据
    //public delegate void OnDevReadSendEvent(int devID, string devName, int flagRS, string strRSData);

    //Global.Com = new CommPort();
    //Global.Com.PortNum = 4; //端口号   
    //Global.Com.BaudRate = CommPort.StanderdRate.R9600; //串口通信波特率   
    //Global.Com.ByteSize = 8; //数据位   
    //Global.Com.Parity = 0; //奇偶校验   
    //Global.Com.StopBits = 1;//停止位   
    //Global.Com.ReadTimeout = 1000; //读超时   

    //Global.Com.CtrlConnect = true;
    //Global.Com.SendData("11223344");
    public class CommPort
    {
        public enum SendType
        {
            HexType = 0,
            StringType = 1
        }

        public enum StanderdRate
        {
            R50 = 50,
            R75 = 75,
            R110 = 110,
            R150 = 150,
            R300 = 300,
            R600 = 600,
            R1200 = 1200,
            R2400 = 2400,
            R4800 = 4800,
            R9600 = 9600,
            R19200 = 19200,
            R38400 = 38400,
            R57600 = 57600,
            R76800 = 76800,
            R115200 = 115200
        }

        private int fDevID;         //设备ID；
        private string fDevName;    //设备名称；
        private int portNum = 1;
        private StanderdRate baudRate = StanderdRate.R9600;
        private byte byteSize = 8;
        private byte parity = 0;
        private byte stopBits = 1;
        private int readTimeOut = 1000;
        private SendType fDataSendType = SendType.HexType;  //数据发送格式 0: 16进制发送    1：字符串发送
        private int fAutoSendDelayTime = 30; //自动发送延时时间，单位为秒 默认为30秒
        private bool fStopAutoSend = false; //是否停止自动发送

        public event OnDevReadSendEvent onDevReadSendEvent; //发送、接收内容事件，用于记录发送、接收内容
        
        public int DevID
        {
            get { return fDevID; }
            set { fDevID = value; }
        }
        public string DevName
        {
            get { return fDevName; }
            set { fDevName = value; }
        }
        public int PortNum
        {
            get { return portNum; }
            set { portNum = value; }
        }
        public StanderdRate BaudRate
        {
            get { return baudRate; }
            set { baudRate = value; }
        }
        public byte ByteSize
        {
            get { return byteSize; }
            set { byteSize = value; }
        }
        public byte Parity // 0-4=no,odd,even,mark,space   
        {
            get { return parity; }
            set { parity = value; }
        }
        public byte StopBits // 0,1,2 = 1, 1.5, 2   
        {
            get { return stopBits; }
            set { stopBits = value; }
        }
        public int ReadTimeout
        {
            get { return readTimeOut; }
            set { readTimeOut = value; }
        }
        public SendType DataSendType
        {
            get { return fDataSendType; }
            set { fDataSendType = value; }
        }
        public int AutoSendDelayTime
        {
            get { return fAutoSendDelayTime; }
            set { fAutoSendDelayTime = value; }
        }
        public bool StopAutoSend
        {
            get { return fStopAutoSend; }
            set { fStopAutoSend = value; }
        }
        public bool CtrlConnect
        {
            get { return opened; }
            set
            {
                if (opened != value)
                {
                    try
                    {
                        if (value)
                        {
                            Open();
                        }
                        else
                        {
                            Close();
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }
        }
  
        //comm port win32 file handle   
        private int hComm = -1;   
  
        private bool opened = false;   
  
        //win32 api constants   
        private const uint GENERIC_READ = 0x80000000;   
        private const uint GENERIC_WRITE = 0x40000000;   
        private const int OPEN_EXISTING = 3;   
        private const int INVALID_HANDLE_VALUE = -1;   
  
        [StructLayout(LayoutKind.Sequential)]   
        public struct DCB 
        {   
            //taken from c struct in platform sdk   
            public int DCBlength; // sizeof(DCB)   
            public StanderdRate BaudRate; // current baud rate   
            /* these are the c struct bit fields, bit twiddle flag to set  
            public int fBinary; // binary mode, no EOF check  
            public int fParity; // enable parity checking  
            public int fOutxCtsFlow; // CTS output flow control  
            public int fOutxDsrFlow; // DSR output flow control  
            public int fDtrControl; // DTR flow control type  
            public int fDsrSensitivity; // DSR sensitivity  
            public int fTXContinueOnXoff; // XOFF continues Tx  
            public int fOutX; // XON/XOFF out flow control  
            public int fInX; // XON/XOFF in flow control  
            public int fErrorChar; // enable error replacement  
            public int fNull; // enable null stripping  
            public int fRtsControl; // RTS flow control  
            public int fAbortOnError; // abort on error  
            public int fDummy2; // reserved  
            */   
            public uint flags;   
            public ushort wReserved; // not currently used   
            public ushort XonLim; // transmit XON threshold   
            public ushort XoffLim; // transmit XOFF threshold   
            public byte ByteSize; // number of bits/byte, 4-8   
            public byte Parity; // 0-4=no,odd,even,mark,space   
            public byte StopBits; // 0,1,2 = 1, 1.5, 2   
            public char XonChar; // Tx and Rx XON character   
            public char XoffChar; // Tx and Rx XOFF character   
            public char ErrorChar; // error replacement character   
            public char EofChar; // end of input character   
            public char EvtChar; // received event character   
            public ushort wReserved1; // reserved; do not use   
        }   
  
        [StructLayout(LayoutKind.Sequential)]   
        private struct COMMTIMEOUTS 
        {   
            public int ReadIntervalTimeout;   
            public int ReadTotalTimeoutMultiplier;   
            public int ReadTotalTimeoutConstant;   
            public int WriteTotalTimeoutMultiplier;   
            public int WriteTotalTimeoutConstant;   
        }   
  
        [StructLayout(LayoutKind.Sequential)]   
        private struct OVERLAPPED 
        {   
            public int Internal;   
            public int InternalHigh;   
            public int Offset;   
            public int OffsetHigh;   
            public int hEvent;   
        }   
  
        [DllImport("kernel32.dll")]   
        private static extern int CreateFile(   
        string lpFileName, // file name   
        uint dwDesiredAccess, // access mode   
        int dwShareMode, // share mode   
        int lpSecurityAttributes, // SD   
        int dwCreationDisposition, // how to create   
        int dwFlagsAndAttributes, // file attributes   
        int hTemplateFile // handle to template file   
        );   

        [DllImport("kernel32.dll")]   
        private static extern bool GetCommState(   
        int hFile, // handle to communications device   
        ref DCB lpDCB // device-control block   
        );   

        [DllImport("kernel32.dll")]   
        private static extern bool BuildCommDCB(   
        string lpDef, // device-control string   
        ref DCB lpDCB // device-control block   
        );   

        [DllImport("kernel32.dll")]   
        private static extern bool SetCommState(   
        int hFile, // handle to communications device   
        ref DCB lpDCB // device-control block   
        );   

        [DllImport("kernel32.dll")]   
        private static extern bool GetCommTimeouts(   
        int hFile, // handle to comm device   
        ref COMMTIMEOUTS lpCommTimeouts // time-out values   
        );  
 
        [DllImport("kernel32.dll")]   
        private static extern bool SetCommTimeouts(   
        int hFile, // handle to comm device   
        ref COMMTIMEOUTS lpCommTimeouts // time-out values   
        );   

        [DllImport("kernel32.dll")]   
        private static extern bool ReadFile(   
        int hFile, // handle to file   
        byte[] lpBuffer, // data buffer   
        int nNumberOfBytesToRead, // number of bytes to read   
        ref int lpNumberOfBytesRead, // number of bytes read   
        ref OVERLAPPED lpOverlapped // overlapped buffer   
        );   

        [DllImport("kernel32.dll")]   
        private static extern bool WriteFile(   
        int hFile, // handle to file   
        byte[] lpBuffer, // data buffer   
        int nNumberOfBytesToWrite, // number of bytes to write   
        ref int lpNumberOfBytesWritten, // number of bytes written   
        ref OVERLAPPED lpOverlapped // overlapped buffer   
        );   

        [DllImport("kernel32.dll")]   
        private static extern bool CloseHandle(   
        int hObject // handle to object   
        );   

        [DllImport("kernel32.dll")]   
        private static extern uint GetLastError();   
  
        private void Open() 
        {   
            DCB dcbCommPort = new DCB();   
            COMMTIMEOUTS ctoCommPort = new COMMTIMEOUTS();       
            // OPEN THE COMM PORT.     
            hComm = CreateFile("COM" + PortNum ,GENERIC_READ | GENERIC_WRITE,0, 0,OPEN_EXISTING,0,0);     
            // IF THE PORT CANNOT BE OPENED, BAIL OUT.   
            if(hComm == INVALID_HANDLE_VALUE) 
            {   
                throw(new ApplicationException("Comm Port Can Not Be Opened1"));   
            }   
  
            // SET THE COMM TIMEOUTS.     
            GetCommTimeouts(hComm,ref ctoCommPort);   
            ctoCommPort.ReadTotalTimeoutConstant = ReadTimeout;   
            ctoCommPort.ReadTotalTimeoutMultiplier = 0;   
            ctoCommPort.WriteTotalTimeoutMultiplier = 0;   
            ctoCommPort.WriteTotalTimeoutConstant = 0;   
            SetCommTimeouts(hComm,ref ctoCommPort);   
  
            // SET BAUD RATE, PARITY, WORD SIZE, AND STOP BITS.   
            GetCommState(hComm, ref dcbCommPort);   
            dcbCommPort.BaudRate=BaudRate;   
            dcbCommPort.flags=0;   
            //dcb.fBinary=1;   
            dcbCommPort.flags|=1;   
            if (Parity>0)   
            {   
                //dcb.fParity=1   
                dcbCommPort.flags|=2;   
            }   
            dcbCommPort.Parity=Parity;   
            dcbCommPort.ByteSize=ByteSize;   
            dcbCommPort.StopBits=StopBits;   
            if (!SetCommState(hComm, ref dcbCommPort))   
            {   
                //uint ErrorNum=GetLastError();   
                throw(new ApplicationException("Comm Port Can Not Be Opened2"));   
            }   
            //unremark to see if setting took correctly   
            //DCB dcbCommPort2 = new DCB();   
            //GetCommState(hComm, ref dcbCommPort2);   
            opened = true;     
        }   
  
        private void Close() 
        {   
            if (hComm!=INVALID_HANDLE_VALUE) 
            {   
                CloseHandle(hComm);
                opened = false;
            }   
        }   

        public byte[] Read(int NumBytes) 
        {   
            byte[] BufBytes;   
            byte[] OutBytes;   
            BufBytes = new byte[NumBytes];   
            if (hComm!=INVALID_HANDLE_VALUE) 
            {   
                OVERLAPPED ovlCommPort = new OVERLAPPED();   
                int BytesRead=0;   
                ReadFile(hComm,BufBytes,NumBytes,ref BytesRead,ref ovlCommPort);   
                OutBytes = new byte[BytesRead];   
                Array.Copy(BufBytes,OutBytes,BytesRead);   
            }   
            else 
            {   
                throw(new ApplicationException("Comm Port Not Open"));   
            }   
            return OutBytes;   
        }   
  
        private void Write(byte[] WriteBytes) 
        {   
            if (hComm!=INVALID_HANDLE_VALUE) 
            {   
                OVERLAPPED ovlCommPort = new OVERLAPPED();   
                int BytesWritten = 0;   
                WriteFile(hComm,WriteBytes,WriteBytes.Length,ref BytesWritten,ref ovlCommPort);   
            }   
            else 
            {   
                throw(new ApplicationException("Comm Port Not Open"));   
            }   
        }

        /// <summary>
        /// 发送数据 字符串
        /// </summary>
        /// <param name="sendContent">数据内容字符串</param>
        public void SendData(string sendContent)
        {
            byte[] buf;
            if (fDataSendType == SendType.HexType)
            {
                buf = StrToToHexByte(sendContent);
            }
            else
            {
                buf = System.Text.Encoding.Default.GetBytes(sendContent);
            }
            Write(buf);
            if (onDevReadSendEvent != null)
            {
                onDevReadSendEvent(DevID,DevName,1,sendContent);
            }
        }

        /// <summary>
        /// 发送数据 字节数组
        /// </summary>
        /// <param name="buf">字节数组</param>
        public void SendData(byte[] buf)
        {
            Write(buf);
            if (onDevReadSendEvent != null)
            {
                StringBuilder sbTemp = new StringBuilder();
                for (int i = 0; i < buf.Length; i++)
                {
                    sbTemp.Append(buf[i].ToString("X2"));
                }
                onDevReadSendEvent(DevID, DevName, 1, sbTemp.ToString());
            }            
        }

        //释放时关闭串口
        ~CommPort()
        {
            Close();
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
