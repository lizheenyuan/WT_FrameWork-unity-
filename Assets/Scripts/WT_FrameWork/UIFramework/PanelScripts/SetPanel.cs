using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.WT_FrameWork;
using Assets.Scripts.WT_FrameWork.UIFramework.Base;
using Assets.Scripts.WT_FrameWork.UIFramework.Manager;
using Assets.Scripts.WT_FrameWork.Util;
using UnityEngine;
using UnityEngine.UI;

public class SetPanel : BasePanel
{
    private InputField rd1069_1_ip,rd1051_ip,rd1069_2_ip;
    private InputField rd1069_1_port,rd1051_port,rd1069_2_port;
    Text rd1069_1_state, rd1051_state, rd1069_2_state;
    private Button btn_save, btn_quit;
    public override void OnEnter()
    {
        base.OnEnter();
        rd1069_1_ip = transform.Find("content/bg/rd1069_1/ip/InputField").GetComponent<InputField>();
        rd1069_1_port= transform.Find("content/bg/rd1069_1/port/InputField").GetComponent<InputField>();
        rd1069_1_state= transform.Find("content/bg/rd1069_1/state/Text").GetComponent<Text>();

        rd1069_2_ip = transform.Find("content/bg/rd1069_2/ip/InputField").GetComponent<InputField>();
        rd1069_2_port = transform.Find("content/bg/rd1069_2/port/InputField").GetComponent<InputField>();
        rd1069_2_state = transform.Find("content/bg/rd1069_2/state/Text").GetComponent<Text>();

        rd1051_ip = transform.Find("content/bg/rd1051/ip/InputField").GetComponent<InputField>();
        rd1051_port = transform.Find("content/bg/rd1051/port/InputField").GetComponent<InputField>();
        rd1051_state = transform.Find("content/bg/rd1051/state/Text").GetComponent<Text>();

        btn_quit = transform.Find("down/btn_quit").GetComponent<Button>();
        btn_save = transform.Find("down/btn_save").GetComponent<Button>();
        btn_save.onClick.AddListener(SaveConfig);
        btn_quit.onClick.AddListener(ClosePanel);
        LoadConfig();

    }
    public override void OnUpdate()
    {
        base.OnUpdate();
        UpdateDevState();
    }
    public override void OnExit()
    {
        base.OnExit();
        btn_save.onClick.RemoveListener(SaveConfig);
        btn_quit.onClick.RemoveListener(ClosePanel);
    }
    private void LoadConfig()
    {
        rd1069_1_ip.text = Util.GetSystemConfig("RD1069_1","IP");
        rd1069_1_port.text = Util.GetSystemConfig("RD1069_1", "Port");

        rd1069_2_ip.text = Util.GetSystemConfig("RD1069_2", "IP");
        rd1069_2_port.text = Util.GetSystemConfig("RD1069_2", "Port");

        rd1051_ip.text = Util.GetSystemConfig("RD1051", "IP");
        rd1051_port.text = Util.GetSystemConfig("RD1051", "Port");
    }

    private void SaveConfig()
    {
        Util.SetSystemConfig("RD1069_1", "IP", rd1069_1_ip.text);
        Util.SetSystemConfig("RD1069_1", "Port", rd1069_1_port.text);

        Util.SetSystemConfig("RD1069_2", "IP", rd1069_2_ip.text);
        Util.SetSystemConfig("RD1069_2", "Port", rd1069_2_port.text);

        Util.SetSystemConfig("RD1051", "IP", rd1051_ip.text);
        Util.SetSystemConfig("RD1051", "Port", rd1051_port.text);

        Util.SaveSystemConfig();
        ClosePanel();
    }

    private void UpdateDevState()
    {
        //rd1069_1_state.text = GameRoot.CollecterManager.Rd1069_1.CtrlConnect
        //    ? Util.GetLangText("dev_connected")
        //    : Util.GetLangText("dev_disconnected");

        //rd1069_2_state.text = GameRoot.CollecterManager.Rd1069_2.CtrlConnect
        //    ? Util.GetLangText("dev_connected")
        //    : Util.GetLangText("dev_disconnected");

        //rd1051_state.text = GameRoot.CollecterManager.Rd1051.CtrlConnect
        //   ? Util.GetLangText("dev_connected")
        //   : Util.GetLangText("dev_disconnected");

    }
}
