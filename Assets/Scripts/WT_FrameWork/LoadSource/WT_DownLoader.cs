using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.WT_FrameWork.SingleTon;
using Assets.Scripts.WT_FrameWork.UIFramework.Manager;
using Assets.Scripts.WT_FrameWork.UIFramework.PanelScripts;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace Assets.Scripts.WT_FrameWork.LoadSource
{
    class ABInfo
    {
        public AssetBundle Bundle;
        public string BundleName;
        public Action<AssetBundle> OnBundleLoaded;

        public ABInfo(AssetBundle bundle, string bundleName)
        {
            Bundle = bundle;
            BundleName = bundleName;
        }
    }
    public class WT_DownLoader : WT_Mono_Singleton<WT_DownLoader>
    {
        private object _downloadData;
        //private WWW _www;
        private int downloadID;
        private Action<object, int> _onLoadedSuccess;
        private Action<string, int> _onLoadedFailed;
        public event Action<object, int> OnLoadedSuccess
        {
            add { _onLoadedSuccess += value; }
            remove
            {
                if (_onLoadedSuccess != null)
                {
                    _onLoadedSuccess -= value;
                }

            }
        }
        public event Action<string, int> OnLoadedFailed
        {
            add { _onLoadedFailed += value; }
            remove
            {
                if (_onLoadedFailed != null)
                {
                    _onLoadedFailed -= value;
                }

            }
        }
        private Dictionary<string, Texture> _tex_dic;

        private List<ABInfo> _loadedBundle;
        /// <summary>
        /// 不卸载包含此字符串的AB包，常用包不需要卸载（使用包含字符串的形式）
        /// 后期考虑改用正则表达式
        /// </summary>
        public static string AB_UnloadFilter="wt";
        public override void Init()
        {
            base.Init();
            downloadID = 1;
            _tex_dic = new Dictionary<string, Texture>();
            _loadedBundle = new List<ABInfo>();
            GameRoot.Scenemanager.On_WTSceneLoading += () =>
            {
                foreach (ABInfo abInfo in _loadedBundle)
                {
                    if (!abInfo.BundleName.ToLower().Contains(AB_UnloadFilter))
                    {
                        Debug.Log(abInfo.BundleName);
                        abInfo.Bundle.Unload(true);
                    }
                }
                _loadedBundle = _loadedBundle.FindAll(a => a.Bundle != null);
            };
        }
        [Obsolete]
        public void Download<T>(string url, SourceType stype, DownLoadType dtype)
        {
            switch (dtype)
            {
                case DownLoadType.Resource:
                    //_downloadData = (T)DownLoadFromResource(url);
                    break;
                case DownLoadType.AssetBoundle:
                    break;
                case DownLoadType.FileorNet:
                    break;
                    //default:
                    //    throw new ArgumentOutOfRangeException(nameof(dtype), dtype, null);
            }
        }
        /// <summary>
        /// 务必在+=后添加-=避免抛出异常 此方法不判断事件为空
        /// </summary>
        /// <param name="url">ResUrl</param>
        /// <returns>ResID</returns>
        [Obsolete]
        public int LoadTextFromUrl(string url)
        {
            StartCoroutine(LoadTextFromUrlCor(url, downloadID));
            return downloadID++;
        }
        /// <summary>
        /// 务必在+=后添加-=避免抛出异常 此方法不判断事件为空
        /// </summary>
        /// <param name="url">ResUrl</param>
        /// <returns>ResID</returns>
        public int LoadAudioFromUrl(string url)
        {
            StartCoroutine(LoadAudioClipFromUrlCor(url, downloadID));
            return downloadID++;
        }
        [Obsolete]
        public void LoadTextFromUrl(string url, Action<bool, string> onGetResult)
        {
            StartCoroutine(LoadTextFromUrlCor(url, onGetResult));
        }
        
        //使用streamingassets目录加载 不必添加streamingassets前缀直接填写文件名
        [Obsolete]
        public void LoadAudioFromUrl(string url, Action<string, bool, AudioClip> onGetResult)
        {
            StartCoroutine(LoadAudioClipFromUrlCor(url, onGetResult));
        }
        private IEnumerator LoadAudioClipFromUrlCor(string url, Action<string, bool, AudioClip> onGetResult)
        {
            using (var uwr = UnityWebRequestMultimedia.GetAudioClip(Util.Util.AudioPath + url, AudioType.OGGVORBIS))
            {
                yield return uwr.SendWebRequest();
                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    onGetResult(url, false, null);
                    Debug.LogError(uwr.error);
                    yield break;
                }

                AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);
                onGetResult(url, true, clip);
                // use audio clip
            }
            //WWW _www = new WWW(Util.Util.AddStreamAssetsHeader(url));
            //yield return _www;
            //if (!string.IsNullOrEmpty(_www.error))
            //{             
            //    onGetResult(url,false, null);
            //    Debug.Log(url+_www.error);
            //}
            //else
            //{
            //    if (_www.isDone)
            //    {
            //        onGetResult(url,true, _www.GetAudioClip());
            //    }
            //}
        }
        public void LoadTextureFromUrl(string url, Action<string, bool, Texture> onGetResult)
        {
            StartCoroutine(LoadTextureFromUrlCor(url, onGetResult));
        }
        private IEnumerator LoadTextureFromUrlCor(string url, Action<string, bool, Texture> onGetResult)
        {
            WWW _www = new WWW(Util.Util.AddStreamAssetsHeader(url));
            yield return _www;
            if (!string.IsNullOrEmpty(_www.error))
            {
                onGetResult(url, false, null);
            }
            else
            {
                if (_www.isDone)
                {
                    onGetResult(url, true, _www.texture);
                }
            }
        }

        private IEnumerator LoadTextFromUrlCor(string url, int id)
        {
            WWW _www = new WWW(url);
            yield return _www;
            if (!string.IsNullOrEmpty(_www.error))
            {
                _onLoadedFailed(_www.error, id);
            }
            else
            {
                if (_www.isDone)
                {
                    _onLoadedSuccess(_www.text, id);
                }
            }
        }
        private IEnumerator LoadTextFromUrlCor(string url, Action<bool, string> onGetResult)
        {
            WWW _www = new WWW(url);
            yield return _www;
            if (!string.IsNullOrEmpty(_www.error))
            {
                onGetResult(false, _www.error);
            }
            else
            {
                if (_www.isDone)
                {
                    onGetResult(true, _www.text);
                }
            }
        }
        private IEnumerator LoadAudioClipFromUrlCor(string url, int id)
        {
            WWW _www = new WWW(url);
            yield return _www;
            if (!string.IsNullOrEmpty(_www.error))
            {
                _onLoadedFailed(_www.error, id);
            }
            else
            {
                if (_www.isDone)
                {
                    _onLoadedSuccess(_www.GetAudioClip(), id);
                }
            }
        }
        T GetSubAsset<T>(AssetBundle ab, string objName) where T : Object
        {
            T[] ts = ab.LoadAssetWithSubAssets<T>(objName.Split('/')[0]);
            T obj = ts.FirstOrDefault(a => a.name == objName.Split('/')[1]);
            return obj;
        }
        /// <summary>
        /// 加载AB包中的资源   如果objname contains'/' 则为寻找name[0]/name[1]  etc:main_img/sprite0  寻找main_img 下的 sprite0
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName"></param>
        /// <param name="objName"></param>
        /// <param name="OnGetObj"></param>
        public Coroutine LoadAssetFromBundle<T>(string bundleName, string objName, Action<T> OnGetObj) where T : Object
        {
            var abinfo = _loadedBundle.FirstOrDefault(ab => ab.BundleName == bundleName);
            if (abinfo == null)
            {
                // WaitPanel.Show("资源加载中", true);
                abinfo = new ABInfo(null, bundleName);
                abinfo.OnBundleLoaded += (ab) =>
                {
                    // WaitPanel.Show("资源加载中", false);
                    if (objName.Contains('/'))
                    {

                        OnGetObj?.Invoke(GetSubAsset<T>(ab, objName));
                    }
                    else
                    {
                        OnGetObj?.Invoke(ab.LoadAsset<T>(objName));
                    }
                };
                _loadedBundle.Add(abinfo);
                return StartCoroutine(LoadAssetBundleByABInfo(abinfo));
            }
            else
            {
                if (abinfo.Bundle == null)
                {
                    abinfo.OnBundleLoaded += (ab) =>
                    {
                        if (objName.Contains('/'))
                        {

                            OnGetObj?.Invoke(GetSubAsset<T>(ab, objName));
                        }
                        else
                        {
                            OnGetObj?.Invoke(ab.LoadAsset<T>(objName));
                        }
                    };
                }
                else
                {
                    if (objName.Contains('/'))
                    {

                        OnGetObj?.Invoke(GetSubAsset<T>(abinfo.Bundle, objName));
                    }
                    else
                    {
                        OnGetObj?.Invoke(abinfo.Bundle.LoadAsset<T>(objName));
                    }
                }
            }
            return null;
        }

        IEnumerator LoadAssetBundleByABInfo(ABInfo abInfo)
        {
            AssetBundle bundle = null;
            using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(Util.Util.BundleRootPath + abInfo.BundleName))
            {
                yield return uwr.SendWebRequest();
                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.Log(uwr.error);
                }
                else
                {
                    bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    abInfo.Bundle = bundle;
                    abInfo.OnBundleLoaded?.Invoke(bundle);
                    abInfo.OnBundleLoaded = null;
                }
            }
        }
        public IEnumerator LoadAssetBundle(Action<AssetBundle> onBundleLoaded, string bundlePath)
        {
            AssetBundle bundle = null;

            using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(bundlePath))
            {
                yield return uwr.SendWebRequest();
                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.Log(bundlePath+":"+uwr.error);
                }
                else
                {
                    // Get downloaded asset bundle
                    bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    //_loadedBundle.Add(bundlePath,bundle);
                }
                onBundleLoaded(bundle);
                //bundle?.Unload(false);
            }
        }
        /// <summary>
        /// web get请求(建议统一使用LoadAsset(Action<byte[]> onAssetLoaded, string assetPath, WWWForm postdata, Dictionary<string, string> headers, DownMethodloadType dt = DownMethodloadType.Get))
        /// </summary>
        /// <param name="onAssetLoaded"></param>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        ///
        [Obsolete]
        public IEnumerator LoadAsset(Action<byte[]> onAssetLoaded, string assetPath)
        {
            // WaitPanel.Show("数据加载中", true);
            byte[] data = null;

            using (UnityWebRequest uwr = UnityWebRequest.Get(assetPath))
            {
                yield return uwr.SendWebRequest();
                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.Log(uwr.error);
                }
                else
                {
                    // Get downloaded asset bundle
                    data = uwr.downloadHandler.data;
                }
                // WaitPanel.Show("数据加载中", false);
                onAssetLoaded(data);
            }
        }
        public enum DownMethodloadType
        {
            Get, Post
        }
        public IEnumerator LoadAsset(Action<byte[]> onAssetLoaded, string assetPath, WWWForm postdata, Dictionary<string, string> headers, DownMethodloadType dt = DownMethodloadType.Get)
        {
            byte[] data = null;
            UnityWebRequest uwr = null;
            switch (dt)
            {
                case DownMethodloadType.Get:
                    uwr = UnityWebRequest.Get(assetPath);
                    break;
                case DownMethodloadType.Post:
                    uwr = UnityWebRequest.Post(assetPath, postdata);
                    break;
            }

            uwr.timeout = 3;
            if (headers != null && headers.Count > 0)
            {
                foreach (var header in headers)
                {
                    uwr.SetRequestHeader(header.Key, header.Value);
                }
            }
            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                data = uwr.downloadHandler.data;
            }

            onAssetLoaded(data);
        }
    }

    public enum SourceType
    {
        TextAsset,
        Image,
        Audio,
        Other
    }

    public enum DownLoadType
    {
        Resource,
        AssetBoundle,
        FileorNet
    }
}
