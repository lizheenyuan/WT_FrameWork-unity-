using System;
using System.Collections;
using Assets.Scripts.WT_FrameWork.SingleTon;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Assets.Scripts.WT_FrameWork.UIFramework.Manager
{
    public class WT_VideoManager : WT_Mono_Singleton<WT_VideoManager>
    {
        private VideoPlayer moivePlayer;
        public Action OnVideoFinished;//每次使用完后自动清空
        private Coroutine playCor;
        private RenderTexture rt;
        public VideoPlayer MoviePlayer
        {
            get
            {
                if (moivePlayer==null)
                {
                    GameObject o = GameObject.FindWithTag("MainCamera");
                    moivePlayer = o.GetComponent<VideoPlayer>();
                    AudioSource v_sound = GameRoot.GetSingleton<WT_AudioManager>().GetAudioSource();
                    if (v_sound == null)
                    {
                        v_sound = o.AddComponent<AudioSource>();
                    }
                    if (moivePlayer == null)
                    {
                        moivePlayer = o.AddComponent<VideoPlayer>();
                        moivePlayer.renderMode = VideoRenderMode.CameraNearPlane;
                        moivePlayer.playOnAwake = false;
                        moivePlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                        moivePlayer.SetTargetAudioSource(0, v_sound);
                    }
                }               
                return moivePlayer;
            }
        }

        private IEnumerator OnPlayEndCor()
        {
            while (MoviePlayer.isPlaying||!MoviePlayer.isPrepared)
            {
                if (Input.GetKeyDown(KeyCode.Escape)||Input.GetMouseButtonDown(0))
                {
                    MoviePlayer.Stop();
                    break;
                }
                yield return null;
            }
            if (OnVideoFinished!=null)
            {
                Destroy(MoviePlayer);
                OnVideoFinished();
                OnVideoFinished = null;
            }
        }

        public void Play()
        {
            if (MoviePlayer==null)
            {
                Debug.Log("没有找到VideoPlayer组件");
                return;
            }
            MoviePlayer.Play();
            if (playCor!=null)
            {
                StopCoroutine(playCor);
            }
            playCor= StartCoroutine(OnPlayEndCor());
        }
        //自动添加file://
        public void Play(string url)
        {
            MoviePlayer.renderMode = VideoRenderMode.CameraNearPlane;
            MoviePlayer.url = url;
            Play();
        }

        public void Play(VideoClip vc)
        {
            MoviePlayer.renderMode = VideoRenderMode.CameraNearPlane;
            MoviePlayer.clip = vc;
            Play();
        }

        public void PlayOnTex(string url, RawImage play_texture)
        {
            MoviePlayer.renderMode = VideoRenderMode.RenderTexture;
            rt = new RenderTexture(play_texture.texture.width, play_texture.texture.height,24);
            MoviePlayer.targetTexture = rt;
            play_texture.texture = MoviePlayer.targetTexture;
            MoviePlayer.url = "file://" + url;
            Play();
        }

        public void Stop()
        {
            if (MoviePlayer!=null&&MoviePlayer.isPlaying)
            {
                if (playCor!=null)
                {
                    StopCoroutine(playCor);
                }
                MoviePlayer.Stop();
            }      
        }
    }
}
