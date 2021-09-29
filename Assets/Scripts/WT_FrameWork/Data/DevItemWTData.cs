using WT_FrameWork.Data;

namespace Assets.Scripts.WT_FrameWork.Data
{
    public class DevItemWTData:DataBase
    {
        public uint DId { get; set; }
        public string ItemName { get; set; }
        public string ItemContent { get; set; }
        public string OtherInfo { get; set; }
    }
}