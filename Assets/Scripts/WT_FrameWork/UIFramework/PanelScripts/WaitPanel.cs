using Assets.Scripts.WT_FrameWork.MSGCenter.MsgDefine;
using Assets.Scripts.WT_FrameWork.UIFramework.Base;
using Assets.Scripts.WT_FrameWork.UIFramework.Manager;
using Assets.Scripts.WT_FrameWork.UIFramework.UIPanel;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.WT_FrameWork.UIFramework.PanelScripts
{
    public class WaitPanel : BasePanel
    {
        private Image rollImg;
        private Button cancelButton;
        private Text contentText;
        // 显示 标题，内容，是否取消三种按钮，三个事件
        public static void Show(string content, bool IsWait)
        {
            if (UIManager.Instance.PeekPanel() != null &&
                UIManager.Instance.PeekPanel().gameObject.name.Contains("WaitPanel"))
            {
                //多次打开同一个Panel 解决方案1 在未关闭时放弃打开
                Debug.Log(UIManager.Instance.PeekPanel().gameObject.name + "is Show,close it and open new.");
                //  return;
                //多次打开同一个Panel 解决方案2 关闭当前打开再打开 暂时测试此方法效果较好
                UIManager.Instance.PopPanel();
            }
            WaitPanel nPanel = UIManager.Instance.PushPanel("Wait") as WaitPanel;
            nPanel.ShowDialog(content, IsWait);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            //查找  
            FindComponent();
            //            ClearBtnListener();
            RegistClose();
        }
        private bool IsWait = false;
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (IsWait)
            {
                rollImg.transform.Rotate(new Vector3(0, 0, -30) * Time.deltaTime);
                rollImg.gameObject.SetActive(true);
            }
        }

        private void FindComponent()
        {
            cancelButton = transform.Find("bg/Btns/CancelButton").GetComponent<Button>();
            contentText = transform.Find("bg/Content").GetComponent<Text>();
            rollImg = transform.Find("bg/RollImg").GetComponent<Image>();
        }

        private void ClearBtnListener()
        {
            cancelButton.onClick.RemoveAllListeners();
        }

        private void RegistClose()
        {
            cancelButton.onClick.AddListener(CancelBtnClick);
        }

        private void CancelBtnClick()
        {
            //取消加载数据
            //DispatchEvent(WT_Msg.msg_StopLoadData, null, null);
            ClosePanel();
        }

        private void ShowDialog(string content, bool isWait)
        {
            IsWait = isWait;
            if (IsWait == false)
            {
                ClosePanel();
            }

            contentText.text = content;

        }

        public override void OnExit()
        {
            base.OnExit();
            cancelButton.gameObject.SetActive(true);
            ClearBtnListener();
        }
    }
}