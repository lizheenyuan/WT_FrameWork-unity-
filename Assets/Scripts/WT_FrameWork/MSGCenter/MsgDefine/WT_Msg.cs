namespace Assets.Scripts.WT_FrameWork.MSGCenter.MsgDefine
{
    public partial class WT_Msg
    {
        public static readonly string SC_BoardCastChanel_01 = "SC_BoardCastChanel_01";

        public static readonly string CS_UpdateDevData = "CS_UpdateDevData"; //C2S:this,{WT_RequesDataType,data0,data1,data...};

        public static readonly string SC_UpdateDevData = "SC_UpdateDevData"; //client,{jsondata}



    }

    public class WT_RequesDataType
    {
        public const string AppSumm = "AppSumm";//不需要参数
        public const string AppDetail = "AppDetail";//arg0:appnode arg1:apptype
        public const string MeterData = "MeterData";//arg0:terminalID arg1:measureIndex(可选不为空表示集中器下具体一个表信息，为空表示集中器下所有信息)
        public const string FaultReasonList = "FaultReasonList";//不需要参数
        public const string SubmitAppResult = "SubmitAppResult";//arg0:appNo工单编号，arg1:externalEquipCheckStatus=true，arg2:processRemark=测试，arg3:ressonRelateIMGStr
        public const string QueryFzgWorkOrder = "QueryFzgWorkOrder";//获取所有设备概要 不需要参数
        public const string QueryFzgOutLine = "QueryFzgOutLine";//获取台区即台区下的表箱号
        public const string QueryEquips = "QueryEquips";//查询表箱中的设备内容 arg0:fzgid仿真柜id arg1:fzuno 仿真单元编码 
        public const string CallCalDataByFzu = "CallCalDataByFzu";//获取表箱内的所有表具体要显示的数据  arg0:fZGID (OutLineVO->fzgid) arg1:fZUNoInFZG 表箱号
    }

    public class WT_RequestState
    {
        public const string ParamIsNull = "ParamIsNull";//消息的WT_RequesDataType为空
        public const string ParamError = "ParamError";//消息的WT_RequesDataType 内容错误
    }
}
