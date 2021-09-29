using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.WT_FrameWork;
using Assets.Scripts.WT_FrameWork.UIFramework.Base;
using Assets.Scripts.WT_FrameWork.UIFramework.Manager;
using Assets.Scripts.WT_FrameWork.Util;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainBgPanel : BasePanel
{
   private Button _roamBtn;
   protected override void UnRigistEvent()
   {
     UnRigistBtnEvent(_roamBtn);
   }

   protected override void RigistEvent()
   {
      RigistBtnEvent(_roamBtn,OnRoamClick);
   }

   private void OnRoamClick()
   {
      GameRoot.Scenemanager.LoadScene("Roam", () =>
      {
         GameRoot.DownLoader.LoadAssetFromBundle<GameObject>("roaminteractable.unity3d", "interactables", (obj) => { GameObject objx =Instantiate(obj,null) as GameObject;});
      });
      ClosePanel();
   }

   protected override void InitDB()
   {
      base.InitDB();
      _roamBtn = GetComponent<Button>("roambtn");
   }
   
   public override void OnEnter()
   {
      base.OnEnter();
   }
   

   void FillTipText(Text t)
   {
      string txt = t.text;
      if (txt.StartsWith("#"))
      {
         string[] ts = txt.Split('#');
         t.text = Util.GetSystemConfig(ts[1], ts[2]);
      }
   }
   public override void OnExit()
   {
      base.OnExit();
   }

  
}
