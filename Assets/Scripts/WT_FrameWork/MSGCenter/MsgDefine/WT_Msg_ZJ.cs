namespace Assets.Scripts.WT_FrameWork.MSGCenter.MsgDefine
{
    public partial class WT_Msg
    {
        /// <summary>
        /// //显示提示内容 String,{null}
        /// </summary>
        public static readonly string msg_ShowTip = "msg_ShowTip";//msg_ShowTip|string|null/[bool]|显示提示面板，第二参数的[0]表示是否禁用提示自动隐藏（为空也表示不禁用），第二参数的[1]表示提示文本索引值（为空表示不使用）
        public static readonly string msg_HideTip = "msg_HideTip";//msg_HideTip|null|null|隐藏提示面板
        public static readonly string msg_MapCityName = "msg_MapCityName";//msg_MapCityName|null|[Address]|更改地图城市名
        public static readonly string msg_ZJYiChang = "msg_ZJYiChang";//msg_ZJYiChang|bool,[imgurl,reasonid]|用于添加异常id和现场照片 bool? Add:Remove
        public static readonly string msg_ShowGaiyao = "msg_ShowGaiyao";//msg_ShowGaiyao|ID,null|用于显示概要界面 Arg0 type异常类型
        public static readonly string msg_ShowGongdan = "msg_ShowGongdan";//msg_ShowGongdan|bool,null|用于显示详细工单界面 Arg0 type异常类型
        public static readonly string msg_SubAppNo = "msg_SubAppNo";//msg_SubAppNo|string|null|用于提交当前工单编号
        public static readonly string msg_OpenMap = "msg_OpenMap";//地图打开发送请求城市名
        public static readonly string msg_GJQChose = "msg_GJQChose";//是否选择工器具 msg_GJQChose bool|String
        public static readonly string msg_CancelView = "msg_CancelView";//关闭视野移动界面
        public static readonly string msg_MeterBoxBtnClick = "msg_MeterBoxBtnClick";//点击表箱按钮
        public static readonly string msg_ShowMeterBoxList = "msg_ShowMeterBoxList";//显示表箱列表 arg1:bool
        public static readonly string msg_SetCameraPos = "msg_SetCameraPos";//改变摄像机位置
        public static readonly string msg_StopLoadData = "msg_StopLoadData";//取消加载数据
        public static readonly string msg_SucessLoadData = "msg_SucessLoadData";//成功加载数据
    }
}