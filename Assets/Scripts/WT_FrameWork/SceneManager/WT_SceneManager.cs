using System;
using System.Collections;
using System.IO;
using Assets.Scripts.WT_FrameWork.SingleTon;
using Assets.Scripts.WT_FrameWork.UIFramework.Manager;
using Assets.Scripts.WT_FrameWork.UIFramework.UIPanel;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.WT_FrameWork.SceneManager
{
    public class WT_SceneManager : WT_Mono_Singleton<WT_SceneManager>
    {
        private string _next_sceneName;

        /// <summary>
        /// 调用后会自动清空
        /// </summary>
        public Action On_WTSceneLoaded;

        /// <summary>
        /// 调用后会自动清空
        /// </summary>
        public Action On_WTSceneUnloaded;

        /// <summary>
        /// 在加载完成loadscene时
        /// 不自动清空使用时需要注意
        /// WT_Downloader注册了卸载AB包的事件
        /// </summary>
        public Action On_WTSceneLoading;

        public AssetBundle SceneBundle;
        public string SceneBundlePath;

        public string NextSceneName => _next_sceneName;

        public override void Init()
        {
            base.Init();
            //_next_sceneName = SceneManager.GetSceneByBuildIndex(1).name;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoading;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnLoaded;
        }

        public string GetLoadSceneName()
        {
            if (string.IsNullOrEmpty(_next_sceneName))
            {
                _next_sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            }
            return _next_sceneName;
        }


        private void OnSceneUnLoaded(Scene s)
        {
            if (s.name.Contains("Load"))
            {
                UIManager.Instance.PopPanel();
            }
            else
            {
                if (On_WTSceneUnloaded != null)
                {
                    On_WTSceneUnloaded();
                    On_WTSceneUnloaded = null;
                }
            }
        }
        void OnSceneLoading(Scene s, LoadSceneMode lsm)
        {
            if (s.name.Contains("Load"))
            {
                On_WTSceneLoading?.Invoke();
            }
        }
        private void OnSceneLoaded(Scene s, LoadSceneMode lsm)
        {
            if (s.name.Contains("Load"))
            {
                
                UIManager.Instance.PushPanel("Load");
            }
            if (!s.name.Contains("Load") && On_WTSceneLoaded != null)
            {
                On_WTSceneLoaded();
                On_WTSceneLoaded = null;
//                try
//                {
//                    SceneBundle?.Unload(true);
//                }
//                catch (Exception e)
//                {
//                    Debug.LogError("No SceneBundle to unload.");
//                }
            }
        }
    /// <summary>
    /// 所有场景应放在 Scenes文件夹下 命名规则如下：
    /// sceneName为具体的场景名，填写bundle名时为sceneName.tolower()+scene.unity3d
    /// 如 main 场景 文件main.unity  包名：mainscene.unity3d
    /// </summary>
    /// <param name="sceneName"></param>
        public void LoadScene(string sceneName)
        {
            //StartCoroutine(AsyncLoadScene(sceneName));
            _next_sceneName = sceneName;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Load");
            //return true;
        }

        public void LoadSceneFromBundle(string bundlename)
        {
            
        }
        IEnumerator AsyncLoadScene(string sceneName, Action<bool> onLoaded = null)
        {
            if (!UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName).IsValid())
            {
                _next_sceneName = sceneName;
                //yield return GameRoot.DownLoader.LoadSceneAssetBundle((ab) => { SceneBundle = ab;}, _bundlePath);

            }
            else
            {
                print(string.Format("没有{0}场景.", sceneName));
                onLoaded?.Invoke(false);
                yield break;
            }

        }
        public void LoadScene(string sceneName, Action onloadedScene)
        {
            On_WTSceneLoaded += onloadedScene;
            LoadScene(sceneName);
        }
    }
}