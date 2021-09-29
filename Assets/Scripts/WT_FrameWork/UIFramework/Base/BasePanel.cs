using System.IO;
using Assets.Scripts.WT_FrameWork.MSGCenter;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.WT_FrameWork.UIFramework.Base
{
///所有面板的公共基类
/// <summary>
/// OnEnter->FindObject->InitDB->RigistEvent
/// </summary>
    public class BasePanel : CMsgObjBase
    {
        [SerializeField] protected bool isActive;
        protected CanvasGroup canvasGroup; //获取CanvasGroup组件，用于控制该面板的交互功能
        protected object Arg;
        protected object[] Args;
        private void Awake()
        {            
            canvasGroup = transform.GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// 页面进入显示，可交互
        /// </summary>
        public virtual void OnEnter()
        {
            if (isActive)
            {
                return;
            }
            if (canvasGroup == null)
            {
                canvasGroup = transform.GetComponent<CanvasGroup>();
            }
            this.canvasGroup.alpha = 1;
            this.isActive = true;
            this.canvasGroup.blocksRaycasts = true;
            this.canvasGroup.interactable = true;
            FindObject();
            InitDB();
            RigistEvent();
        }

        /// <summary>
        /// 页面暂停（弹出了其他页面），不可交互
        /// </summary>
        public virtual void OnPause()
        {
            if (!canvasGroup)
            {
                return;
            }
            isActive = false;
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false; //使该面板失去交互功能
            canvasGroup.interactable = false;
        }

        /// <summary>
        /// 页面继续显示（其他页面关闭），可交互
        /// </summary>
        public virtual void OnResume()
        {
            if (canvasGroup == null)
            {
                return;
            }
            isActive = true;
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true; //使该面板继续交互功能
            canvasGroup.interactable = true;
        }

        /// <summary>
        /// 本页面被关闭（移除），不再显示在界面上
        /// </summary>
        public virtual void OnExit()
        {
            if (canvasGroup == null)
            {
                return;
            }
            isActive = false;
            this.canvasGroup.alpha = 0;
            this.canvasGroup.blocksRaycasts = false;
            this.canvasGroup.interactable = false;
            UnRigistEvent();
        }

        private void Update()
        {
            if (isActive)
            {
                OnUpdate();
            }

        }

        public virtual void OnUpdate()
        {

        }

        public virtual void ClosePanel()
        {
            UIManager.Instance.PopPanel();
        }

        protected T GetComponent<T>(string path) where T : MonoBehaviour
        {
            Transform t = transform.Find(path);
            if (t!=null)
            {
                return t.GetComponent<T>();
            }
            else
            {
                return null;
            }
        }
        protected virtual void FindObject(){}
        protected virtual void InitDB(){}
        protected virtual void RigistEvent(){}

        protected virtual void UnRigistEvent()
        {
        }

        #region 注册方法

        protected void RigistBtnEvent(Button btn, UnityAction a)
        {
            btn?.onClick.AddListener(a);
        }
        //a ==null remove all
        protected void UnRigistBtnEvent(Button btn, UnityAction a=null)
        {
            if (a==null)
            {
                btn?.onClick.RemoveAllListeners();
            }
            else
            {
                btn?.onClick.RemoveListener(a);
            }
            
        }

        #endregion
    }
}