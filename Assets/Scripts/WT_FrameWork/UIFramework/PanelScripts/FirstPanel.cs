using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.WT_FrameWork.UIFramework.PanelScripts
{
    public class FirstPanel : MonoBehaviour
    {
        public Action<FirstPanel> OnFirstPanelLoad;

        public Slider S_Progress;

        public Text T_Progress;
        // Start is called before the first frame update
        void Start()
        {
            S_Progress = transform.Find("Progress").GetComponent<Slider>();
            T_Progress = transform.Find("Text").GetComponent<Text>();
            OnFirstPanelLoad?.Invoke(this);
        }

    }
}
