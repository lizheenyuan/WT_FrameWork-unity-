//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Assets.Scripts.Public;
//
//namespace Assets.Scripts.Protocol
//{
//    public delegate void OnStrikeDetectEvent(int devID, string devName, int value1, int value2, int value3, int value4);
//     public class StrikeDetect:WTClientSocket
//    {
//        string fReceiveBuffer; //接收缓存区
//        public OnStrikeDetectEvent onStrikeDetectEvent;
//        protected override void ReSolveReceiverData(string strReceiveData)
//        {
//            fReceiveBuffer += strReceiveData;
//            //数据长度字节21
//            int frameLength = 21;
//            //帧数据长度不够，返回
//            if (fReceiveBuffer.Length < frameLength * 2)
//            {
//                return;
//            }
//            int iPosition = -1;
//            iPosition = fReceiveBuffer.IndexOf("0C0304");
//            if(iPosition < 0)
//            {
//                fReceiveBuffer = "";
//                return;
//            }
//            string frameContent = fReceiveBuffer.Substring(iPosition, frameLength * 2);
//            fReceiveBuffer = fReceiveBuffer.Substring(iPosition + frameLength * 2, fReceiveBuffer.Length - iPosition - frameLength * 2);
//            if (frameContent.Substring(frameLength * 2 - 4, 4) == PubFunction.GetDataCRC(frameContent.Substring(0, frameLength * 2 - 4)))
//            {
//                int value1 = int.Parse(frameContent.Substring(6 , 2)) * 1000 + int.Parse(frameContent.Substring(8, 2)) * 100 + int.Parse(frameContent.Substring(10, 2)) * 10 + int.Parse(frameContent.Substring(12, 2));
//                int value2 = int.Parse(frameContent.Substring(14, 2)) * 1000 + int.Parse(frameContent.Substring(16, 2)) * 100 + int.Parse(frameContent.Substring(18, 2)) * 10 + int.Parse(frameContent.Substring(20, 2));
//                int value3 = int.Parse(frameContent.Substring(22, 2)) * 1000 + int.Parse(frameContent.Substring(24, 2)) * 100 + int.Parse(frameContent.Substring(26, 2)) * 10 + int.Parse(frameContent.Substring(28, 2));
//                int value4 = int.Parse(frameContent.Substring(30, 2)) * 1000 + int.Parse(frameContent.Substring(32, 2)) * 100 + int.Parse(frameContent.Substring(34, 2)) * 10 + int.Parse(frameContent.Substring(36, 2));
//
//                if (onStrikeDetectEvent != null)
//                {
//                    onStrikeDetectEvent(DevID, DevName, value1, value2, value3, value4);
//                }
//                //如果缓存还有数据，递归调用
//                if(fReceiveBuffer.Length >= frameLength * 2)
//                {
//                    ReSolveReceiverData("");
//                }
//            }
//            
//        }
//    }
//}
