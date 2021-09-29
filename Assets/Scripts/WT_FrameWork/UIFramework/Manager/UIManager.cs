using System;
using System.Collections.Generic;
using System.Reflection;
using Assets.Scripts.WT_FrameWork.UIFramework.Base;
using Assets.Scripts.WT_FrameWork.UIFramework.UIPanel;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.WT_FrameWork
{
    /// <summary>
    /// 单例模式的核心：
    /// 1.只在该类内定义一个静态的对象，该对象在外界访问，在内部构造
    /// 2.构造函数私有化
    /// </summary>
    public class UIManager
    {
        //此类作为一个单例模式，即只有一个实例的模式

        private static UIManager _instance;

        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UIManager();
                }
                return _instance;
            }
        }

        private AssetBundle _panels;

        private UIManager() //构造函数私有化（单例模式）-7JTG4
        {
            ParseUIPanelTypeJson(); //构造该类时会解析Json
          
        }

        private Dictionary<string, string> panelPathDict; //存储所有Perfab面板的路径

        private Dictionary<string, BasePanel> panelDict;
        //借助BasePanel脚本保存所有实例化出来的面板物体（因为BasePanel脚本被所有面板预设物体的自己的脚本所继承，所以需要的时候可以根据BasePanel脚本来实例化每一个面板对象）

        private Stack<BasePanel> panelStack; //这是一个栈，用来保存实例化出来（显示出来）的面板

        private Transform canvasTransform; //用来使实例化的面板归为它的子物体

        private Transform CanvasTransform
        {
            get
            {
                if (canvasTransform == null)
                {
                    canvasTransform = GameObject.Find("WTFrameWork/Canvas").transform;
                }
                return canvasTransform;
            }
        }

        public AssetBundle Panels
        {
            get { return _panels; }
        }

        public void InitUI(AssetBundle bundle)
        {
            //_panels = AssetBundle.LoadFromFile(/*Application.streamingAssetsPath*/"http://192.168.1.205:8901" + "/AB_Win/uipanel.unity3d");
            _panels = bundle;
            if (_panels == null)
            {
                Debug.Log("Failed to load AssetBundle!");
            }
        }
        public void ActiveOrDisableUI(bool isActive)
        {
            CanvasTransform.gameObject.SetActive(isActive);
        }
        [Obsolete]
        //页面入栈，即把页面显示在界面上
        public BasePanel PushPanel(UIPanelType panelType)
        {
            if (panelStack == null) //如果栈不存在，就实例化一个空栈
            {
                panelStack = new Stack<BasePanel>();
            }
            if (panelStack.Count > 0)
            {
                BasePanel topPanel = panelStack.Peek(); //取出栈顶元素保存起来，但是不移除
                topPanel.OnPause(); //使该页面暂停，不可交互
            }
            BasePanel panelTemp = GetPanel(panelType);
            panelStack.Push(panelTemp);
            panelTemp.OnEnter(); //页面进入显示，可交互
            return panelTemp;
        }
        public BasePanel PushPanel(string panelType)
        {
            if (panelStack == null) //如果栈不存在，就实例化一个空栈
            {
                panelStack = new Stack<BasePanel>();
            }
            if (panelStack.Count > 0)
            {
                BasePanel topPanel = panelStack.Peek(); //取出栈顶元素保存起来，但是不移除
                topPanel.OnPause(); //使该页面暂停，不可交互
            }
            BasePanel panelTemp = GetPanel(panelType);
            panelStack.Push(panelTemp);
            panelTemp.OnEnter(); //页面进入显示，可交互
            return panelTemp;
        }
        public BasePanel PushPanel(string panelType,object arg,object[] args)
        {
            if (panelStack == null) //如果栈不存在，就实例化一个空栈
            {
                panelStack = new Stack<BasePanel>();
            }
            if (panelStack.Count > 0)
            {
                BasePanel topPanel = panelStack.Peek(); //取出栈顶元素保存起来，但是不移除
                topPanel.OnPause(); //使该页面暂停，不可交互
            }
            BasePanel panelTemp = GetPanel(panelType);
            FieldInfo finfo_arg = typeof(BasePanel).GetField("Arg", BindingFlags.NonPublic|BindingFlags.GetField|BindingFlags.Instance);
            FieldInfo finfo_args = typeof(BasePanel).GetField("Args", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            finfo_arg?.SetValue(panelTemp, arg);
            finfo_args?.SetValue(panelTemp,args);
            panelStack.Push(panelTemp);
            panelTemp.OnEnter(); //页面进入显示，可交互
            return panelTemp;
        }
        public BasePanel PeekPanel()
        {
            if (panelStack == null)
            {
                panelStack = new Stack<BasePanel>();
            }
            if (panelStack.Count <= 0) return null;
            return panelStack.Peek();
        }

        //页面出栈，即把页面从界面上移除
        public BasePanel PopPanel()
        {
            if (panelStack == null)
            {
                panelStack = new Stack<BasePanel>();
            }
            if (panelStack.Count <= 0) return null;
            //关闭栈顶页面的显示
            BasePanel topPanel1 = panelStack.Pop();
            topPanel1.OnExit();
            if (panelStack.Count <= 0) return topPanel1;
            BasePanel topPanel2 = panelStack.Peek();
            topPanel2.OnResume(); //使第二个栈里的页面显示出来，并且可交互
            return topPanel1;
        }

        [System.Serializable]
        private class UIPanelTypeJson //内部类里面就一个链表容器,用来配合解析
        {
            public List<UIPanelInfo> infoList;
        }

        //根据面板类型UIPanelType得到实例化的面板
        [Obsolete]
        public BasePanel GetPanel(UIPanelType panelType)
        {
            if (panelDict == null) //如果panelDict字典为空，就实例化一个空字典
            {
                panelDict = new Dictionary<string, BasePanel>();
            }
            //BasePanel panel;
            //panelDict.TryGetValue(panelType, out panel);//不为空就根据类型得到Basepanel
            BasePanel panel = panelDict.TayGet(Enum.GetName(typeof(UIPanelType), panelType)); //我们扩展的Dictionary的方法，代码作用同上两行
            if (panel == null) //如果得到的panel为空，那就去panelPathDict字典里面根据路径path找到，然后加载，接着实例化
            {
                string path = panelPathDict.TayGet(Enum.GetName(typeof(UIPanelType), panelType)); //我们扩展的Dictionary的方法
                //panelPathDict.TryGetValue(panelType, out path);
                //GameObject instPanel = GameObject.Instantiate(Resources.Load("UIPanel/"+ Enum.GetName(typeof(UIPanelType), panelType) + "Panel")) as GameObject; //根据路径加载并实例化面板
                GameObject instPanel = Object.Instantiate(_panels.LoadAsset<GameObject>(Enum.GetName(typeof(UIPanelType), panelType) + "Panel.prefab"));
                instPanel.transform.SetParent(this.CanvasTransform, false); //设置为Canvas的子物体,false表示实例化的子物体坐标以Canvas为准
                //TODO
                if (instPanel.GetComponent<BasePanel>() == null)
                {
                    //Debug.Log(Enum.GetName(typeof(UIPanelType), panelType) + "Panel");
                    Type tb = Type.GetType("Assets.Scripts.WT_FrameWork.UIFramework.PanelScripts." + Enum.GetName(typeof(UIPanelType), panelType) + "Panel");
                    if (tb == null)
                    {
                        tb = Type.GetType(Enum.GetName(typeof(UIPanelType), panelType) + "Panel");
                    }
                    instPanel.AddComponent(tb);
                }
                panelDict.Add(Enum.GetName(typeof(UIPanelType), panelType), instPanel.GetComponent<BasePanel>());
                return instPanel.GetComponent<BasePanel>();
            }
            else
            {
                return panel;
            }

        }
        public BasePanel GetPanel(string panelname)
        {
            if (panelDict == null) //如果panelDict字典为空，就实例化一个空字典
            {
                panelDict = new Dictionary<string, BasePanel>();
            }
            //BasePanel panel;
            //panelDict.TryGetValue(panelType, out panel);//不为空就根据类型得到Basepanel
            BasePanel panel = panelDict.TayGet(panelname); //我们扩展的Dictionary的方法，代码作用同上两行
            if (panel == null) //如果得到的panel为空，那就去panelPathDict字典里面根据路径path找到，然后加载，接着实例化
            {
                string path = panelPathDict.TayGet(panelname); //我们扩展的Dictionary的方法
                //panelPathDict.TryGetValue(panelType, out path);
                GameObject instPanel = Object.Instantiate(_panels.LoadAsset<GameObject>(panelname + "Panel.prefab"));
                instPanel.transform.SetParent(this.CanvasTransform, false); //设置为Canvas的子物体,false表示实例化的子物体坐标以Canvas为准
                //TODO
                if (instPanel.GetComponent<BasePanel>() == null)
                {
                    Type tb = Type.GetType("Assets.Scripts.WT_FrameWork.UIFramework.PanelScripts." + panelname + "Panel");
                    if (tb == null)
                    {
                        tb = Type.GetType(panelname + "Panel");
                    }
                    instPanel.AddComponent(tb);
                }
                panelDict.Add(panelname, instPanel.GetComponent<BasePanel>());
                return instPanel.GetComponent<BasePanel>();
            }
            else
            {
                return panel;
            }

        }
        //解析UIPanelType.json的信息
        private void ParseUIPanelTypeJson()
        {
            panelPathDict = new Dictionary<string, string>(); //实例化一个字典对象
            //TextAsset ta = Resources.Load<TextAsset>("UIPanelType"); //获取UIPanelType.json文件的文本信息
            //UIPanelTypeJson jsonObject = JsonUtility.FromJson<UIPanelTypeJson>(ta.text);
            //    //把UIPanel.json文本信息转化为一个内部类的对象，对象里面的链表里面对应的是每个Json信息对应的类
            //foreach (UIPanelInfo info in jsonObject.infoList)
            //{
            //    //Debug.Log(info.panelType);
            //    panelPathDict.Add(Enum.GetName(typeof(UIPanelType), info.panelType), info.path); //把每一个进过json文件转化过来的类存入字典里面(键值对的形式)
            //}
        }
    }
}