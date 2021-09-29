// #if !UNITY_WEBGL
//
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using System.Runtime.InteropServices;
// using System.Text;
// using Assets.Scripts.WT_FrameWork.Util;
// using UnityEngine;
// public class WT_SuperDog  {
//
//     protected const string defaultScope = "<dogscope />";
//     private const string vendorCodeString =
//                                             "rXNoIbnZ1t7i3BnlKOjCeI/YtH70Eu0CxaRxGMFcYr5KFev9" +
//                                             "LTMYbhKh2Ia2K7KNKxkYFQszBeRM3DfycqaiknlTDbqs89SX" +
//                                             "UL0yb9AaKUE1NBPCd9zchJlbzXeVOofTAeXhj0vg2eUlwuk8" +
//                                             "3I22vvA+oIof0VWJL265UHqPazrBpiLIB5xe/yGhbxGxrT/k" +
//                                             "WMYlTwW19fNWJ+9+K4v6isFsExzl98oxgn6T5R/iK71VU50h" +
//                                             "5/RAuI7/sCw5oxH4oxxtw/1p8ouo4xRYt4bQQFKfnN27tDXL" +
//                                             "rzZe7luIcnFRmSAfsLMJnehJFFjY46bQXpzxJbR8cnGIJioU" +
//                                             "Qf5N0uN54YfbONWEepiwgp1ifS1++iUScy2tVo2HgoRD4z2e" +
//                                             "cRBMsgSJxMVLt4p3wV3WIK82bCjXbXQ0nBK1sQPFanmNvjiV" +
//                                             "cHUXaeCjwp4+azAf0tUI8t4fuf06zzi1Oc8S9mIqdNNIRgzz" +
//                                             "8eeGlypP0Sw8RW7be8vs5O8E0OVQMjQt/CSDVF10SC+AAjMu" +
//                                             "WZDBZg+aSz1xXud+h//QVQgvdwX215v7w5MT0c3+/VhxAOvZ" +
//                                             "doGyc0ENEFNgpm2yt5vLozCDVWPBChrs+dNv7x43DYPfwNyk" +
//                                             "XoJOUsgfAVd+4fj6DXBbRxdTWsqCt74xtQgbUD/uXSNxRqFQ" +
//                                             "Pt7CP00171z2SErjaJ/sdguWHlmMLxHINbmtZTrKnYfZfz73" +
//                                             "YqbayYyLSi+t5sbOvbgowqyIXDEumferJgXqn7MFeXKmIe52" +
//                                             "oKlrHBIVbX/hiEp2NhOg8JNYsvyuFrC0rgIDtt82hJyA5pbm" +
//                                             "uO61qWFfU1yGW0DGycWOfBmOy1ZOTmaV1jooMywvW5Un+mNn" +
//                                             "kxzSyBN7fzZnTXZfZ3FwhJe1yXabiXEM0bL620Sp1fJd8UAJ" +
//                                             "sVFGHyhKEL3Kr1KZK9PvOyFa5Rpy9LbXlsOExGi4n8ETjbA6" +
//                                             "MRBYY73NtIq4C1drDG5+1Q==";
//     protected const int FileId = 0x0001;
//     private Dog dog;
//     public int FeatureID = 1;
//     private string mechanicCode;
//    
//     protected Dog GetDog(int FeatureID)
//     {
//         Dog d = new Dog(new DogFeature(DogFeature.FromFeature(FeatureID).Feature));
//         DogStatus status = d.Login(vendorCodeString, defaultScope);
//         return d.IsLoggedIn() ? d : null;
//     }
//     protected string GetDogFile(int FileId)
//     {
//         if ((null == dog) || !dog.IsLoggedIn())
//             return null;
//
//         DogFile file = dog.GetFile(FileId);
//         if (!file.IsLoggedIn())
//         {
//             // Not logged into a dog - nothing left to do.
//             //MessageBox.Show("未登陆");
//             return null;
//         }
//         // get the file size
//         int size = 0;
//         DogStatus status = file.FileSize(ref size);
//
//
//         if (DogStatus.StatusOk != status)
//         {
//             //MessageBox.Show("文件大小获取失败");
//             return null;
//         }
//
//         //MessageBox.Show("文件大小" + size.ToString());
//
//         // read the contents of the file into a buffer
//         byte[] bytes = new byte[size];
//
//
//         status = file.Read(bytes, 0, bytes.Length);
//
//
//         if (DogStatus.StatusOk != status)
//         {
//             //MessageBox.Show("文件读取失败");
//             return null;
//         }
//         string msg = "";
//         for (int i = 0; i < bytes.Length; i++)
//         {
//             msg = msg + Convert.ToChar(bytes[i]);
//         }
//         Debug.Log(msg);
//         return msg;
//     }
//     public bool LoginDog(ref string mCode)
//     {
//         //lzy 非框架需要修改
//         if (Util.GetSystemConfig("MachineID", "FeatrueID") != null)
//         {
//             FeatureID = int.Parse(Util.GetSystemConfig("MachineID", "FeatrueID"));
//         }
//         else
//         {
//             Debug.Log("未能正确读取配置文件中加密狗相关内容");
//             return false;
//         }
//         //mechanicCode = Util.GetSystemConfig("MachineID", "ID");
//         //加密狗模块已经经过很多测试 放弃只读取一次加密狗  改为正常每次启动软件都检测 与其他软件统一加密逻辑  lzy19/01/31
//         mechanicCode = "0";
//         //end lzy
//         if (mechanicCode=="0")
//         {
//             System.Diagnostics.Process proc = new System.Diagnostics.Process();
//             proc.EnableRaisingEvents = false;
//             proc.StartInfo.FileName = Path.Combine(Application.streamingAssetsPath, "Dog\\MachineCodeProject.exe");
//             proc.StartInfo.CreateNoWindow = true;
//             proc.StartInfo.UseShellExecute = false;
//             proc.StartInfo.RedirectStandardOutput = true;
//             proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
//             proc.Start();
//             mechanicCode = proc.StandardOutput.ReadLine();
//             proc.WaitForExit();
//             //Util.SetSystemConfig("MachineID", "ID", mechanicCode);
//             //Util.SaveSystemConfig();
// #if UNITY_EDITOR
//             Debug.Log(mechanicCode);
// #endif
//         }
//         mCode = mechanicCode;
//         dog = GetDog(FeatureID);
//         if (dog == null || !dog.IsLoggedIn())
//         {
//             return false;
//         }
//         else
//         {
//             string dogString = GetDogFile(FileId);
//             Debug.Log(dogString);
//             if (mechanicCode == dogString)
//             {
//                 //MessageBox.Show("欢迎使用本软件");
//                 return true;
//             }
//             else
//             {
//                 Debug.Log("请使用绑定的超级狗");
//                 return false;
//             }
//         }
//     }
//
//     public void LogOut()
//     {
//         if (dog!=null&&dog.IsLoggedIn())
//         {
//             dog.Logout();
//             dog.Dispose();
//             dog = null;
//         }
//     }
// }
//
// #endif