using System;
using System.Text;
using SpeechLib;
using UnityEngine;
using UnityEngine.UI;

namespace Tools
{
    public enum TextShowStyle
    {
        SubStr,
        StrUnderLine
    }
    public class WorkerSpeaker : MonoBehaviour, ISpeaker
    {
        public Text TextPanel;
        public bool IsShowText = true;
        private SpVoice voice;
        private bool isSpeaking = false;
        private string speakStr;
        public TextShowStyle TSS;
        public static WorkerSpeaker Instance;

        // Start is called before the first frame update
        void Awake()
        {
            speakStr = "测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字测试文字";
            voice = Init();
            InitParam();
            // Speak(speakStr);
            Instance = this;
        }

        private int _test = 0;
        private void Update()
        {
            if (IsPosChanged()&&IsShowText && TextPanel != null && voice.Status.RunningState == SpeechRunState.SRSEIsSpeaking)
            {
                _test++;
                print(_test);
                switch (TSS)
                {
                    case TextShowStyle.SubStr:
                        TextPanel.text = GetSpeakedStr();
                        break;
                    case TextShowStyle.StrUnderLine:
                        TextPanel.text = GetUnderLineStr().ToString();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
            }

            if (voice.Status.RunningState == SpeechRunState.SRSEDone)
            {
                if (OnComplelted != null)
                {
                    OnComplelted?.Invoke();
                    // OnComplelted = null;
                    Debug.Log("-------------------speak end " + speakStr);
                }

                isSpeaking = false;
            }
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.P))
            {
                Pause();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                Resume();
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                Speak(speakStr);
            }
#endif
        }

        private StringBuilder _sbContent;
        private int _curWordPos = 0;
        private StringBuilder GetUnderLineStr(string color = "Red")
        {
            int pos = voice.Status.InputWordPosition;
            int len = voice.Status.InputWordLength;
            var ret = new StringBuilder(_sbContent.ToString()).Insert(pos, "<color=red>");
            ret.Insert(pos + len+"<color=red>".Length, "</color>");
            return ret;
        }

        private bool IsPosChanged()
        {
            int pos = _curWordPos;
            _curWordPos = voice.Status.InputWordPosition;
            return pos != _curWordPos;
        }
        private void OnDestroy()
        {
            Stop();
        }

        void InitParam()
        {
            _curWordPos = 0;
            if (voice != null)
            {
                voice.Rate = 1;
                voice.Volume = 100;
                for (int i = 0; i < voice.GetVoices().Count; i++)
                {
                    // Debug.Log(voice.GetVoices().Item(i).GetDescription());
                    if (voice.GetVoices().Item(i).GetDescription().Contains("Microsoft Cortana"))
                    {
                        voice.Voice = voice.GetVoices().Item(i);
                        break;
                    }
                }
                // voice.Voice = voice.GetVoices().Item(0);

                // var enums = voice.GetVoices().GetEnumerator();
                // while (enums.MoveNext())
                // {
                //    var token = ((SpObjectToken)enums.Current);
                //    if (token != null && token.GetDescription().Contains("Microsoft Cortana"))//Microsoft Cortana
                //    {
                //        voice.Voice = token;
                //        Debug.Log($"Select Voice:{token.GetDescription()}");
                //        break;
                //    }
                // }
            }
        }

        public SpVoice Init()
        {
            return new SpVoice();
        }

        public ISpeaker Speak(string str)
        {
            _curWordPos = 0;
            // if (voice.Status.RunningState == SpeechRunState.SRSEIsSpeaking)
            // {
            //     voice.Pause();
            // }
            _sbContent = new StringBuilder(str);
            for (int i = 0; i < voice.GetVoices().Count; i++)
            {
                // Debug.Log(voice.GetVoices().Item(i).GetDescription());
                if (voice.GetVoices().Item(i).GetDescription().Contains("Microsoft Cortana"))
                {
                    voice.Voice = voice.GetVoices().Item(i);
                    break;
                }
            }

            this.speakStr = str;
            // if (onComplelted != null)
            // {
            //     onComplelted = null;
            // }

            voice.Resume();
            voice.Speak(str, SpeechVoiceSpeakFlags.SVSFlagsAsync);
            isSpeaking = true;
            return this;
        }

        public ISpeaker SpeakKangKang(string str)
        {
            // if (voice.Status.RunningState == SpeechRunState.SRSEIsSpeaking)
            // {
            //     voice.Pause();
            // }
            for (int i = 0; i < voice.GetVoices().Count; i++)
            {
                // Debug.Log(voice.GetVoices().Item(i).GetDescription());
                if (voice.GetVoices().Item(i).GetDescription().Contains("Kangkang"))
                {
                    voice.Voice = voice.GetVoices().Item(i);
                    break;
                }
            }

            this.speakStr = str;
            if (onComplelted != null)
            {
                onComplelted = null;
            }

            voice.Speak(str, SpeechVoiceSpeakFlags.SVSFlagsAsync);
            isSpeaking = true;
            return this;
        }

        public void Pause()
        {
            voice.Pause();
            isSpeaking = false;
            OnComplelted = null;
        }

        public void Resume()
        {
            voice.Resume();
            isSpeaking = true;
        }
        public void Stop()
        {
            voice.Speak("", SpeechVoiceSpeakFlags.SVSFPurgeBeforeSpeak);
        }
        public string GetSpeakedStr()
        {
            return speakStr.Substring(0, Mathf.Min(voice.Status.InputWordPosition + 2, speakStr.Length));
        }

        public void SetVoice(int i)
        {
        }

        private Action onComplelted;

        public Action OnComplelted
        {
            get => onComplelted;
            set
            {
                onComplelted = null;
                onComplelted += value;
            }
        }

        public bool IsSpeaking => isSpeaking;
    }

    public interface ISpeaker
    {
        SpVoice Init();

        // void Speak(string str);
        ISpeaker Speak(string str);
        void Pause();
        string GetSpeakedStr();
        Action OnComplelted { get; set; }
    }
}