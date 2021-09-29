using System.Collections;
using System.Collections.Generic;
using System.IO;
//using Assets.Scripts.UIFramework.Manager;
using Assets.Scripts.WT_FrameWork.SceneManager;
using Assets.Scripts.WT_FrameWork.UIFramework.Base;
using Assets.Scripts.WT_FrameWork.UIFramework.Manager;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.WT_FrameWork.UIFramework.PanelScripts
{
    public class LoadPanel : BasePanel
    {
        private AsyncOperation async;
        private Slider sld;
        private Text m_Text;

        public override void OnEnter()
        {
            base.OnEnter();

            sld = transform.Find("Slider").GetComponent<Slider>();
            sld.value = 0;
            m_Text = transform.Find("Slider/TipText").GetComponent<Text>();
            StartCoroutine(LoadSence());
        }

        private uint downloadNum = 1;
        private IEnumerator LoadSence()
        {
            AssetBundle bundle = null;
            Debug.Log("scene bundle ==null");
            if (GameRoot.Scenemanager.SceneBundle!=null)
            {
                /*
                sld.value = 0;
                for (int i = 0; i < 90; i++)
                {
                    sld.value = i / 100f;
                    m_Text.text = "Loading Scene: " + (sld.value * 100).ToString("#00.00") + "%";
                    yield return new WaitForSeconds(0.01f);
                }
                sld.value = 0.9f;
                */
                GameRoot.Scenemanager.SceneBundle.Unload(true);
            }
            // else
            {
                using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(Util.Util.BundleRootPath+GameRoot.Scenemanager.NextSceneName.ToLower()+"scene.unity3d"))
                {
                    var bundleRequest = uwr.SendWebRequest();
                    //bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    while ((!uwr.isDone || sld.value < 1) && isActive == true)
                    {
                        if ((bundleRequest.progress <= 1f) && sld.value <= 1f)
                        {
                            m_Text.text = "Loading progress: " + (bundleRequest.progress * 0.9f * 100).ToString("#00.00") + "%";
                            sld.value = bundleRequest.progress;
                        }
                        else
                        {
                            

                        }

                        yield return null;
                        //if (bundleRequest.progress >= 0.9f)
                        //{
                        //    //                    m_Text.text = "Press the key of A to continue";
                        //    //                    if (Input.GetKeyDown(KeyCode.A))
                        //    //                        async.allowSceneActivation = true;
                        //    if (sld.value < 1.0f)
                        //    {
                        //        sld.value += 0.005f;
                        //        m_Text.text = "Loading progress: " + (sld.value * 100).ToString("#00.0") + "%";
                        //        yield return new WaitForSeconds(0.001f);
                        //    }
                        //}

                    }
                    if (uwr.isNetworkError || uwr.isHttpError)
                    {
                        Debug.Log(uwr.error);
                    }
                    else
                    {
                        // Get downloaded asset bundle
                        bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                        GameRoot.Scenemanager.SceneBundle = bundle;
                    }
                }
            }

            async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(Path.Combine(@"Assets/Scenes/" + GameRoot.GetSingleton<WT_SceneManager>().GetLoadSceneName() + ".unity"));
            async.allowSceneActivation = false;
            while (!async.isDone == true)
            {
                if ((async.progress <= 0.9f))
                {
                    sld.value += async.progress * 0.1f;
                    m_Text.text = "Loading Scene: " + (sld.value * 100).ToString("#00.00") + "%";
                }

                if (async.progress >= 0.9f && !async.isDone)
                {
                    async.allowSceneActivation = true;
                }
                yield return null;
            }


        }
    }
}