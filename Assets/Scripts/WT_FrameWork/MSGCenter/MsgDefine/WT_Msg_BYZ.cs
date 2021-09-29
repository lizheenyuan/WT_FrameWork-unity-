namespace Assets.Scripts.WT_FrameWork.MSGCenter.MsgDefine
{
    public partial class WT_Msg
    {
        public static readonly string msg_ClickDevice = "msg_ClickDevice";//点击可操作设备(string)name,{null}

        public static readonly string msg_ClickDevToUI = "msg_ClickDevToUI";//点击可操作设备向UI发送消息(string)msg,{null}

        public static readonly string msg_DeviceView = "msg_DeviceView";//切换到设备视角，仅在已选择设备时生效 null,{null}

        public static readonly string msg_MoveView = "msg_MoveView";//上下左右移动视角(bool)horiMove,{(bool)isToMax}  参数分别代表“横向移动”、“正向移动”

        public static readonly string msg_ZoomView = "msg_ZoomView";//前后缩放视角(bool)zoomIn,{null}  参数表示向前拉近

        public static readonly string msg_CancelDeviceView = "msg_CancelDeviceView";//取消设备视角 null,{null}

        public static readonly string msg_CheckUseGQJ = "msg_CheckUseGQJ";//检查获取当前使用的工器具null,{null}

        public static readonly string msg_SendUseGQJ = "msg_SendUseGQJ";//发送当前使用的工器具(bool)useGQJ,{(string)gqjName}  参数表示是否使用工器具，使用工器具的名称（若无则为null）

        public static readonly string msg_CameraInitReady = "msg_CameraInitReady";//发送消息以便通知相机初期化已完成null,{null}

        public static readonly string msg_CanSelectMeter = "msg_CanSelectMeter";//发送消息通知是否可以点击选择表计设备(bool)canSelect,{null}

        public static readonly string msg_AddUseGQJ = "msg_AddUseGQJ";//向UI发送消息添加取出的工器具(GameObject)gqj,{null}
    }
}