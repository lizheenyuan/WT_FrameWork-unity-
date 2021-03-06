using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.WT_FrameWork.MSGCenter;
using Assets.Scripts.WT_FrameWork.UIFramework.Manager;
using UnityEngine;
using UnityEngine.UI;

public class DevItem1069 : DevItemBase {

    protected override void AddStatesListener()
    {
        base.AddStatesListener();
        AddMsg();
        GameRoot.EventDispatcher.AddEventListener("msg_cur_" + dev.DevName + "_fault_all", OnGetFaultState);
        GameRoot.EventDispatcher.AddEventListener("msg_" + dev.DevName + "_readsend",OnGetReadSend);
    }
    private Text t_rec, t_send;
    private Scrollbar sr_rec, sr_send;
    void AddMsg()
    {
        Button btn_showText = transform.Find("btn_showText").GetComponent<Button>();
        CanvasGroup cg = transform.Find("bg").GetComponent<CanvasGroup>();
        cg.alpha = 0;
        btn_showText.onClick.AddListener(() => { cg.alpha = cg.alpha > 0 ? 0 : 1; });
        GameRoot.EventDispatcher.AddEventListener("msg_" + dev.DevName + "_readsend", OnGetReadSend);
        t_rec = cg.transform.Find("sv_rec/Viewport/Content").GetComponent<Text>();
        t_send = cg.transform.Find("sv_send/Viewport/Content").GetComponent<Text>();
        sr_rec = cg.transform.Find("sv_rec/Scrollbar Vertical").GetComponent<Scrollbar>();
        sr_send = cg.transform.Find("sv_send/Scrollbar Vertical").GetComponent<Scrollbar>();
    }
    private void OnGetReadSend(CBaseEvent cet)
    {
        if (t_rec.text.Length > 2048)
        {
            t_rec.text = "";
        }

        if (t_send.text.Length > 2048)
        {
            t_send.text = "";
        }
        if ((int)(cet.Argments["flag"]) == 0)
        {
            t_rec.text += cet.Argments["strdata"] + "\n";
        }
        else
        {
            t_send.text += cet.Argments["strdata"] + "\n";
        }

        sr_rec.value = 0;
        sr_send.value = 0;
    }
    private void OnGetFaultState(CBaseEvent cet)
    {
        bool[] faults = (bool[])cet.Argments["all_fault"];
        for (int i = 0; i < faults.Length; i++)
        {
            SetStateColor(items[i+1],faults[i]);
        }
    }
}
