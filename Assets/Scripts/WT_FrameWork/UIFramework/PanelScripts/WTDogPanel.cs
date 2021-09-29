#if !UNITY_WEBGL

using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.WT_FrameWork;
using Assets.Scripts.WT_FrameWork.UIFramework.Base;
using Assets.Scripts.WT_FrameWork.UIFramework.Manager;
using UnityEngine;
using UnityEngine.UI;

public class WTDogPanel : BasePanel
{

    private Button btn_quit;
    private InputField ipf;
    public override void OnEnter()
    {
        base.OnEnter();
        btn_quit = transform.Find("msg/btn_quit").GetComponent<Button>();
        ipf = transform.Find("msg/InputField").GetComponent<InputField>();
        ipf.text = GameRoot.mechaineCode;
        btn_quit.onClick.AddListener(OnBtn_QuitClick);
    }
    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.KeypadDivide))
        //{
        //    UIManager.Instance.PushPanel(?);
        //}
    }
    private void OnBtn_QuitClick()
    {
        Application.Quit();
    }
    public override void OnExit()
    {
        base.OnExit();
        btn_quit.onClick.RemoveAllListeners();
    }
}

#endif