using Assets.Scripts.WT_FrameWork;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{

	[ActionCategory("FrameWork/UI")]
	[Tooltip("打开UI面板")]
	public class OpenUIPanel : FsmStateAction
	{
		[RequiredField]
		public FsmString PanelName;
		
		public FsmString arg;
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			UIManager.Instance.PushPanel(PanelName.Value, int.Parse(arg.Value),null);
			Finish();
		}


	}

}
