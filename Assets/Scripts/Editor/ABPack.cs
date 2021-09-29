using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class ABPack
{

    private static string BundlePath= "AB_Win";
    private static string UIBundlePath= "/UI/UIPanel/";

    private static string WebGLBundlePath = "AB_WebGL";

    [MenuItem("Tools/AB/Build_ALL(Win)", priority = 1)]
    public static void BuildWindows()
    {
        BuildAB(BundlePath,BuildTarget.StandaloneWindows);
    }
    private static void BuildAB(string path,BuildTarget bt)
    {
        string ab_path = Path.Combine(Application.streamingAssetsPath, path);
        if (!Directory.Exists(ab_path))
        {
            Directory.CreateDirectory(ab_path);
        }

        try
        {
            BuildPipeline.BuildAssetBundles(ab_path, BuildAssetBundleOptions.None, bt);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message + "\r\n" + e.StackTrace);
        }

        EditorUtility.DisplayDialog("Notice", "Build Windows Completed", "OK");
    }

    //D:/lzyFiles/WorkSpace/UnityProject/TZ004_K2/trunk/WT-FrameWork/Assets/UI/UIPanel\DevPanel.prefab
    [MenuItem("Tools/AB//Build_UI(Win)",priority = 0)]
    public static void BuildWindows_UI()
    {
        BuildUI(BundlePath,BuildTarget.StandaloneWindows);
    }
    [MenuItem("Tools/AB/Build_ALL(WebGL)", priority = 2)]
    public static void BuildWebGL()
    {
        BuildAB(WebGLBundlePath, BuildTarget.WebGL);
    }
    [MenuItem("Tools/AB//Build_UI(WebGL)", priority = 3)]
    public static void BuildWebGL_UI()
    {
        BuildUI(WebGLBundlePath, BuildTarget.WebGL);
    }
    static void BuildUI(string path, BuildTarget bt)
    {
        string ab_path = Path.Combine(Application.streamingAssetsPath, path);
        if (!Directory.Exists(ab_path))
        {
            Directory.CreateDirectory(ab_path);
        }
        List<AssetBundleBuild> panels = new List<AssetBundleBuild>();
        string[] panelsInfo = Directory.GetFiles(Application.dataPath + @UIBundlePath, "*.prefab");
        if (panelsInfo.Length > 0)
        {

            //缩短路径
            for (int j = 0; j < panelsInfo.Length; j++)
            {
                int i = panelsInfo[j].LastIndexOfAny(new[] { '\\', '/' });
                panelsInfo[j] = "Assets" + UIBundlePath + panelsInfo[j].Substring(i + 1);
            }

            var assetBundleBuild = new AssetBundleBuild();
            assetBundleBuild.assetNames = panelsInfo;
            assetBundleBuild.assetBundleName = "uipanel";
            assetBundleBuild.assetBundleVariant = "unity3d";
            panels.Add(assetBundleBuild);
            BuildPipeline.BuildAssetBundles(ab_path, panels.ToArray(), BuildAssetBundleOptions.None, bt);
            EditorUtility.DisplayDialog("Notice", "Build Windows UIBundle Completed", "OK");
        }
    }
    [MenuItem("Tools/AB//Clear")]
    public static async void ClearBuild()
    {
        string ab_path = Path.Combine(Application.streamingAssetsPath, BundlePath);
        if (!Directory.Exists(ab_path))
        {
            Directory.CreateDirectory(ab_path);
        }
        string[] bundles = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath,BundlePath));
        string noticeString = "";
        foreach (var bundle in bundles)
        {
            FileInfo finfo = new FileInfo(bundle);
            noticeString +="\t"+finfo.Name + "\r\n";
        }
        bool b= EditorUtility.DisplayDialog("Warning", $"Remove:\r\n {noticeString}", "Y","N");
        if (b)
        {
            foreach (string bundle in bundles)
            {
                File.Delete(bundle);
            }
        }

        AssetDatabase.Refresh();
    }
}
