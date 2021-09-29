using Assets.Scripts.Public;

namespace Assets.Scripts.WT_FrameWork.Protocol
{
    /// <summary>
    /// 数码管通信协议
    /// </summary>
    public class NumberingTube: WTClientSocket
    {
        public void SendIntData(int value)
        {
            string strDeviceAddr = "01";    //设备地址
            string strFunctionCode = "06";  //功能码
            string strPhysicsBeginAddr = "0088";    //设置数据的物理起始地址
            string strNum = value.ToString("X4");                 //设置值
            string strCmd = strDeviceAddr + strFunctionCode + strPhysicsBeginAddr + strNum;
            strCmd = strCmd + PubFunction.GetDataCRC(strCmd);
            SendData(strCmd);
        }
    }
}
