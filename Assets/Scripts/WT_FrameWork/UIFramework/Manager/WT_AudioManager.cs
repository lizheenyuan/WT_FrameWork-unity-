using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.WT_FrameWork.LoadSource;
using Assets.Scripts.WT_FrameWork.SingleTon;
using UnityEngine;

namespace Assets.Scripts.WT_FrameWork.UIFramework.Manager
{
    [RequireComponent(typeof (AudioSource))]
    public class WT_AudioManager : WT_Mono_Singleton<WT_AudioManager>
    {
        private Hashtable sounds;
        private Dictionary<string, Vector3> effectinfo;
        public static float audioVolume, effectVoice;
        private AudioSource audio_source;
        private int SoundCount;
        private int CurrentID = 0;
        private WT_DownLoader downLoader;

        public override void Init()
        {
            base.Init();
            sounds = new Hashtable();
            effectinfo = new Dictionary<string, Vector3>();
            downLoader = GameRoot.GetSingleton<WT_DownLoader>();
            audio_source = transform.gameObject.GetComponent<AudioSource>();
//            audio_source.volume= float.Parse(Util.Util.GetSystemConfig("AudioConfig", "AudioVolume"));
//            effectVoice = float.Parse(Util.Util.GetSystemConfig("AudioConfig", "AudioEffect"));
        }

        //默认添加StreamingAsset
        public void Play(string s_name)
        {
            audio_source.volume = 1f;
            if (sounds.Count == 0 || !sounds.ContainsKey(s_name))
            {
                downLoader.LoadAudioFromUrl(s_name, onGetResult);
            }
            if (sounds.ContainsKey(s_name))
            {
                audio_source.clip = sounds[s_name] as AudioClip;
                audio_source.Play();
            }
        }

        //默认添加StreamingAsset
        public void PlayEffectAtPoint(string s_name, Vector3 pos)
        {
            if (effectinfo.ContainsKey(s_name))
            {
                effectinfo[s_name] = pos;
            }
            else
            {
                effectinfo.Add(s_name, pos);
            }
            if (sounds.Count == 0 || !sounds.ContainsKey(s_name))
            {
                downLoader.LoadAudioFromUrl(s_name, onEffectGetResult);
            }
            if (sounds.ContainsKey(s_name))
            {
                AudioSource.PlayClipAtPoint(sounds[s_name] as AudioClip, pos);
            }
        }

        private void onEffectGetResult(string s_name, bool result, AudioClip audioClip)
        {
            if (result)
            {
                sounds.Add(s_name, audioClip);
                AudioSource.PlayClipAtPoint(audioClip, effectinfo[s_name]);
            }
            else
            {
                Debug.LogError("声音文件不存在:" + s_name);
            }
        }

        private void onGetResult(string s_name, bool result, AudioClip audioClip)
        {
            if (result)
            {
                try
                {
                    sounds.Add(s_name, audioClip);
                    
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.StackTrace);
                }
                audio_source.clip = audioClip;
                audio_source.Play();
            }
            else
            {
                Debug.LogError("声音文件不存在:" + s_name);
            }
        }

        public void Pause()
        {
            if (audio_source.isPlaying)
            {
                audio_source.Pause();
            }
        }

        public void UnPause()
        {
            audio_source.UnPause();
        }

        public void Stop()
        {
            if (audio_source.isPlaying)
            {
                audio_source.Stop();
            }
        }

        public void SetAudioPlayerVolume(float vol)
        {
            if (vol>=0&&vol<=1)
            {
                audio_source.volume = vol;
            }
            if (vol>1)
            {
                audio_source.volume = 1;
            }
            if (vol < 0)
            {
                audio_source.volume = 0;
            }
        }

        public AudioSource GetAudioSource()
        {
            return audio_source;
        }

        public override void Release()
        {
            base.Release();
            Destroy(audio_source);
        }
    }
}