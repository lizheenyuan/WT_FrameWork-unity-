using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.WT_FrameWork.MSGCenter.Interface
{
    public interface ICmessage
    {
        //暂时定义： 
        CBaseEvent CBE { get; }
        //暂时定义： 第一个参数为一个参数 第二个参数为数组类型   hashtable中参数名分别为arg、args
        void AddListenEvent(string eventName,Action<object ,object[]> eAction);
        void RemoveListenEvent(string eventName);
        void DispatchEvent(string eventname, object arg,object[] args);
        int RemoveAllEvent();
    }
}
