using System.Collections.Generic;
using Assets.Scripts.WT_FrameWork.Dev;
using Assets.Scripts.WT_FrameWork.Protocol.New;
using Assets.Scripts.WT_FrameWork.UIFramework.Base;
using Assets.Scripts.WT_FrameWork.UIFramework.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.WT_FrameWork.UIFramework.PanelScripts
{
    public class DevPanel : BasePanel
    {
        public List<DevItemBase> dibs1069,dibs1051,dibs0808,dibsMeter;
        private Transform content_1069root, content_1051root, content_0808root, content_Meterroot;
        private Text versionText;
        public override void OnEnter()
        {
            base.OnEnter();
            
            versionText = transform.Find("up/version").GetComponent<Text>();
            versionText.text = Util.Util.GetSystemConfig("AppConfig", "version");
            content_0808root = transform.Find("mid/Scroll View/Viewport/Content/dev_0808Group/Viewport/Content");
            content_1051root = transform.Find("mid/Scroll View/Viewport/Content/dev_1051Group/Viewport/Content");
            content_1069root = transform.Find("mid/Scroll View/Viewport/Content/dev_1069Group/Viewport/Content");
            content_Meterroot = transform.Find("mid/Scroll View/Viewport/Content/dev_Meter_Group/Viewport/Content");

            GameObject mdr0808 = Resources.Load("UI/Dev/mdr0808item") as GameObject;
            GameObject rd1051 = Resources.Load("UI/Dev/rd1051item") as GameObject;
            GameObject rd1069 = Resources.Load("UI/Dev/rd1069item") as GameObject;
            GameObject meter = Resources.Load("UI/Dev/meteritem") as GameObject;
            if (dibs0808.Count == 0)
            {
                foreach (var dev0808 in GameRoot.CollecterManager.Devs0808.Values)
                {
                    GameObject gt = Instantiate(mdr0808, content_0808root);
                    DevItemBase dib = gt.GetComponent<DevItemBase>();
                    dib.LoadDev(dev0808);
                    dibs0808.Add(dib);
                }
            }

            if (dibs1069.Count == 0)
            {
                foreach (var dev1069 in GameRoot.CollecterManager.Devs1069.Values)
                {
                    GameObject gt = Instantiate(rd1069, content_1069root);
                    DevItemBase dib = gt.GetComponent<DevItemBase>();
                    dib.LoadDev(dev1069);
                    dibs1069.Add(dib);
                }
            }

            if (dibs1051.Count == 0)
            {
                foreach (var dev1051 in GameRoot.CollecterManager.Devs_1051.Values)
                {
                    GameObject gt = Instantiate(rd1051, content_1051root);
                    DevItemBase dib = gt.GetComponent<DevItemBase>();
                    dib.LoadDev(dev1051);
                    dibs1051.Add(dib);
                }
            }

            if (dibsMeter.Count==0)
            {
                foreach (var m in GameRoot.CollecterManager.DevsMeters.Values)
                {
                    for (int i = 0; i < m.DevNum; i++)
                    {
                        GameObject gt = Instantiate(meter, content_Meterroot);
                        DevItemBase dib = gt.GetComponent<DevItemBase>();
                        DevItemMeter dim = dib as DevItemMeter;
                        dim.Addr = i;
                        dib.LoadDev(m);
                        dibsMeter.Add(dim);
                    }
                   
                }
            }
            GameRoot.CollecterManager.ConnectAllDev();
        }

        public override void ClosePanel()
        {
            base.ClosePanel();
        }

      
        public override void OnExit()
        {
            base.OnExit();
        }

        public override void OnPause()
        {
            base.OnPause();
        }

        public override void OnResume()
        {
            base.OnResume();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }
    }
}
