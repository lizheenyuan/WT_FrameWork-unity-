using Assets.Scripts.Protocol;

namespace Assets.Scripts.WT_FrameWork.Protocol.ReadCard
{
    public class RFCardBox : SerialPortUtil
    {
        public string CardID;
        protected override void ReSolveReceiverData(string strReceiveData)
        {
            base.ReSolveReceiverData(strReceiveData);
            //截取卡号
            CardID = strReceiveData;
        }
    }
}
