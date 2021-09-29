using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.WT_FrameWork.MSGCenter;
using Assets.Scripts.WT_FrameWork.Protocol;
using Assets.Scripts.WT_FrameWork.UIFramework.Manager;
using UnityEngine;
using UnityEngine.UI;

public class DevItemBase : MonoBehaviour
{
    protected Text nameText, ipporText;
    protected Collecter dev;
    protected Image devState;

    public Image[] items;

    //void Start()
    //{
    //    print(name);
    //}

    public virtual void LoadDev(WTClientSocket c)
    {
        if (c!=null)
        {
            dev = c as  Collecter;
        }
        nameText = transform.Find("dev_name").GetComponent<Text>();
        ipporText = transform.Find("ipport").GetComponent<Text>();
        devState = transform.Find("connect_state").GetComponent<Image>();
        items = transform.Find("states").GetComponentsInChildren<Image>();
        nameText.text = c.DevName;
        ipporText.text = c.ServerIPAddress + ":" + c.ServerPort;
        AddStatesListener();
    }

    protected void SetStateColor(Image img,bool isGreen)
    {
        img.color = isGreen ? Color.green : Color.red;
    }

    /// <summary>
    /// 添加颜色变化和连接状态变化
    /// "msg_" + DevName + "_statechanged"
    /// </summary>
    protected virtual void AddStatesListener()
    {
        GameRoot.EventDispatcher.AddEventListener("msg_" + dev.DevName + "_statechanged", OnConStateChanged);
    }

    protected void OnConStateChanged(CBaseEvent cet)
    {
        SetStateColor(devState, (bool)cet.Argments["constate"]);
    }
}
