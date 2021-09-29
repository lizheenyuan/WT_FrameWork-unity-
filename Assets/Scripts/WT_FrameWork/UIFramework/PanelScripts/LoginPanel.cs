using System;
using System.Collections;
using System.IO.Ports;
using Assets.Scripts.Protocol;
using Assets.Scripts.UIFramework.Manager;
using Assets.Scripts.User;
using Assets.Scripts.WT_FrameWork.LoadSource;
using Assets.Scripts.WT_FrameWork.UIFramework.Base;
using Assets.Scripts.WT_FrameWork.UIFramework.Manager;
using Assets.Scripts.WT_FrameWork.UIFramework.UIPanel;
using Assets.Scripts.WT_FrameWork.Util;
using LitJson;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.WT_FrameWork.UIFramework.PanelScripts
{
    public class LoginPanel : BasePanel
    {
        private InputField UserName;
        private InputField UserID;
        private Button StudentLogin;
        private Button VisitorLogin;
        private string COMPortNum;
        private InputField user_cardID;
        private WT_DownLoader loader;
        private int Resid;
        private UserBase user;

        public override void OnEnter()
        {
            base.OnEnter();
            StudentLogin = transform.Find("StudentLogin").GetComponent<Button>();
            VisitorLogin = transform.Find("VisitorLogin").GetComponent<Button>();
            StudentLogin.onClick.RemoveAllListeners();
            VisitorLogin.onClick.RemoveAllListeners();
            //StudentLogin.onClick.AddListener(StudentLog);//学生登录
            VisitorLogin.onClick.AddListener(VisitorLog); //游客登录
            UserName = transform.Find("NameText/InputField").GetComponent<InputField>();
            UserID = transform.Find("IDText/InputField").GetComponent<InputField>();
            COMPortNum = Util.Util.GetSystemConfig("PortConfig", "RFCardBox");
            StartCoroutine(TryOPenPort());
            PortManager.GetInstance().CardBox.onDevReadSendEvent += OnReadCard;
            loader = GameRoot.GetSingleton<WT_DownLoader>();
            loader.OnLoadedSuccess += Loader_OnLoadedSuccess;
            loader.OnLoadedFailed += Loader_OnLoadedFailed;
            //        RFCardBox.GetInstance().OpenPort(COMPortNum,SerialPortBaudRates.BaudRate_9600,Parity.None,SerialPortDatabits.EightBits,StopBits.One);
            //        RFCardBox.GetInstance().onDevReadSendEvent += OnReadCard;
        }

        private IEnumerator TryOPenPort()
        {
            yield return new WaitForSeconds(0.5f);
            try
            {
                PortManager.GetInstance()
                    .CardBox.OpenPort(COMPortNum, SerialPortBaudRates.BaudRate_9600, Parity.None,
                        SerialPortDatabits.EightBits, StopBits.One);
            }
            catch (Exception ex)
            {
                UIManager.Instance.PopPanel();
                NoticePanel.Show("提示", ex.Message, NoticePanel.NoticeBtnType.Yes, OverSoftWare, null, null);
            }
        }

        private void OverSoftWare()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
Application.Quit();
#endif
        }

        private void OnReadCard(int devid, string devname, int flagrs, string strrsdata)
        { }

        public override void OnPause()
        {
            base.OnPause();
            canvasGroup.alpha = 0.65f;
        }

        public override void OnExit()
        {
            loader.OnLoadedSuccess -= Loader_OnLoadedSuccess;
            loader.OnLoadedFailed -= Loader_OnLoadedFailed;
            // StudentLogin.onClick.RemoveListener(StudentLog);//学生登录
            VisitorLogin.onClick.RemoveListener(VisitorLog); //游客登录
            base.OnExit();
            // PortManager.GetInstance().CardBox.onDevReadSendEvent -= OnReadCard;
        }

        private void StudentLog()
        {
            //判断学生姓名和卡号是否存在，若存在，即切换界面
            if (UserID.text != "")
            {
                Resid =
                    loader.LoadTextFromUrl(Util.Util.GetSystemConfig("NetConfig", "Url_Login") +
                                           PortManager.GetInstance().CardBox.CardID);
            }


        }

        private void Loader_OnLoadedFailed(string obj, int id)
        {
            if (Resid != id)
            {
                return;
            }
            print(obj.ToString());

        }

        private void Loader_OnLoadedSuccess(object obj, int id)
        {
            if (Resid != id)
            {
                return;
            }
            if ((string) obj == "0")
            {
                NoticePanel.Show(Util.Util.GetLangText("LoginFailed"), Util.Util.GetLangText("LoginAgain"),
                    NoticePanel.NoticeBtnType.Yes, null, null, null);
            }
            else
            {
                JsonData ds = JsonMapper.ToObject(obj.ToString());
                // print("登录用户名字：" + ds["User_Name"]);
                //{"IC_CardID":"040c022000040095f3e8702f","User_Name":"\u674E\u5411\u9633","Score":0.0}
                user = new User_Student(UserType.Student, "1", ds["User_Name"].ToString(),
                    float.Parse(ds["Score"].ToString()), ds["IC_CardID"].ToString());
                NoticePanel.Show(Util.Util.GetLangText("LoginSuccess"), string.Format("欢迎{0}\n点击确认进入...", ds["User_Name"]),
                    NoticePanel.NoticeBtnType.Yes, ClickYes, null, null);
            }


        }

        private void ClickYes()
        {
            UserManager.GetInstance().SetCurrentUser(user);
            UIManager.Instance.PopPanel();
            UIManager.Instance.PushPanel(UIPanelType.Main);
        }

        private void VisitorLog()
        {
            user = new User_Visitor(UserType.Visitor, "0", "游客"); //"1"表示学员，"0"表示游客
            UserManager.GetInstance().SetCurrentUser(user);
            UIManager.Instance.PopPanel();
            UIManager.Instance.PushPanel(UIPanelType.Main);

        }
    }
}