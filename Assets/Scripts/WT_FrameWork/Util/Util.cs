using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Assets.Scripts.WT_FrameWork.Protocol;
using Assets.Scripts.WT_FrameWork.UIFramework.Manager;
using Assets.Scripts.WT_FrameWork.UIFramework.UIPanel;
using LitJson;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.WT_FrameWork.Util
{
    public class Util
    {
        /// <summary>
        /// lzy 11/16 字符串转换PanelType
        /// </summary>
        /// <param name="panelName"></param>
        /// <returns></returns>
        public static UIPanelType String2PanelType(string panelName)
        {
            return (UIPanelType)Enum.Parse(typeof(UIPanelType), panelName);
        }

#if UNITY_ANDROID
        private static string _resPathRoot =  Application.persistentDataPath;
#elif UNITY_WEBGL&&!UNITY_EDITOR
        private static string _resPathRoot = "http://192.168.0.205:8901";
#else
        private static string _resPathRoot = AddFileHeader2Str(Application.streamingAssetsPath);
#endif

#if UNITY_EDITOR
        private static string _localConfigPath = Application.streamingAssetsPath + "/Configs/";
#elif UNITY_STANDALONE_WIN
        private static string _localConfigPath = Application.persistentDataPath + "/Configs/";
#else
        private static string _localConfigPath=null;
#endif
        public static string _configPath = _resPathRoot + "/Configs/";
        private static TextAsset ta_Lang;
        private static string ta_Config;
        private static string recordZXLX;

        public static string ResPathRoot
        {
            get { return _resPathRoot; }
            set
            {
#if !UNITY_EDITOR
                _resPathRoot = value;
#endif
            }
        } /*=> _resPathRoot;*/
        public static string BundleRootPath
        {
            get
            {
#if UNITY_STANDALONE
                return _resPathRoot + "/AB_Win/";
#elif UNITY_WEBGL || UNITY_EDITOR
                return _resPathRoot + "/AB_WebGL/";
#endif

            }
        }
        public static string UIBundlePath
        {
            get
            {
#if UNITY_STANDALONE
                return _resPathRoot + "/AB_Win/uipanel.unity3d";
#elif UNITY_WEBGL || UNITY_EDITOR
                return _resPathRoot + "/AB_WebGL/uipanel.unity3d";
#endif

            }
        }
        public static string ScenesBundlePath
        {
            get
            {
#if UNITY_STANDALONE
                return _resPathRoot + "/AB_Win/scenes.unity3d";
#elif UNITY_WEBGL || UNITY_EDITOR
                return _resPathRoot + "/AB_WebGL/scenes.unity3d";
#endif

            }
        }
        public static string AudioPath
        {
            get { return _resPathRoot + "/Audio/"; }
        }
        public static string GetLangText(string key)
        {
            JsonData jd = JsonMapper.ToObject(Ta_Lang.text);
            if (jd[key] != null)
            {
                return (string)jd[key];
            }

            return null;
        }

        public static string GetSystemConfig(string configType, string key)
        {
            JsonData jd = JsonMapper.ToObject(TaConfig);
            if (jd[configType][key] != null)
            {
                return (string)jd[configType][key];
            }

            return null;
        }

        /// <summary>
        /// 把配置修改存在内存，如果需要序列化调用save
        /// </summary>
        /// <param name="configType"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SetSystemConfig(string configType, string key, string value)
        {
            if ((GetSystemConfig(configType, key) != null))
            {
                JsonData jd = JsonMapper.ToObject(TaConfig);
                jd[configType][key] = value;
                TaConfig = jd.ToJson();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 把修改过的配置保存到本地配置文件中
        /// </summary>
        public static void SaveSystemConfig()
        {
            if (string.IsNullOrEmpty(_localConfigPath))
            {
                return;
            }
            try
            {
                FileStream fs = new FileStream(Path.Combine(_localConfigPath, "SystemConfig.json"), FileMode.Truncate,
                    FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                sw.Write(TaConfig);
                sw.Flush();
                sw.Close();
                TaConfig = "";
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        public static List<int> GetCollections()
        {
            List<int> myCollections = new List<int>();
            JsonData jd = JsonMapper.ToObject(RecordZxlx);
            if (jd["Collections"] != null)
            {
                for (int i = 0; i < jd["Collections"].Count; i++)
                {
                    myCollections.Add(Convert.ToInt32(jd["Collections"][i].ToString()));
                }

                return myCollections;
            }

            return null;
        }

        public static void SaveCollections(List<int> collections)
        {
            string jString = JsonMapper.ToJson(collections);
            JsonData jdRoot = JsonMapper.ToObject(RecordZxlx);
            jdRoot["Collections"] = JsonMapper.ToObject(jString);
            RecordZxlx = JsonMapper.ToJson(jdRoot);
        }

        public static Dictionary<int, string> GetResults(string PanelName) //
        {
            Dictionary<int, string> results = new Dictionary<int, string>();
            JsonData jd = JsonMapper.ToObject(RecordZxlx);
            if (jd[PanelName] != null)
            {
                for (int i = 0; i < jd[PanelName].Count; i++)
                {
                    results.Add(Convert.ToInt32(jd[PanelName][i]["Sno"].ToString()),
                        (string)jd[PanelName][i]["result"]);
                }

                return results;
            }

            return null;
        }

        public static string GetStrTrimStart0(string str)
        {
            return str.TrimStart('0');
        }

        private class STResult
        {
            public string Sno = "";
            public string result = "";
        }

        public static void SaveResults(string PanelName, Dictionary<int, string> results)
        {
            List<STResult> jdList = new List<STResult>();
            foreach (int key in results.Keys)
            {
                STResult stR = new STResult();
                stR.Sno = key.ToString();
                stR.result = results[key];
                jdList.Add(stR);
            }

            string jString = JsonMapper.ToJson(jdList);
            JsonData jdRoot = JsonMapper.ToObject(RecordZxlx);
            jdRoot[PanelName] = JsonMapper.ToObject(jString);
            RecordZxlx = JsonMapper.ToJson(jdRoot);
        }

        /// <summary>
        /// 读取Excel文件到DataSet数据集的方法
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>DataSet对象</returns>
        /// LZW@17/11/16/AM
        //public static DataSet FromExcelToDataSet(string filePath)
        //{
        //    try
        //    {
        //        FileStream stream = File.Open(Application.dataPath + filePath, FileMode.Open, FileAccess.Read);
        //        //使用OpenXml读取Excel文件
        //        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        //        //将execl数据转化为DataSet
        //        DataSet result = excelReader.AsDataSet();
        //        return result;
        //    }

        //    catch (Exception ex)
        //    {
        //        throw new Exception("读取Execl失败：" + ex.Message);
        //    }
        //}
        //获取类型中的静态变量
        public static string GetPalmKey(Type t,string fieldname)
        {
            try
            {
                BindingFlags flag = BindingFlags.Static | BindingFlags.Public;
                FieldInfo f_key = t.GetField(fieldname, flag);
                object o = f_key.GetValue(null);
                return o.ToString();
            }
            catch (Exception e)
            {
                Debug.LogError($"获取字段值是失败，原因是：{e.Message}\n{e.StackTrace}");
                return null;
            }
        }
        /// <summary>
        /// /保存二进制文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <returns>布尔值 真假</returns>
        ///   LZW@17/11/16/AM
        public static bool SaveBinaryFile(string path, byte[] content)
        {
            //二进制文件流信息  
            BinaryWriter bw = new BinaryWriter(new FileStream(path, FileMode.Create));
            try
            {
                //加密问题后期考虑
                bw.Write(content);
            }
            catch (IOException e)
            {
                Debug.Log(e.Message);
            }

            return true;
        }

        /// <summary>
        /// 读取二进制文件
        /// </summary>
        /// <param name="content"></param>
        /// <returns>二进制文件数组</returns>
        public static byte[] ReadBinaryFile(byte[] content)
        {
            try
            {
                using (BinaryReader br = new BinaryReader(new MemoryStream(content)))
                {
                    byte[] data = br.ReadBytes(content.Length);
                    return data;
                }
            }
            catch (IOException e)
            {
                Debug.Log(e.Message);
            }

            return null;
        }

        /// <summary>
        ///  DataSet转换为byte数组文件
        /// </summary>
        /// <param name="dsOriginal"></param>
        /// <returns>二进制数组</returns>
        public static byte[] GetBinaryFormatData(DataSet dsOriginal)
        {
            byte[] binaryDataResult = null;
            MemoryStream memStream = new MemoryStream();
            //以二进制格式将对象或整个连接对象图形序列化和反序列化。
            IFormatter brFormatter = new BinaryFormatter();
            //dsOriginal.RemotingFormat 为远程处理期间使用的DataSet 获取或设置 SerializtionFormat        
            //SerializationFormat.Binary      将字符串比较方法设置为使用严格的二进制排序顺序
            dsOriginal.RemotingFormat = SerializationFormat.Binary;
            //把字符串以二进制放进memStream中
            brFormatter.Serialize(memStream, dsOriginal);
            //转为byte数组
            binaryDataResult = memStream.ToArray();
            memStream.Close();
            memStream.Dispose();
            return binaryDataResult;
        }

        ///二进制byte数组转DataSet
        /// <summary>
        /// 将字节数组反序列化成DataSet对象
        /// </summary>
        /// <param name="binaryData">字节数组</param>
        /// <returns>DataSet对象</returns>
        public static DataSet RetrieveDataSet(byte[] binaryData)
        {
            DataSet ds = null;
            MemoryStream memStream = new MemoryStream(binaryData, true);
            IFormatter brFormatter = new BinaryFormatter();
            ds = (DataSet)brFormatter.Deserialize(memStream);
            return ds;
        }

        public static List<int> ParseExamJson(string jsString, out float time)
        {
            JsonData jd = JsonMapper.ToObject(jsString);
            string test = jd["testnum"].ToString();
            time = float.Parse(jd["ptime"].ToString());

            string[] nums = test.Split(',');
            List<int> testList = new List<int>();
            foreach (string num in nums)
            {
                if (num != "")
                {
                    testList.Add(int.Parse(num));
                }
            }

            return testList;
        }

        public static byte Str2Byte(string str)
        {
            return Convert.ToByte(str, 16);
        }

        public static byte[] Str2Bytes(string str)
        {
            byte[] b = new byte[str.Length / 2];
            for (int i = 0; i < b.Length; i += 2)
            {
                b[i / 2] = Convert.ToByte(str.Substring(i, 2));
            }

            return b;
        }

        //计算校验和
        public static string CheckSum(string strContent)
        {
            int sum = 0;
            for (int i = 0; i < strContent.Length / 2; i++)
            {
                sum += Convert.ToInt32(strContent.Substring(i * 2, 2), 16);
            }

            sum = sum & 0xFF;
            return sum.ToString("X2");
        }

        public static Dictionary<string, float> Parse_qd_UsersScore2Dic(string data)
        {
            Dictionary<string, float> usdic = new Dictionary<string, float>();
            string[] rs = data.Split(',');
            if (rs.Length % 2 == 0)
            {
                for (int i = 0; i < rs.Length; i += 2)
                {
                    usdic.Add(rs[i], float.Parse(rs[i + 1]));
                }
            }

            return (from udata in usdic orderby udata.Value descending select udata).ToDictionary(s => s.Key,
                s => s.Value);
        }

        public static string AddFileHeader2Str(string str)
        {
#if UNITY_STANDALONE_WIN
            return "file:///" + str;
#else
            return "file://" + str;
#endif
        }

        //包含file：///
        public static string AddStreamAssetsHeader(string str, bool addFileHeader = true)
        {
            string file_header = "";
            if (addFileHeader)
            {
#if UNITY_STANDALONE_WIN
                file_header = "file:///";
#else
                file_header = "file://";
#endif
            }

            return file_header + Path.Combine(Application.streamingAssetsPath, str);
        }

        public class PointClickInfo
        {
            public Collecter pc;
            public string pfault;
            public int ptime;
            public bool isLoop = false;
        }

        public static void CollecterPointClick(Collecter c, string fault, int time = 1000)
        {
            Thread t = new Thread(PointClick);
            t.IsBackground = true;
            t.Start(new PointClickInfo { pc = c, pfault = fault, ptime = time, isLoop = false });
        }

        public static Thread LoopCollecterPointClick(PointClickInfo pci)
        {
            Thread t = new Thread(PointClick);
            t.Priority = System.Threading.ThreadPriority.BelowNormal;
            t.IsBackground = true;
            t.Start(pci);
            return t;
        }

        private static void PointClick(object pci)
        {
            PointClickInfo pointci = pci as PointClickInfo;
            do
            {
                pointci.pc.AddFault(pointci.pfault);
                pointci.pc.SetFault();
                Thread.Sleep(pointci.ptime);
                pointci.pc.RemoveFault(pointci.pfault);
                pointci.pc.SetFault();
            } while (pointci.isLoop);

            //避免关闭不掉
            Thread.Sleep(pointci.ptime);
            pointci.pc.RemoveFault(pointci.pfault);
            pointci.pc.SetFault();
        }

        public static TextAsset Ta_Lang
        {
            get { return ta_Lang ?? Resources.Load<TextAsset>("Lang/Lang"); }
            set { ta_Lang = value; }
        }

        public static string TaConfig
        {
            get
            {
                if (string.IsNullOrEmpty(ta_Config))
                {
#if UNITY_WEBGL
                    using (
                        StreamReader sr =
                            new StreamReader(new FileStream(Path.Combine(Util._configPath, "SystemConfig.json"),
                                FileMode.Open, FileAccess.Read)))
                    {
                        ta_Config = sr.ReadToEnd();
                        sr.Close();
                    }
#else
                Debug.Log("no config data");
                LoadConfigFromFile();
#endif

                }

                return ta_Config;
            }
            set { ta_Config = value; }
        }

        public static Coroutine LoadConfigFromFile()
        {
#if UNITY_EDITOR
            using (FileStream fs = File.Open(Path.Combine(_localConfigPath, "SystemConfig.json"), FileMode.Open, FileAccess.Read))
            {
                StreamReader sr = new StreamReader(fs, Encoding.UTF8);
                TaConfig = sr.ReadToEnd();
                sr.Close();
            }
            return null;
#elif UNITY_STANDALONE || UNITY_ANDROID
            Debug.Log(_configPath + "SystemConfig.json");
            string localConfigPath = _localConfigPath;
            string cfgfile = Path.Combine(localConfigPath, "SystemConfig.json");
            if (!File.Exists(cfgfile))
            {
                if (!Directory.Exists(localConfigPath))
                {
                    Directory.CreateDirectory(localConfigPath);
                }
                FileStream fs = File.Open(cfgfile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.Read);
                return GameRoot.DownLoader.StartCoroutine(GameRoot.DownLoader.LoadAsset((b) =>
                    {
                        if (b != null)
                        {
                            TaConfig = System.Text.Encoding.UTF8.GetString(b, 0, b.Length);
                            fs.Write(b,0,b.Length);
                        }
                        fs.Flush();
                        fs.Close();
                    }, _configPath + "SystemConfig.json"));
            }
            else
            {
                using (FileStream fs = File.Open(cfgfile,FileMode.Open,FileAccess.Read))
                {
                    StreamReader sr = new StreamReader(fs,Encoding.UTF8);
                    TaConfig = sr.ReadToEnd();
                    sr.Close();
                }
                return null;
            }
#elif UNITY_WEBGL
               return GameRoot.DownLoader.StartCoroutine(GameRoot.DownLoader.LoadAsset((b) =>
                {
                    if (b != null)
                    {
                        TaConfig = System.Text.Encoding.UTF8.GetString(b, 0, b.Length);
                    }
                }, _configPath + "SystemConfig.json"));


#endif

        }

        public static string RecordZxlx
        {
            get
            {
                using (
                    StreamReader sr =
                        new StreamReader(new FileStream(Path.Combine(_configPath, "Record.json"), FileMode.Open,
                            FileAccess.Read)))
                {
                    recordZXLX = sr.ReadToEnd();
                    sr.Close();
                }
                return recordZXLX;
            }
            set
            {
                recordZXLX = value;
                try
                {
                    FileStream fs = new FileStream(Path.Combine(_configPath, "Record.json"), FileMode.Truncate,
                        FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                    sw.Write(value);
                    sw.Flush();
                    sw.Close();
                    TaConfig = "";
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    throw;
                }
            }
        }

        public static int GetStringHalfCount(string str)
        {
            Regex regChina = new Regex("^[^\x00-\xFF]");
            int count = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (regChina.IsMatch(str[i].ToString()))
                {
                    count += 2;
                }
                else
                {
                    count++;
                }
            }
            return count;
        }
    }
}