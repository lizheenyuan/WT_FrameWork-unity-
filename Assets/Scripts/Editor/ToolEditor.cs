using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Assets.Scripts.WT_FrameWork.UIFramework.PanelScripts;
using Assets.Scripts.WT_FrameWork.Util;
using UnityEngine;
using UnityEditor;

public class ToolEditor : EditorWindow
{
    public enum OPTIONS
    {
        CUBE = 0,
        SPHERE = 1,
        PLANE = 2
    }

    string myString = "Hello World";
    bool groupEnabled;
    bool myBool = true;
    float myFloat = 1.23f;

    private static ToolEditor window;
    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/My Window")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        window = (ToolEditor)EditorWindow.GetWindow(typeof(ToolEditor));
        window.position = new Rect(50, 50, 250, 60);
        window.Show();
    }
    void InstantiatePrimitive(OPTIONS op)
    {
        switch (op)
        {
            case OPTIONS.CUBE:
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = Vector3.zero;
                break;
            case OPTIONS.SPHERE:
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = Vector3.zero;
                break;
            case OPTIONS.PLANE:
                GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                plane.transform.position = Vector3.zero;
                break;
            default:
                Debug.LogError("Unrecognized Option");
                break;
        }
    }
    public OPTIONS op;
    void OnGUI()
    {
        op = (OPTIONS)EditorGUILayout.EnumPopup("Primitive to create:", op);
        if (GUILayout.Button("Create"))
            InstantiatePrimitive(op);
        // Now create the menu, add items and show it
        if (EditorGUILayout.DropdownButton(new GUIContent("123"),FocusType.Keyboard ))
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("MenuItem1"), false, Callback, "item 1");
            menu.AddItem(new GUIContent("MenuItem2"), false, Callback, "item 2");
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("SubMenu/MenuItem3"), false, Callback, "item 3");
            menu.AddItem(new GUIContent("SubMenu/MenuItem4"), false, Callback, "item 4");
            menu.AddItem(new GUIContent("SubMenu/MenuItem5"), false, Callback, "item 5");
            menu.AddSeparator("SubMenu/");
            menu.AddItem(new GUIContent("SubMenu/MenuItem6"), false, Callback, "item 6");

            menu.ShowAsContext();
        }
        
        //Event evt = Event.current;
        //Rect contextRect = new Rect(10, 10, 100, 100);

        //if (evt.type == EventType.ContextClick)
        //{
        //    Vector2 mousePos = evt.mousePosition;
        //    if (contextRect.Contains(mousePos))
        //    {
               

        //        evt.Use();
        //    }
        //}

    }
    public void Callback(object obj)
    {
        Debug.Log("Selected: " + obj);
    }
}
// Creates an instance of a primitive depending on the option selected by the user.
public class EditorGUILayoutPopup : EditorWindow
{
    public string[] options;
    private Dictionary<string, Type> CInfo;
    public int index = 0;
    [MenuItem("Tools/Csv",priority =2)]
    static void Init()
    {
        EditorWindow window = GetWindow(typeof(EditorGUILayoutPopup));
        window.position = new Rect(Screen.width / 2 - 100, Screen.height / 2 - 150, 300, 500);
        window.Show();
    }

    void Awake()
    {
        CInfo=new Dictionary<string, Type>();
        LoadPanelInfo();
    }

    void LoadPanelInfo()
    {
        List<Type> opList = new List<Type>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            opList.AddRange(assembly.GetTypes().Where(a => ((a.Namespace == "Assets.Scripts.WT_FrameWork.Data" || string.IsNullOrEmpty(a.Namespace)) && a.Name.EndsWith("WTData"))).ToArray());
        }
        CInfo=new Dictionary<string, Type>(opList.ToDictionary(a=>a.Name));
        if (opList.Count > 0)
        {
            options = opList.Select(a => a.Name).ToArray();
        }
    }

    private bool isShow;
    void OnGUI()
    {
        if (options!=null)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            index = EditorGUILayout.Popup(index, options);
            bool b=GUILayout.Button("Refresh");
            if (b)
            {
               LoadPanelInfo();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Show/UnShow"))
            {
                isShow = !isShow;
               
            }
            ShowInfo(isShow);


            EditorGUILayout.EndVertical();
        }
       
    }
    void OnInspectorUpdate()
    {
        this.Repaint();
    }

    void OnFocus()
    {
        Awake();
    }
    void OnProjectChange()
    {
        Awake();
    }
    private Vector2 svPos;
    void ShowInfo(bool isshow)
    {
        if (!isshow)
        {
            return;
        }
        //通过反射 显示要显示的列
        BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;//反射标识
        PropertyInfo[] propInfoArr = CInfo[options[index]].GetProperties(bf);
        
        svPos=EditorGUILayout.BeginScrollView(svPos,true,false,GUILayout.Height(300));
        EditorGUILayout.BeginHorizontal();
        foreach (PropertyInfo propertyInfo in propInfoArr)
        {
            GUILayout.Label(propertyInfo.Name,GUILayout.Width(50));
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
        if (GUILayout.Button("CreateCsv"))
        {
            string csvroot = Application.streamingAssetsPath + "/Csv";
            string csvpath = Path.Combine(csvroot, options[index] + ".csv");
            if (!Directory.Exists(csvroot))
            {
                Directory.CreateDirectory(csvroot);
            }

            if (File.Exists(csvpath))
            {
                if (EditorUtility.DisplayDialog("文件已存在", "是否替换" + options[index] + ".csv", "y", "n"))
                {

                    //CsvHelper.SaveAsCSV<ttt> (options[index] + ".csv", null);
                }
            }
            MethodInfo mi = typeof(CsvHelper).GetMethod("SaveAsCSV", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(CInfo[options[index]]);
            mi.Invoke(null, new[] { csvpath, null });
        }


    }
    Type ShowClassInfo(string cname)
    { 
        Debug.Log(cname);
        Type tt = Type.GetType("Assets.Scripts.WT_FrameWork.UIFramework.PanelScripts." + cname);
        if (tt == null)
        {
            tt = Type.GetType(cname);
        }

        return tt;
    }
}
