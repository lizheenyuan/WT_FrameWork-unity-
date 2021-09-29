using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.WT_FrameWork.MSGCenter;
using Assets.Scripts.WT_FrameWork.MSGCenter.MsgDefine;
using UnityEngine;
using UnityEngine.Assertions.Must;

/// <summary>
/// 重写Init方法，并在Init中给ID赋值
/// </summary>
public class BDFZ_ClientMsg : CMsgObjBase
{
    #region UnitySystemFunc

    protected sealed override void Awake()
    {
    }

    protected sealed override void Start()
    {
        Init();
    }

    protected sealed override void OnEnable()
    {
        AddListenEvent(WT_Msg.SC_BoardCastChanel_01, OnGetMsgCenterBoardCastMsg);
        AddListenEvent(WT_Msg.SC_UpdateDevData, onGetMsgDataUpdate);
        OnMsgObjEnable();
    }

    protected sealed override void OnDisable()
    {
        RemoveListenEvent(WT_Msg.SC_BoardCastChanel_01);
        RemoveListenEvent(WT_Msg.SC_UpdateDevData);
        OnMsgObjDisable();
    }

    #endregion

    public virtual void RequestServerData(params string[] data)
    {
        DispatchEvent(WT_Msg.CS_UpdateDevData, this, data);
    }
    public virtual void OnGetMsgCenterBoardCastMsg(object arg1, object[] arg2)
    {

    }

    private void onGetMsgDataUpdate(object arg1, object[] arg2)
    {
        if (IsEquals((arg1 as BDFZ_ClientMsg)))
        {
            OnGetMsgDataUpdate(arg1, arg2);
        }
    }
    /// <summary>
    /// 收到服务器数据
    ///  JsonData jda = jd["appSummVOList"];
    ///  List<AppSummVO> applist = JsonMapper.ToObject<List<AppSummVO>>(jda.ToJson());
    /// </summary>
    /// <param name="arg1">BDFZ_ClientMsg 通过id判断是否为本身</param>
    /// <param name="arg2">arg2[0] 表示回传数据</param>
    public virtual void OnGetMsgDataUpdate(object arg1, object[] arg2)
    {
        
    }
    public virtual void OnMsgObjEnable()
    {
    }
    public virtual void OnMsgObjDisable()
    {
    }

    protected virtual bool IsEquals(BDFZ_ClientMsg other)
    {
        return this.Id == other.Id;
    }

    private static int _curId=0;
    private int _id=0;
    /// <summary>
    /// 请给ID赋值
    /// 不能使用unityapi 使用此函数初始化
    /// </summary>
    public virtual void Init()
    {
        _curId++;
        _id = _curId;
    }


    public int Id
    {
        get { return _id; }
        protected set {  }
    }
    
}
