//#define TEST_MODE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Assets.Scripts.WT_FrameWork.LoadSource;
using Assets.Scripts.WT_FrameWork.MSGCenter;
using Assets.Scripts.WT_FrameWork.MSGCenter.MsgDefine;
using Assets.Scripts.WT_FrameWork.UIFramework.Manager;
using Assets.Scripts.WT_FrameWork.UIFramework.PanelScripts;
using LitJson;
using UnityEngine;
// using UnityEngine.Experimental.PlayerLoop;

public class ClientRequestInfo
{
    private BDFZ_ClientMsg _client;
    private string _requestUrl;
    private string _requestDataType;
    public WWWForm PostData;
    public Dictionary<string, string> RequestHeader;
    public WT_DownLoader.DownMethodloadType RequestDownloadType;
    public ClientRequestInfo(BDFZ_ClientMsg client,string d,string requestDataType, WT_DownLoader.DownMethodloadType rdt=WT_DownLoader.DownMethodloadType.Get)
    {
        _client = client;
        _requestUrl = d;
        _requestDataType = requestDataType;
        RequestDownloadType = rdt;
        RequestHeader=new Dictionary<string, string>();
    }

    public BDFZ_ClientMsg Client
    {
        get { return _client; }
    }

    public string RequestUrl
    {
        get { return _requestUrl; }
    }

    public string RequestDataType
    {
        get { return _requestDataType; }
    }
}
/// <summary>
/// WT_Msg.CS_UpdateDevData消息 arg0 第一参数为BDFZ_ClientMsg即谁给我发的消息，arg1中的arg1[0]表示ServerUrl中的方法名 arg1[1-...]表示参数
/// </summary>
public class BDFZ_ServerMsg :CMsgObjBase
{
    public int MaxRequsetCount=1;
    private List<Coroutine> _requsetCoroutines;
    private Queue<ClientRequestInfo> _requestWaitQueue;
    public Encoding JsonSerializeEncodeEncoding=Encoding.UTF8;
    public Action<ClientRequestInfo> OnRequestFailed;
    void Start()
    {
        Debug.Log("msg center start---");
        AddListenEvent(WT_Msg.CS_UpdateDevData, ResolveRequest);
        _requsetCoroutines=new List<Coroutine>(MaxRequsetCount);
        _requestWaitQueue=new Queue<ClientRequestInfo>();
        OnRequestFailed += (cinfo) =>
        {
            NoticePanel.Show("Request Error",cinfo?.RequestUrl,NoticePanel.NoticeBtnType.Yes);
        };
    }
    /// <summary>
    /// 处理请求
    /// </summary>
    /// <param name="arg1">BDFZ_ClientMsg</param>
    /// <param name="arg2">null</param>
    private void ResolveRequest(object arg1, object[] arg2)
    {
        Debug.Log("msg center ResolveRequest---");
        BDFZ_ClientMsg client = null;
        string requestDT = null;
        if (arg1==null)
        {
            //处理没有client对象异常 ---暂时考虑为不赋值
            Debug.LogError("Server: arg0 is null...");
            return;
        }
        else
        {
            if (typeof(BDFZ_ClientMsg).IsInstanceOfType(arg1))
            {
                client = arg1 as BDFZ_ClientMsg;//根据参数给client赋值
            }
            else
            {
                Debug.LogError("Server: arg0 is`t a BDFZ_ClientMsg...");
                return;
            }
        }
        if (arg2[0] != null)
        {
            requestDT = arg2[0] as string;
        }
        else
        {
            //处理 WT_RequesDataType 为空的异常
            OnGetServerData(client,WT_RequestState.ParamIsNull,null);
            Debug.LogError("Server: request ParamIsNull...");
            return;
        }
        MethodInfo info = typeof(ServerUrl).GetMethod(requestDT, BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public);
        if (info==null)
        {
            //处理WT_RequesDataType名称错误异常
            OnGetServerData(client, WT_RequestState.ParamError, null);
            Debug.LogError("Server: request ParamError...");
            return;
        }
        else
        {
            try
            {
                //处理 post与get
                if (requestDT== WT_RequesDataType.SubmitAppResult)
                {
                    var crinfo = new ClientRequestInfo(client, info.Invoke(null, null).ToString(), requestDT,
                        WT_DownLoader.DownMethodloadType.Post)
                    {
                        PostData = new WWWForm(){},
                        
                        RequestHeader = new Dictionary<string, string>()
                            {{"Content-Type", "application/x-www-form-urlencoded"}}
                    };
                    crinfo.PostData.AddField("uid",ServerUrl.UID);
                    crinfo.PostData.AddField("appNo", arg2[1].ToString());
                    crinfo.PostData.AddField("externalEquipCheckStatus", arg2[2].ToString());
                    crinfo.PostData.AddField("processRemark", arg2[3].ToString());
                    crinfo.PostData.AddField("ressonRelateIMGStr", JsonMapper.ToJson(arg2[4]));
                    _requestWaitQueue.Enqueue(crinfo);
                }
                else
                {
                    _requestWaitQueue.Enqueue(new ClientRequestInfo(client, info.Invoke(null, GetMsgDatas(arg2)).ToString(), requestDT));
                }
            }
            catch (Exception e)
            {
                OnGetServerData(client, WT_RequestState.ParamError, null);
                Debug.LogError("Server: request ParamError...");
                Debug.LogError(e.Message+"\n"+e.StackTrace+"\n"+ requestDT);
            }
            
        }
    }

    object[] GetMsgDatas(object[] datas)
    {
        if (datas==null||datas.Length==0)
        {
            return null;
        }
        List<object> ls=new List<object>(datas);
        ls.RemoveAt(0);
        return ls.ToArray();
    }
    void SendWebRequest(ClientRequestInfo clientinfo)
    {
        if (clientinfo != null&&!_requestWaitQueue.Contains(clientinfo))
        {
            _requestWaitQueue.Enqueue(clientinfo);
        }
    }

    void OnGetServerData(BDFZ_ClientMsg client,string wtRequestType,string jdstr)
    {
        DispatchEvent(WT_Msg.SC_UpdateDevData,client,new []{ wtRequestType, jdstr });
    }
    void Update()
    {
        //lock (_requsetCoroutines)
        {
            while (_requsetCoroutines.Count<=MaxRequsetCount&&_requestWaitQueue.Count>0)
            {
                var cinfo = _requestWaitQueue.Dequeue();
                Coroutine c=null;
                c= StartCoroutine(GameRoot.DownLoader.LoadAsset(data =>
                {
                    JsonData jd = null;
                    string datastr=null;
                    if (data!=null)
                    {
                        try
                        {
                            datastr = JsonSerializeEncodeEncoding.GetString(data);
                            jd = JsonMapper.ToObject(datastr);
                        }
                        catch (Exception e)
                        {
                            _requsetCoroutines.Remove(c);
                            Debug.LogError("Can`t parse webserver msg to json");
                            OnRequestFailed?.Invoke(cinfo);
                        }
                       
                    }
                    else
                    {
                        Debug.Log("Request failed!");
                        OnRequestFailed?.Invoke(cinfo);
                    }

                    try
                    {
                        if (datastr != null && jd != null && (bool)jd["successful"] == true)
                        {
                            OnGetServerData(cinfo.Client, cinfo.RequestDataType, datastr);
                        }
                        else
                        {
                            OnGetServerData(cinfo.Client, cinfo.RequestDataType, null);
                        }

                        if (c != null && _requsetCoroutines.Contains(c))
                        {
                            _requsetCoroutines.Remove(c);
                        }

                    }
                    catch (Exception e)
                    {
                        NoticePanel.Show("internal error",e.Message,NoticePanel.NoticeBtnType.Yes);
                    }
                   
                }, cinfo.RequestUrl,cinfo.PostData,new Dictionary<string, string>(cinfo.RequestHeader){ { "token",ServerUrl.RequestToken }},cinfo.RequestDownloadType ));
                _requsetCoroutines.Add(c);
            }
        }
    }
}

class ServerUrl
{
#if UNITY_EDITOR||TEST_MODE
    private static string _urlBase = "http://192.168.0.23:6677/wt/jibei";
    private static string _uid = "stu2";

    private static string _requestToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1c2VyRGF0YSI6eyJ1X25hbWUiOiJzdHUyIiwidV9pZCI6InN0dTIiLCJpZCI6IjExMDQiLCJyX2lkIjozfSwiaXNzIjoic3R1MiIsImV4cCI6MTYxNTUyNjU4MH0.wHPYw0wKk50RbiYlD34x3NM1g3xAH_tXTv9PsTE5ApI";
#else
    private static string _urlBase;
    private static string _uid;

    private static string _requestToken;
#endif

    //can just be set once
    public static string UID
    {
        get { return _uid; }
        set
        {
            if (string.IsNullOrEmpty(_uid))
            {
                _uid = value;
            }
        }
    }

    public static string RequestToken
    {
        set
        {
            if (string.IsNullOrEmpty(_requestToken))
            {
                _requestToken = value;
            }
        }
        get { return _requestToken; }
    }
    //设置服务器地址 can just be set once
    public static void SetUrlBase(string urlbase)
    {
        if (string.IsNullOrEmpty(_urlBase))
        {
            _urlBase = urlbase;
        }
    }
    //获取--掌机---工单概要地址
    public static string AppSumm()
    {
        return LowerStr(_urlBase + "/close/wonder/restapi/zhangji/getAppSumm" + AddParams(new Dictionary<string, string>() { { "uid", UID },{ "realOrVirtual","1" } }));
    }
    //获取--掌机---工单详情地址
    public static string AppDetail(string appnode, string apptype)
    {
        return LowerStr(_urlBase + "/close/wonder/restapi/zhangji/getAppDetail" +
                        AddParams(new Dictionary<string, string>()
                            {{"uid", UID}, {"appNos", appnode}, {"appType", apptype},{ "realOrVirtual","1" }}));
    }
    //你如果只传“terminalID”，那就是会给你集中器下所有电能表的当前和冻结数据；你如果传“terminalID”和"measureIndex"，就会给你集中器下当前电能表的当前和冻结数据；你如果三个字段都传，那就是给你集中器下，当前电能表的当前或者冻结数据
    public static string MeterData(string terminalID, string measureIndex)
    {
        Dictionary<string,string> p=new Dictionary<string, string>();
        if (string.IsNullOrEmpty(terminalID))
        {
        }
        else
        {
            p.Add("terminalID",terminalID);
            if (!string.IsNullOrEmpty(measureIndex))
            {
                p.Add("measureIndex",measureIndex);
            }
        }
        return LowerStr(_urlBase + "/close/wonder/restapi/virtualTemplate/callCalData" + AddParams(p));
    }

    public static string CallCalDataByFzu(string fZGID, string fZUNoInFZG)
    {
        Dictionary<string, string> p = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(fZGID)&&!string.IsNullOrEmpty(fZUNoInFZG))
        {
           p.Add("fZGID",fZGID);
           p.Add("fZUNoInFZG", fZUNoInFZG);
        }
        return LowerStr(_urlBase + "/close/wonder/restapi/virtualTemplate/callCalDataByFzu" + AddParams(p));
    }

    //查询虚拟仿真柜概要(到仿真单元一层)
    public static string QueryFzgOutLine()
    {
        return LowerStr(_urlBase+ "/op/virtualSimulation/queryFzgOutLine" + AddParams(new Dictionary<string, string>() { { "uid", UID }}));
    }
    //查询虚拟仿真柜挂接设备及故障(仿真单元下的设备)
    public static string QueryEquips(string fzgid, string fzuno)
    {
        return LowerStr(_urlBase+ "/op/virtualSimulation/queryEquips" + AddParams(new Dictionary<string, string>() {{ "fzgid",fzgid },{ "fzuno",fzuno} }));
    }
    //获取--掌机---故障列表
    public static string FaultReasonList()
    {
        return LowerStr(_urlBase + "/close/wonder/restapi/zhangji/getFaultReasonList"); 
    }

    public static string QueryFzgWorkOrder()
    {
        return LowerStr(_urlBase + "/op/virtualSimulation/queryFzgWorkOrder" + AddParams(new Dictionary<string, string>() {{"uid", UID}}));
    }
    public static string SubmitAppResult()
    {
        return LowerStr(_urlBase + "/close/wonder/restapi/zhangji/feedbackArchive");
    }
    public static string LowerStr(string str)
    {
        if (!string.IsNullOrEmpty(str))
        {
            return str.Trim();
        }

        return "";
    }
    private static string AddParams(Dictionary<string, string> p)
    {
        if (p == null || p.Count == 0)
        {
            return "";
        }

        StringBuilder sb = new StringBuilder("?");
        foreach (var s in p)
        {
            sb.Append($"{s.Key}={s.Value}&");
        }

        sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }

}
