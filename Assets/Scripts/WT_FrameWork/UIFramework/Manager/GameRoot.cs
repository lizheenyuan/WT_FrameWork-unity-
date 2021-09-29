using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Assets.Scripts.User;
using Assets.Scripts.WT_FrameWork.LoadSource;
using Assets.Scripts.WT_FrameWork.Managers;
using Assets.Scripts.WT_FrameWork.MSGCenter;
using Assets.Scripts.WT_FrameWork.SceneManager;
using Assets.Scripts.WT_FrameWork.SingleTon;
using Assets.Scripts.WT_FrameWork.UIFramework.UIPanel;
using Assets.Scripts.WT_FrameWork.Util;
using UnityEngine;
using Assets.Scripts.UIFramework.Manager;
using Assets.Scripts.WT_FrameWork.UIFramework.PanelScripts;
using DG.Tweening;
using LitJson;
using UnityEngine.Networking;
using UnityEngine.UI;
using Object = System.Object;
using System.Linq;
using Assets.Scripts.WT_FrameWork.Data;

namespace Assets.Scripts.WT_FrameWork.UIFramework.Manager
{
    public class GameRoot : MonoBehaviour
    {
        [SerializeField]
        public string FirstLoadScene;
        [SerializeField]
        public string FirstLoadPanel;
        public UserBase MUser;
        private static GameObject _rootObj;
        private static List<Action> _singletonReleaseList = new List<Action>();

        private static WT_SceneManager _scenemanager;
        private static WT_AudioManager _audioManager;
        private static CEventDispatcher _eventDispatcher;
        private static WT_DownLoader _downLoader;
        private static WT_VideoManager _videoManager;
        private static WT_CollecterManager _collecterManager;
        private static UserManager _usermanager;
        public static WT_SceneManager Scenemanager
        {
            get
            {
                if (_scenemanager == null)
                {
                    _scenemanager = GameRoot.GetSingleton<WT_SceneManager>();
                }
                return _scenemanager;
            }
        }

        /* public static WT_AudioManager AudioManager
         {
             get
             {
                 if (_audioManager == null)
                 {
                     _audioManager = GameRoot.GetSingleton<WT_AudioManager>();
                 }
                 return _audioManager;
             }
         }
         */
        public static CEventDispatcher EventDispatcher
        {
            get
            {
                if (_eventDispatcher == null)
                {
                    _eventDispatcher = GameRoot.GetSingleton<CEventDispatcher>();
                }
                return _eventDispatcher;
            }
        }

        public static WT_DownLoader DownLoader
        {
            get
            {
                if (_downLoader == null)
                {
                    _downLoader = GameRoot.GetSingleton<WT_DownLoader>();
                }
                return _downLoader;
            }
        }

        public static WT_VideoManager VideoManager
        {
            get
            {
                if (_videoManager == null)
                {
                    _videoManager = GameRoot.GetSingleton<WT_VideoManager>();
                }
                return _videoManager;
            }
        }

        public static WT_AudioManager AudioManager
        {
            get
            {
                if (_audioManager == null)
                {
                    _audioManager = GameRoot.GetSingleton<WT_AudioManager>();
                }
                return _audioManager;
            }
        }


        public static WT_CollecterManager CollecterManager
        {
            get
            {
                if (_collecterManager == null)
                {
                    _collecterManager = GameRoot.GetSingleton<WT_CollecterManager>();
                }
                return _collecterManager;
            }
        }

        public static UserManager M_Usermanager
        {
            get
            {
                if (_usermanager == null)
                {
                    _usermanager = UserManager.GetInstance();
                }
                return _usermanager;
            }
        }
        public static BDFZ_ServerMsg NetMsg_Manager { get; private set; }

        /// <summary>
        /// 引导模式标记
        /// </summary>
        public static bool IsGuideMode { get; private set; }
#if !UNITY_WEBGL
        public static string mechaineCode = "";
        // private WT_SuperDog wt_dog;
#endif


        [DllImport("__Internal")]
        private static extern string getQueryVariable(string defaultValue);

        public static string GetQueryVariable(string str = "userId")
        {
            return getQueryVariable(str);
        }
        // Use this for initialization
        private void Awake()
        {
            //        UIManager.Instance.PushPanel(UIPanelType.Welcome);
            _rootObj = this.gameObject;
        }
        private void Start()
        {
            IsGuideMode = true;//todo 一時
            //            TestRules();return;
            InitServerInfo();
            DontDestroyOnLoad(transform.parent.gameObject);//加载不删除框架
            /*新的初次加载部分*/
            GameObject fp = Instantiate(Resources.Load("UI/FirstPanel"), transform.parent.Find("Canvas")) as GameObject;
            FirstPanel flp = fp.GetComponent<FirstPanel>();
            flp.OnFirstPanelLoad += (f) =>
            {
                f.S_Progress.value = 0;
                StartCoroutine(OnFPload(flp));
            };

            //GameRoot.AudioManager.Play("Audio/dev_start.ogg");
            //            if (!wt_dog.LoginDog(ref mechaineCode))
            //            {
            //#if !UNITY_EDITOR
            //                FirstLoadPanel = UIPanelType.WTDog;
            //#endif
            //            }

            /* StartCoroutine(_downLoader.LoadAssetBundle((ab)=>
             {
                 UIManager.Instance.InitUI(ab);
                 DoLoadFirstScene();
             }, "http://192.168.1.205:8901/AB_Win/uipanel.unity3d"));
             */
        }

        void InitServerInfo()
        {
#if UNITY_WEBGL&&!UNITY_EDITOR
            try
            {
                ServerUrl.SetUrlBase(GetQueryVariable("serverIp"));
                Debug.Log("ServerUrlBase:" +GetQueryVariable("serverIp"));
                ServerUrl.UID = GetQueryVariable("userId");
                Debug.Log("ServerUID:" + ServerUrl.UID);
                ServerUrl.RequestToken = GetQueryVariable("token");
                Debug.Log("ServerToken:" + ServerUrl.RequestToken);
                Util.Util.ResPathRoot = GetQueryVariable("resourceIp");
                Debug.Log("ResPathRoot:" + Util.Util.ResPathRoot);
            }
            catch (Exception e)
            {
                Debug.Log("Can`t get configs..."+e.StackTrace);
            }
#endif

        }

        IEnumerator OnFPload(FirstPanel flp)
        {
            Tweener st = flp.S_Progress.DOValue(0.1f, 0.5f);
            st.OnStart(() =>
                {
                    flp.T_Progress.text = "加载必要组件";
                    // Loom.Initialize();//初始化Loom
                    //global::WT_FrameWork.Log.Instance.Init();
                    _audioManager = GameRoot.GetSingleton<WT_AudioManager>();
                    _eventDispatcher = GameRoot.GetSingleton<CEventDispatcher>();
                    //创建BDFZ_ServerMsg
                    NetMsg_Manager = new GameObject("NetMsg_Manager").AddComponent<BDFZ_ServerMsg>();
                    NetMsg_Manager.transform.parent = transform.parent;
                    //global::WT_FrameWork.Log.Instance.Init();
                    _downLoader = GameRoot.GetSingleton<WT_DownLoader>();
#if !UNITY_WEBGL
                    _videoManager = GameRoot.GetSingleton<WT_VideoManager>();
                    _collecterManager = GameRoot.GetSingleton<WT_CollecterManager>();
                    // wt_dog = new WT_SuperDog();
#endif
                    _scenemanager = GameRoot.GetSingleton<WT_SceneManager>(); //SceneManager加载main场景


                }
            );
            yield return st.IsComplete();
            //加载配置文件
            //yield return Util.Util.LoadConfigFromFile();
            flp.S_Progress.value += 0.2f;
           
            yield return _downLoader.LoadAssetBundle((ab) =>
            {
            }, Util.Util.BundleRootPath + "wt_font.unity3d");
            yield return _downLoader.LoadAssetBundle((ab) =>
            {
            }, Util.Util.BundleRootPath + "shaders.unity3d");
            yield return _downLoader.LoadAssetBundle((ab) =>
            {
                UIManager.Instance.InitUI(ab);
            }, Util.Util.UIBundlePath);
            //yield return _downLoader.LoadAssetBundle((ab) =>
            //{

            //}, Util.Util.BundleRootPath + "wtzj.unity3d");
            flp.S_Progress.value += 0.2f;
            yield return LoadSence(flp);
            flp.S_Progress.value = 1f;
            flp.gameObject.SetActive(false);

        }
        private AsyncOperation async;
        private IEnumerator LoadSence(FirstPanel flp)
        {
            AssetBundle bundle = null;
            using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(Util.Util.BundleRootPath+FirstLoadScene+"scene.unity3d"))
            {
                var bundleRequest = uwr.SendWebRequest();

                while ((!uwr.isDone))
                {
                    flp.S_Progress.value = bundleRequest.progress * 0.9f;
                    flp.T_Progress.text = "Loading Scene: " + (flp.S_Progress.value * 90).ToString("#00.00") + "%";
                    if (uwr.isNetworkError || uwr.isHttpError)
                    {
                        Debug.Log(uwr.error);
                        yield break;
                    }

                    yield return null;

                }

                // Get downloaded asset bundle
                bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                GameRoot.Scenemanager.SceneBundle = bundle;
                async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
                    Path.Combine(@"Assets/Scenes/" + FirstLoadScene +
                                 ".unity"));
                async.allowSceneActivation = false;
                while (!async.isDone == true)
                {
                    if ((async.progress <= 0.9f))
                    {
                        flp.S_Progress.value += async.progress * 0.1f;
                        flp.T_Progress.text = "Loading Scene: " + (flp.S_Progress.value * 100).ToString("#00.00") + "%";
                    }



                    //场景加载完毕 还未跳转
                    if (async.progress >= 0.9f && !async.isDone)
                    {
                        //yield return DownLoader.LoadAsset((data) =>
                        //    {
                        //        print(Encoding.UTF8.GetString(data));
                        //    }, Util.Util.ResPathRoot + "/1.txt");
                        //跳转
                        async.allowSceneActivation = true;
                        // yield return _downLoader.LoadAssetFromBundle<Sprite>("wtzj.unity3d", "JB_BDFZ", (s) => { });
                        UIManager.Instance.PushPanel(FirstLoadPanel.ToString());
                    }
                    yield return null;
                }
            }
            //async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(Path.Combine(@"Assets/scenes/" + FirstLoadScene + ".unity"));
            //async.allowSceneActivation = false;
            //while (!async.isDone  == true)
            //{
            //    if ((async.progress <= 0.9f))
            //    {
            //        flp.S_Progress.value += async.progress*0.5f;
            //        flp.T_Progress.text = "Loading Scene: " + (flp.S_Progress.value * 100).ToString("#00.00") + "%";
            //    }

            //    if (async.progress >= 0.9f && !async.isDone)
            //    {
            //        async.allowSceneActivation = true;
            //    }
            //    yield return null;
            //}
            //UIManager.Instance.PushPanel(FirstLoadPanel);
        }
        //public  void DoLoadFirstScene()
        //{
        //    Scenemanager.LoadScene(FirstLoadScene, () => { UIManager.Instance.PushPanel("Video",1,new Object[]{1,2,3}); });
        //}

        ////在第一次load场景中加载资源
        //private void OnFirstSceneUnload()
        //{

        //}


        private static T AddSingleton<T>() where T : WT_Mono_Singleton<T>
        {
            if (_rootObj.GetComponent<T>() == null)
            {
                string path = typeof(T).Name;
                T t = _rootObj.transform.parent.Find(path).gameObject.AddComponent<T>();
                t.SetInstance(t);
                t.Init();

                _singletonReleaseList.Add(delegate ()
                {
                    t.Release();
                });
                return t;
            }
            return null;
        }
        public static T GetSingleton<T>() where T : WT_Mono_Singleton<T>
        {
            T t = _rootObj.GetComponent<T>();

            if (t == null)
            {
                t = _rootObj.transform.parent.GetComponentInChildren<T>();
                if (t == null)
                {
                    t = AddSingleton<T>();
                }
            }
            return t;
        }
        private void Update()
        {
            ReturnBack();

        }



#if UNITY_EDITOR

        private void OnDestroy()
        {
            foreach (var singletonAction in _singletonReleaseList)
            {
                singletonAction();
            }
        }

#else
        void OnApplicationQuit()
        {
            PortManager.GetInstance().UnInit();
            //wt_dog?.LogOut();
            foreach (var singletonAction in _singletonReleaseList)
            {
                singletonAction();
            }
        }
#endif


        private void ReturnBack()
        {

#if UNITY_ANDROID || UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                //            if (UIManager.Instance.PopPanel().transform.name.Contains("Main"))
                //            {
                //                UIManager.Instance.PushPanel(UIPanelType.Main);
                //               // UIManager.Instance.PushPanel(UIPanelType.Exit);
                //            }
                Application.Quit();
            }
#endif
        }

   
    }
}