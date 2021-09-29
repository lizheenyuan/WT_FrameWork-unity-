namespace Assets.Scripts.WT_FrameWork.Protocol
{
    /// <summary>
    /// 脉冲板通讯协议 9600 无校验
    /// </summary>
    public class PulseBoard: WTClientSocket
    {
        public void SendGears(int num)
        {
            string strCMD = "0103" + num.ToString("X2") + "00000064AC";
            SendData(strCMD);
        }
    }
}
