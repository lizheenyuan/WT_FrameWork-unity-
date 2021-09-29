using Assets.Scripts.WT_FrameWork.UIFramework.Base;
using Assets.Scripts.WT_FrameWork.UIFramework.UIPanel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.WT_FrameWork.UIFramework.PanelScripts
{
    public class NoticePanel : BasePanel
    {
        private Button yesButton, noButton, cancelButton;
        private Text titleText, contentText;
        // 显示 标题，内容，是否取消三种按钮，三个事件
        public static void Show(string title, string content, NoticeBtnType btnType, UnityAction onYesClick = null,
            UnityAction onNoClick = null, UnityAction onCancelClick = null)
        {
            if (UIManager.Instance.PeekPanel() != null &&
                UIManager.Instance.PeekPanel().gameObject.name.Contains("NoticePanel"))
            {
                //多次打开同一个Panel 解决方案1 在未关闭时放弃打开
                Debug.Log(UIManager.Instance.PeekPanel().gameObject.name + "is Show,close it and open new.");
                //  return;
                //多次打开同一个Panel 解决方案2 关闭当前打开再打开 暂时测试此方法效果较好
                UIManager.Instance.PopPanel();
            }
            NoticePanel nPanel = UIManager.Instance.PushPanel(UIPanelType.Notice) as NoticePanel;
            nPanel.ShowDialog(title, content, btnType, onYesClick, onNoClick, onCancelClick);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            //查找  
            FindComponent();
//            ClearBtnListener();
            RegistClose();
        }

        private void FindComponent()
        {
            yesButton = transform.Find("bg/Btns/YesButton").GetComponent<Button>();
            noButton = transform.Find("bg/Btns/NoButton").GetComponent<Button>();
            cancelButton = transform.Find("bg/Btns/CancelButton").GetComponent<Button>();
            titleText = transform.Find("bg/Title").GetComponent<Text>();
            contentText = transform.Find("bg/Content").GetComponent<Text>();
        }

        private void ClearBtnListener()
        {
            yesButton.onClick.RemoveAllListeners();
            noButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
        }

        private void RegistClose()
        {
            yesButton.onClick.AddListener(ClosePanel);
            noButton.onClick.AddListener(ClosePanel);
            cancelButton.onClick.AddListener(ClosePanel);
        }

        private void ShowDialog(string title, string content, NoticeBtnType btnType, UnityAction onYesClick,
            UnityAction onNoClick, UnityAction onCancelClick)
        {
            titleText.text = title;
            contentText.text = content;
            switch (btnType)
            {
                case NoticeBtnType.Yes:
                    yesButton.gameObject.SetActive(true);
                    noButton.gameObject.SetActive(false);
                    cancelButton.gameObject.SetActive(false);
                    if (onYesClick != null)
                    {
                        yesButton.onClick.AddListener(onYesClick);
                    }
                    break;
                case NoticeBtnType.YesNo:
                    yesButton.gameObject.SetActive(true);
                    noButton.gameObject.SetActive(true);
                    cancelButton.gameObject.SetActive(false);
                    if (onYesClick != null)
                    {
                        yesButton.onClick.AddListener(onYesClick);
                    }
                    if (onNoClick != null)
                    {
                        noButton.onClick.AddListener(onNoClick);
                    }

                    break;
                case NoticeBtnType.YesNoCancel:
                    yesButton.gameObject.SetActive(true);
                    noButton.gameObject.SetActive(true);
                    cancelButton.gameObject.SetActive(true);
                    if (onYesClick != null)
                    {
                        yesButton.onClick.AddListener(onYesClick);
                    }
                    if (onNoClick != null)
                    {
                        noButton.onClick.AddListener(onNoClick);
                    }
                    if (onCancelClick != null)
                    {
                        cancelButton.onClick.AddListener(onCancelClick);
                    }
                    break;
                default:
                    break;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            yesButton.gameObject.SetActive(true);
            noButton.gameObject.SetActive(true);
            cancelButton.gameObject.SetActive(true);
            ClearBtnListener();
        }


        public enum NoticeBtnType
        {
            Yes,
            YesNo,
            YesNoCancel
        }
    }
}