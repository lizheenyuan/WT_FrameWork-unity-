using System;

namespace Assets.Scripts.WT_FrameWork.SingleTon
{
    public class WT_Singleton<T> where T : class, new()
    {
        private static T m_instance;

        public static T instance
        {
            get
            {
                if (WT_Singleton<T>.m_instance == null)
                {
                    WT_Singleton<T>.CreateInstance();
                }
                return WT_Singleton<T>.m_instance;
            }
        }

        protected WT_Singleton()
        {
        }

        public static void CreateInstance()
        {
            if (WT_Singleton<T>.m_instance == null)
            {
                WT_Singleton<T>.m_instance = Activator.CreateInstance<T>();
                (WT_Singleton<T>.m_instance as WT_Singleton<T>).Init();
            }
        }

        public static void DestroyInstance()
        {
            if (WT_Singleton<T>.m_instance != null)
            {
                (WT_Singleton<T>.m_instance as WT_Singleton<T>).UnInit();
                WT_Singleton<T>.m_instance = (T)((object)null);
            }
        }

        public static T GetInstance()
        {
            if (WT_Singleton<T>.m_instance == null)
            {
                WT_Singleton<T>.CreateInstance();
            }
            return WT_Singleton<T>.m_instance;
        }

        public static bool HasInstance()
        {
            return WT_Singleton<T>.m_instance != null;
        }

        public virtual void Init()
        {
        }

        public virtual void UnInit()
        {
        }
    }
}