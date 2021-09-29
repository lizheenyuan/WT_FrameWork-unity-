using Assets.Scripts.WT_FrameWork.Util;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("FrameWork/Util")]
    [Tooltip("获取配置信息key1，key2")]
    public class GetSystemConfig : FsmStateAction
    {
        [RequiredField]
        public FsmString Key1;
        [RequiredField]
        public FsmString Key2;

        public FsmString RetVal;

        // Code that runs on entering the state.
        public override void OnEnter()
        {
            if (string.IsNullOrEmpty(Key1.Value) || string.IsNullOrEmpty(Key2.Value) || Key1.IsNone || Key2.IsNone)
            {
                Log("参数错误");
            }
            else
            {
                RetVal.Value = Util.GetSystemConfig(Key1.Value, Key2.Value);
            }

            Finish();
        }
    }
}