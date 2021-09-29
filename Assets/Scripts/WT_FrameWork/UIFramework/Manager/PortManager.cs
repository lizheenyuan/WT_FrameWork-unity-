using Assets.Scripts.Protocol;
//using Assets.Scripts.WT_FrameWork.Controller;
using Assets.Scripts.WT_FrameWork.Protocol.ReadCard;
using Assets.Scripts.WT_FrameWork.SingleTon;

namespace Assets.Scripts.WT_FrameWork.UIFramework.Manager
{
    public class PortManager : WT_Singleton<PortManager>
    {
        private RFCardBox card_box;
//        private FireExtController fire_Ext;

        public RFCardBox CardBox
        {
            get { return card_box; }
        }

//        public FireExtController FireExt
//        {
//            get { return fire_Ext; }
//        }

        public override void Init()
        {
            base.Init();
            card_box = new RFCardBox();
//            fire_Ext = new FireExtController(Util.Util.GetSystemConfig("PortConfig", "MieHuoQi_COM"),
//                SerialPortBaudRates.BaudRate_9600, System.IO.Ports.Parity.None, SerialPortDatabits.EightBits,
//                System.IO.Ports.StopBits.One);
        }

        public override void UnInit()
        {
            base.UnInit();
            card_box.ClosePort();
//            fire_Ext.ClosePort();
        }
    }
}