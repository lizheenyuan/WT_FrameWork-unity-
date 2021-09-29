using System;
using Assets.Scripts.User;
using Assets.Scripts.WT_FrameWork.SingleTon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 从服务器获取概要的类
/// </summary>
public class GongdanGaiyaoBase
{
    /// <summary>
    /// 工单总数
    /// </summary>
    public bool successful;
    public AppSum resultValue;
    public string type;
    public string resultHint;
    public string errorPage;   
}

public class AppSum
{
    public int num;
    /// <summary>
    /// 考试信息
    /// </summary>
    public CrtExamPlanVO crtExamPlanVO;
    /// <summary>
    /// 工单概要清单
    /// </summary>
    public List<AppSummVO> appSummVOList;
}

/// <summary>
/// 从服务器获取明细的类
/// </summary>
public class GongdanMingxiBase
{
    public bool successful;
    public AppDetailVO[] resultValue;
    public string type;
    public string resultHint;
    public string errorPage;
}
/// <summary>
/// 从服务器获取异常现象和原因
/// </summary>
public class YiChangAndReasonBase
{
    public bool successful;
    public YichangResultValue resultValue;
    public string type;
    public string resultHint;
    public string errorPage;  
}

public class YichangResultValue
{
    public FaultReasonVO[] JL;
    public FaultReasonVO[] CJ;
    public FaultReasonVO[] XS;
}

public class SubmitResult
{   
    public bool successful;
    public object resultValue;
    public string resultHint;
    public object errorPage;
    public string type;
}


/// <summary>
/// 工单明细
/// </summary>
public class AppDetailVO
{
    //异常设备类型
    public string faultType;
    //工单编号
    public int workOrderID;
    //异常设备类型
    public string equipTypeName;
    //设备类型
    public string equipCategoryName;
    //异常现象
    public string faultName;
    //异常发生时间
    public DateTime faultAppearTime;
    //仿真装置资产编号
    public string fZGAssetNo;
    //台区编号
    public int fZGID;
    //台区名称
    public string fZGName;
    //户号
    public int ammeterID;
    //户名
    public object ammeterName;
    //终端逻辑地址
    public object terminalLogicalAddr;
    //终端资产编号
    public string terminalAssetNo;
    //采集器资产编号
    public object acquisitionAssetNo;
    //电能表资产编号
    public object ammeterAssetNo;
    //用电地址
    public object ammeterAddr;
    //受电容量(KVA)
    public object electricalCapacity;
    //电力用户类型
    public string electricUserType;
    //终端通信方式
    public object terminalCommunicateModeName;
    //终端接线方式
    public string terminalConnectionModeName;
    //派发人ID
    public string dispatchUserID;
    //派发时间
    public DateTime dispatchTime;

    public string equipLoc;
    public int equipNoInFZU;
    public string fZUNoInFZG;
    public int fZUNoInSubFZG;
    public int subFZGNo;
}

/// <summary>
/// 工单概要清单
/// </summary>
public class AppSummVO
{
    //异常设备类型
    public string faultType;
    //工单编号
    public int workOrderID;
    //设备类型
    public string equipCategoryName;
    //门牌号
    public string equipLoc;
    public int equipNoInFZU;
    public string fZUNoInFZG;
    public int fZUNoInSubFZG;
    public int subFZGNo;
    //仿真装置资产编号
    public string fZGAssetNo;
    //电表名称
    public object ammeterName;
    //用电地址
    public object ammeterAddr;
    //终端名称
    public string terminalName;
    //终端资产编号
    public string terminalAssetNo;
    //电能表资产编号
    public object ammeterAssetNo;
    //采集设备资产编号
    public object acquisitionAssetNo;
    //用户用电类型
    public string electricUserType;
    //工单状态
    public string workOrderStatus;
    //归档失败次数
    public int filedFailTimes;
    //反馈的异常原因，可同时反馈多个异常原因
    public object feedbackReasonIDS; 
}
/// <summary>
/// 当前考试状态
/// </summary>
public class CrtExamPlanVO
{
    //当前考试
    public int examID;
    //当前轮次
    public int crtRoundID;
    //当前轮次状态
    public string crtRoundStatus;
}
/// <summary>
/// 异常原因
/// </summary>
public class FaultReasonVO
{
    //异常现象编号
    public string faultID;
    //异常现象名称
    public string faultName;
    //异常原因编号
    public string reasonID;
    //异常原因名称
    public string reasonName;
    //原因类别编号
    public string reasonTypeID;
    //原因类别名称
    public string reasonTypeName;
    //异常设备（例如：集中器，采集器，电能表）
    public string equipCategoryName;

}