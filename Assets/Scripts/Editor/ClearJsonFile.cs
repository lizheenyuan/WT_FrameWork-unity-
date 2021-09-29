using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

public class ClearJsonFile
{
    private static string jsonDataDir = Path.Combine(Application.persistentDataPath, "Configs");
    [MenuItem("Tools/ClearAllJson")]
    public static void ClearAllJson()
    {
        if (Directory.Exists(jsonDataDir))
        {
            foreach (var file in Directory.GetFiles(jsonDataDir))
            {
                File.Delete(file);
                Debug.Log("已删除"+file);
            }
        }
        else
        {
            Debug.Log("No Dir Configs");
        }
    }
    [MenuItem("Tools/ClearRecordJson")]
    public static void ClearRecordJson()
    {
        if (File.Exists(Path.Combine(jsonDataDir,"Record.json")))
        {
            File.Delete(Path.Combine(jsonDataDir, "Record.json"));
            Debug.Log("已删除"+ "Record.json");
        }
        else
        {
            Debug.Log("未找到" + "Record.json");
        }
    }
    [MenuItem("Tools/ClearSystemConfigJson")]
    public static void ClearSystemConfigJson()
    {
        if (File.Exists(Path.Combine(jsonDataDir, "SystemConfig.json")))
        {
            File.Delete(Path.Combine(jsonDataDir, "SystemConfig.json"));
            Debug.Log("已删除" + "SystemConfig.json");
        }
        else
        {
            Debug.Log("未找到" + "SystemConfig.json");
        }
    }
}
