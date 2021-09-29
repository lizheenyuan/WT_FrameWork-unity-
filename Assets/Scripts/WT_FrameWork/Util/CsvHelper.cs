using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;

namespace Assets.Scripts.WT_FrameWork.Util
{
    public static class CsvHelper
    {
        /// <summary>
        /// 保持数据为csv文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <param name="listModel"></param>
        /// <returns></returns>
        public static bool SaveAsCSV<T>(string fileName, IList<T> listModel) where T : class, new()
        {
            bool flag = false;
            try
            {
                StringBuilder sb = new StringBuilder();
                //通过反射 显示要显示的列
                BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;//反射标识
                Type objType = typeof(T);
                PropertyInfo[] propInfoArr = objType.GetProperties(bf);
                string header = string.Empty;
                List<string> listPropertys = new List<string>();
           
                foreach (PropertyInfo info in propInfoArr)
                {
                    if (string.Compare(info.Name.ToUpper(), "ID") != 0) //不考虑自增长的id或者自动生成的guid等
                    {
                        if (!listPropertys.Contains(info.Name))
                        {
                            listPropertys.Add(info.Name);
                        }
                        header += info.Name + ",";
                    }
                }
                sb.AppendLine(header.Trim(',')); //csv头
                if (listModel!=null)
                {
                
                    foreach (T model in listModel)
                {
                    if (model == null) continue;
                    string strModel = string.Empty;
                    foreach (string strProp in listPropertys)
                    {
                        foreach (PropertyInfo propInfo in propInfoArr)
                        {
                            if (string.Compare(propInfo.Name.ToUpper(), strProp.ToUpper()) == 0)
                            {
                                PropertyInfo modelProperty = model.GetType().GetProperty(propInfo.Name);
                                if (modelProperty != null)
                                {
                                    object objResult = modelProperty.GetValue(model, null);
                                    string result = ((objResult == null) ? string.Empty : objResult).ToString().Trim();
                                    if (result.IndexOf(',') != -1)
                                    {
                                        result = "\"" + result.Replace("\"", "\"\"") + "\""; //特殊字符处理 ？
                                        //result = result.Replace("\"", "“").Replace(',', '，') + "\"";
                                    }
                                    if (!string.IsNullOrEmpty(result))
                                    {
                                        Type valueType = modelProperty.PropertyType;
                                        if (valueType.Equals(typeof(Nullable<decimal>)))
                                        {
                                            result = decimal.Parse(result).ToString("#.#");
                                        }
                                        else if (valueType.Equals(typeof(decimal)))
                                        {
                                            result = decimal.Parse(result).ToString("#.#");
                                        }
                                        else if (valueType.Equals(typeof(Nullable<double>)))
                                        {
                                            result = double.Parse(result).ToString("#.#");
                                        }
                                        else if (valueType.Equals(typeof(double)))
                                        {
                                            result = double.Parse(result).ToString("#.#");
                                        }
                                        else if (valueType.Equals(typeof(Nullable<float>)))
                                        {
                                            result = float.Parse(result).ToString("#.#");
                                        }
                                        else if (valueType.Equals(typeof(float)))
                                        {
                                            result = float.Parse(result).ToString("#.#");
                                        }
                                    }
                                    strModel += result + ",";
                                }
                                else
                                {
                                    strModel += ",";
                                }
                                break;
                            }
                        }
                    }

                    strModel = strModel.Substring(0, strModel.Length - 1);
                    sb.AppendLine(strModel);
                }

                }
                string content = sb.ToString();
                //string dir = Directory.GetCurrentDirectory();
                //string dir = Application.persistentDataPath;//持久数据存储路径
                //string dir = SimpleFramework.Util.DataPath;
                //if (Application.isEditor)
                //{
                //    dir = Application.streamingAssetsPath + "/../GameRes/PrefabRes/packlist/";
                //}

                //string fullName = Path.Combine(dir, "/"+fileName);
                string fullName =  fileName;
                if (File.Exists(fullName)) File.Delete(fullName);
                using (FileStream fs = new FileStream(fullName, FileMode.CreateNew, FileAccess.Write))
                {
                    StreamWriter sw = new StreamWriter(fs, Encoding.Default);
                    sw.Flush();
                    sw.Write(content);
                    sw.Flush();
                    sw.Close();
                }
                flag = true;
            }
            catch (Exception e)
            {
                //Debug.LogError("KKException:"+e.ToString() +"/n"+e.StackTrace);
                Console.WriteLine(e.ToString()+"/n"+e.StackTrace);
                flag = false;
            }
            return flag;
        }

        /// <summary>
      /// 将CSV文件的数据读取到DataTable中
      /// <param name="fileName">CSV文件路径</param>
      /// <returns>返回读取了CSV数据的DataTable</returns>
      /// </summary>
        public static DataTable OpenCSV(string filePath)
      {
          Encoding encoding = Encoding.Default; //Encoding.ASCII;//
          DataTable dt = new DataTable();
          FileStream fs = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
          
          //StreamReader sr = new StreamReader(fs, Encoding.UTF8);
          StreamReader sr = new StreamReader(fs, encoding);
          //string fileContent = sr.ReadToEnd();
          //encoding = sr.CurrentEncoding;
          //记录每次读取的一行记录
          string strLine = "";
          //记录每行记录中的各字段内容
          string[] aryLine = null;
          string[] tableHead = null;
          //标示列数
          int columnCount = 0;
          //标示是否是读取的第一行
          bool IsFirst = true;
          //逐行读取CSV中的数据
          while ((strLine = sr.ReadLine()) != null)
          {
              //strLine = Common.ConvertStringUTF8(strLine, encoding);
              //strLine = Common.ConvertStringUTF8(strLine);
  
              if (IsFirst == true)
              {
                  tableHead = strLine.Split(',');
                  IsFirst = false;
                  columnCount = tableHead.Length;
                  //创建列
                  for (int i = 0; i < columnCount; i++)
                  {
                      DataColumn dc = new DataColumn(tableHead[i]);
                     dt.Columns.Add(dc);
                 }
             }
             else
             {
                 aryLine = strLine.Split(',');
                 DataRow dr = dt.NewRow();
                 for (int j = 0; j < columnCount; j++)
                 {
                     dr[j] = aryLine[j];
                 }
                 dt.Rows.Add(dr);
             }
         }
         if (aryLine != null && aryLine.Length > 0)
         {
             dt.DefaultView.Sort = tableHead[0] + " " + "asc";
         }
         
         sr.Close();
         fs.Close();
         return dt;
        }

        public static IList<T> OpenCSV<T>(byte[] csvdata) where T : new()
        {
            MemoryStream ms = new MemoryStream(csvdata);
            return OpenCSV<T>(ms);
        }

        static IList<T> OpenCSV<T>(Stream stream) where T : new()
        {
            Encoding encoding = Encoding.Default; //Encoding.ASCII;//
            //StreamReader sr = new StreamReader(fs, Encoding.UTF8);
            StreamReader sr = new StreamReader(stream, encoding);
            //string fileContent = sr.ReadToEnd();
            //encoding = sr.CurrentEncoding;
            //记录每次读取的一行记录
            string strLine = "";
            //记录每行记录中的各字段内容
            string[] aryLine = null;
            string[] tableHead = null;
            //标示列数
            int columnCount = 0;
            //标示是否是读取的第一行
            bool IsFirst = true;

            #region 获取字段

            //通过反射 显示要显示的列
            BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;//反射标识
            Type objType = typeof(T);
            PropertyInfo[] propInfoArr = objType.GetProperties(bf);
            string header = string.Empty;
            List<string> listPropertys = new List<string>();

            foreach (PropertyInfo info in propInfoArr)
            {
                if (string.Compare(info.Name.ToUpper(), "ID") != 0) //不考虑自增长的id或者自动生成的guid等
                {
                    if (!listPropertys.Contains(info.Name))
                    {
                        listPropertys.Add(info.Name);
                    }
                }
            }

            #endregion
            IList<T> objList = new List<T>();
            //逐行读取CSV中的数据
            while ((strLine = sr.ReadLine()) != null)
            {
                //strLine = Common.ConvertStringUTF8(strLine, encoding);
                //strLine = Common.ConvertStringUTF8(strLine);

                if (IsFirst == true)
                {
                    IsFirst = false;
                }
                else
                {
                    aryLine = strLine.Split(',');
                    T t = new T();
                    for (int i = 0; i < listPropertys.Count; i++)
                    {
                        SetModelValue(listPropertys[i], aryLine[i], t);
                    }
                    objList.Add(t);
                }
            }

            sr.Close();
            stream.Close();
            return objList;
        }
        public static IList<T> OpenCSV<T>(string filePath) where T : new()
        {
            FileStream fs = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            return OpenCSV<T>(fs);
        }
        public static IList<T> OpenCsv<T>(string filePath) where T : new()
        {
            FileStream fs = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            return OpenCSV<T>(fs);
        }
        public static bool SetModelValue(string FieldName, string Value, object obj)
        {
            try
            {
                Type Ts = obj.GetType();
                object v = Convert.ChangeType(Value, Ts.GetProperty(FieldName)?.PropertyType);
                Ts.GetProperty(FieldName)?.SetValue(obj, v, null);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
