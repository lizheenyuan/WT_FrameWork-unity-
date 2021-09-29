using UnityEngine;

namespace Assets.Scripts.WT_FrameWork.SingleTon
{
    public class WT_Mono_Singleton<T> : MonoBehaviour where T : WT_Mono_Singleton<T>
    {
        private static T _instance;

        public static T GetInstance()
        {
            return _instance;
        }

        public void SetInstance(T t)
        {
            if (_instance == null)
            {
                _instance = t;
            }
        }

        public virtual void Init()
        {
            return;
        }

        public virtual void Release()
        {
            return;
        }
    }
}