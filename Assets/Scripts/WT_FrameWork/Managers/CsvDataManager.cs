using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Assets.Scripts.WT_FrameWork.SingleTon;
using Assets.Scripts.WT_FrameWork.Util;
using UnityEngine;
using WT_FrameWork.Data;

namespace Assets.Scripts.WT_FrameWork.Managers
{
    public class CsvDataManager:WT_Singleton<CsvDataManager>
    {
        public readonly string CsvFolderPath = Path.Combine(Application.streamingAssetsPath, "Csv");
        protected Dictionary<Type, object> Datas;
        public override void Init()
        {
            base.Init();
            Datas = new Dictionary<Type, object>();
            LoadDBInfo();

        }

        public List<T> GetDataTable<T>() where T : DataBase
        {
            if (Datas==null||!Datas.Keys.Contains(typeof(T)))
            {
                return null;
            }
            return Datas[typeof(T)] as List<T>;
        }
        void LoadDBInfo()
        {
            List<Type> opList = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                
                opList.AddRange(assembly.GetTypes().Where(a => ((a.Namespace == "Assets.Scripts.WT_FrameWork.Data" || string.IsNullOrEmpty(a.Namespace)) && a.Name.EndsWith("WTData"))).ToArray());
            }
            foreach(var op in opList)
            {
                var filepath = Path.Combine(CsvFolderPath, $"{op.Name}.csv");
                var obj = ExportCsvHelper(op, filepath);
                Datas.Add(op,obj);
            }
            // if (opList.Count > 0)
            // {
            // }
        }

        public static object ExportCsvHelper(Type t,string s)
        {
            MethodInfo mi = typeof(CsvHelper).GetMethod("OpenCsv", BindingFlags.Static|BindingFlags.InvokeMethod|BindingFlags.Public);
            // MethodInfo mi = typeof(CsvHelper).GetMethod("OpenCSV");
            mi = mi.MakeGenericMethod(t);
            return mi.Invoke(null, new []{s});
        }
        public override void UnInit()
        {
            base.UnInit();
        }
    }
}