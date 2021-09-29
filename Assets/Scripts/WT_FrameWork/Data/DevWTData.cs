using WT_FrameWork.Data;

namespace Assets.Scripts.WT_FrameWork.Data
{
    public class DevWTData:DataBase
    {
        public uint DevId { get; set; }
        public string DevName { get; set; }
        public string Introduce { get; set; }
        public string Items { get; set; }// | 分割关联devitems表
        public string OtherInfo { get; set; }
    }
}