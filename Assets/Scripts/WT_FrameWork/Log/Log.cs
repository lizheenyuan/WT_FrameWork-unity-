using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using LitJson;

namespace WT_FrameWork
{
    public class Log
    {
        private static Log _instance;
        public static Log Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Log();
                }
                return _instance;
            }
        }

        private string _crashLogPath;
        public void Init()
        {
#if UNITY_EDITOR
            _crashLogPath = Application.dataPath + @"/../Log";
#else
            _crashLogPath = Application.persistentDataPath + @"/Log";
#endif
            if (!Directory.Exists(_crashLogPath))
            {
                Directory.CreateDirectory(_crashLogPath);
            }
            _crashLogPath += "/crash.txt";
            if (File.Exists(_crashLogPath))
            {
                string text = File.ReadAllText(_crashLogPath);
                if (!String.IsNullOrEmpty(text))
                {
                    //
                }
                //File.Delete(_crashLogPath);
            }
            else
            {
                File.Create(_crashLogPath).Dispose();
            }            

            AppDomain curDomain = AppDomain.CurrentDomain;
            curDomain.UnhandledException += curDomain_UnhandledException;

            Application.RegisterLogCallback(new Application.LogCallback(CaptureLog));
        }

        void curDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;
            if (exception != null)
            {
                CaptureLog(exception.Message, exception.StackTrace, LogType.Error);
            }
        }

        JsonData GetDeviceInfo()
        {   
            JsonData detail = new JsonData();

            DateTime now = System.DateTime.UtcNow;
            detail["LogTime"] = now.Year + "/" + now.Month + "/" + now.Day + "  " + 
                now.Hour + ":" + now.Minute + ":" + +now.Second + ":" + now.Millisecond;

            detail["DeviceModel"] = SystemInfo.deviceModel;
            detail["DeviceType"] = (int)SystemInfo.deviceType;
            detail["DeviceUniqureIndentifier"] = SystemInfo.deviceUniqueIdentifier;
            detail["DeviceMemorySize"] = SystemInfo.systemMemorySize;
            detail["GraphicalMemorySize"] = SystemInfo.graphicsMemorySize;
            detail["ProcessName"] = SystemInfo.processorType;
            detail["ProcessCount"] = SystemInfo.processorCount;
            detail["OS"] = SystemInfo.operatingSystem;
            
            //string Result = "======================CRASH INFO===========================" + "\r\n"
            //    + " [CREATE CRASH LOG TIME] ---Time :" + now.Year + "/" + now.Month + "/" + now.Day + "  " + now.Hour + ":" + now.Minute + ":" + +now.Second + ":" + now.Millisecond + "\r\n"
            //    + " [DEVICE MODEL]  " + SystemInfo.deviceModel + "\r\n"
            //    + " [DEVICE TYPE] " + SystemInfo.deviceType + "\r\n"
            //    + " [DEVICE UNIQUE INDENTIFIER] " + SystemInfo.deviceUniqueIdentifier + "\r\n"
            //    + " [DEVICE MEMORY SIZE] " + SystemInfo.systemMemorySize + "\r\n"
            //    + " [GRAPHICAL MEMORY SIZE] " + SystemInfo.graphicsMemorySize + "\r\n"
            //    + " [PROCESSOR NAME] " + SystemInfo.processorType + "\r\n"
            //    + " [DEVICE PROCESSOR COUNT] " + SystemInfo.processorCount + "\r\n"
            //    + " [OS] " + SystemInfo.operatingSystem + "\r\n"
            //    + "===================================================================" + "\r\n"
            //    + "\r\n";
            return detail;
        }

        void CaptureLog(string condition, string stacktrace, LogType type)
        {
#if UNITY_ANDROID
            if (type == LogType.Exception || type == LogType.Error)
#else
            if (type == LogType.Exception || type == LogType.Error)
#endif
            {
                JsonData result = GetDeviceInfo();
                result["Log"] = condition;
                result["StackTrace"] = stacktrace;
                string log = result.ToJson();
                CreateLocalLogFile(log);
            }
        }        

        void CreateLocalLogFile(string LogString)
        {
            if (File.Exists(_crashLogPath))
            {
                try
                {
                    StreamWriter write = File.AppendText(_crashLogPath);
                    write.Write(LogString);
                    write.Flush();
                    write.Close();
                }
                catch (System.Exception ex)
                {
                    string error = ex.Message;
                    LogInfo(error);
                }
            }
        }
        
        public static void LogInfo(object message)
        {
            UnityEngine.Debug.Log(message);
        }

        public static void LogInfo(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.Log(message, context);
        }

        public static void LogError(object message)
        {
            UnityEngine.Debug.LogError(message);
        }

        public static void LogError(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogError(message, context);
        }

        public static void LogException(Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }

        public static void LogException(Exception exception, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogException(exception, context);
        }

        public static void LogWarning(object message)
        {
            UnityEngine.Debug.LogWarning(message);
        }
        public static void LogWarning(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogWarning(message, context);
        }
    }
}
