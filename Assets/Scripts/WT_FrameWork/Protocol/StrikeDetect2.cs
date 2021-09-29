using System;
using Assets.Scripts.Public;

namespace Assets.Scripts.WT_FrameWork.Protocol
{
	public delegate void OnStrikeDetect2Event(int devID, string devName, int value1);

	public class StrikeDetect2:WTClientSocket
	{
		
		string fReceiveBuffer; //接收缓存区
		public event OnStrikeDetect2Event onStrikeDetectEvent;
		public event OnStrikeDetect2Event onChangeNumFactorEvent;
		protected override void ReSolveReceiverData(string strReceiveData)
		{
			fReceiveBuffer += strReceiveData;
			//数据长度字节8
			int frameLength = 8;
			//帧数据长度不够，返回
			if (fReceiveBuffer.Length < frameLength * 2)
			{
				return;
			}
			int iPosition = -1;
			iPosition = fReceiveBuffer.IndexOf("AA0000");
			if (iPosition >= 0)
			{
				string frameContent = fReceiveBuffer.Substring (iPosition, frameLength * 2);
				fReceiveBuffer = fReceiveBuffer.Substring (iPosition + frameLength * 2, fReceiveBuffer.Length - iPosition - frameLength * 2);
				if (frameContent.Substring (frameLength * 2 - 4, 2) == PubFunction.CheckSum (frameContent.Substring (2, 10))) {
					int value1 = Convert.ToInt32 (frameContent.Substring (8, 4), 16);
					//Debug.Log (value1);
					//				int value1 = int.Parse(frameContent.Substring(6 , 2)) * 1000 + int.Parse(frameContent.Substring(8, 2)) * 100 + int.Parse(frameContent.Substring(10, 2)) * 10 + int.Parse(frameContent.Substring(12, 2));
					//				int value2 = int.Parse(frameContent.Substring(14, 2)) * 1000 + int.Parse(frameContent.Substring(16, 2)) * 100 + int.Parse(frameContent.Substring(18, 2)) * 10 + int.Parse(frameContent.Substring(20, 2));
					//				int value3 = int.Parse(frameContent.Substring(22, 2)) * 1000 + int.Parse(frameContent.Substring(24, 2)) * 100 + int.Parse(frameContent.Substring(26, 2)) * 10 + int.Parse(frameContent.Substring(28, 2));
					//				int value4 = int.Parse(frameContent.Substring(30, 2)) * 1000 + int.Parse(frameContent.Substring(32, 2)) * 100 + int.Parse(frameContent.Substring(34, 2)) * 10 + int.Parse(frameContent.Substring(36, 2));

					if (onStrikeDetectEvent != null) {
						onStrikeDetectEvent (DevID, DevName, value1);
					}
					//如果缓存还有数据，递归调用
					if (fReceiveBuffer.Length >= frameLength * 2) {
						ReSolveReceiverData ("");
					}
				}

			} 
			else
			{
				iPosition = fReceiveBuffer.IndexOf("AA0002");
				if (iPosition >= 0)
				{
					string frameContent = fReceiveBuffer.Substring (iPosition, frameLength * 2);
					fReceiveBuffer = fReceiveBuffer.Substring (iPosition + frameLength * 2, fReceiveBuffer.Length - iPosition - frameLength * 2);
					if (frameContent.Substring (frameLength * 2 - 4, 2) == PubFunction.CheckSum (frameContent.Substring (2, 10))) {
						int value1 = Convert.ToInt32 (frameContent.Substring (8, 4), 16);
						//Debug.Log (value1);
						//				int value1 = int.Parse(frameContent.Substring(6 , 2)) * 1000 + int.Parse(frameContent.Substring(8, 2)) * 100 + int.Parse(frameContent.Substring(10, 2)) * 10 + int.Parse(frameContent.Substring(12, 2));
						//				int value2 = int.Parse(frameContent.Substring(14, 2)) * 1000 + int.Parse(frameContent.Substring(16, 2)) * 100 + int.Parse(frameContent.Substring(18, 2)) * 10 + int.Parse(frameContent.Substring(20, 2));
						//				int value3 = int.Parse(frameContent.Substring(22, 2)) * 1000 + int.Parse(frameContent.Substring(24, 2)) * 100 + int.Parse(frameContent.Substring(26, 2)) * 10 + int.Parse(frameContent.Substring(28, 2));
						//				int value4 = int.Parse(frameContent.Substring(30, 2)) * 1000 + int.Parse(frameContent.Substring(32, 2)) * 100 + int.Parse(frameContent.Substring(34, 2)) * 10 + int.Parse(frameContent.Substring(36, 2));

						if (onChangeNumFactorEvent != null) {
							onChangeNumFactorEvent (DevID, DevName, value1);
						}
						//如果缓存还有数据，递归调用
						if (fReceiveBuffer.Length >= frameLength * 2) {
							ReSolveReceiverData ("");
						}
					}

				} 
			}
		}

		public void ReadFactor()
		{
			string strCMD = "AA00020200000455" ;
			SendData(strCMD);
		}

		public void SaveFactor(int num)
		{
			
			string strFrameHead = "AA";
			string strAddress = "00";
			string strFunctionCode = "01";
			string strDataLength = "02";
			string strValue = num.ToString("X4");
			string strCheckout = PubFunction.CheckSum(strAddress+strFunctionCode+strDataLength+strValue);
			string strFrameLast = "55";
			string strCMD = strFrameHead + strAddress + strFunctionCode + strDataLength + strValue + strCheckout +strFrameLast;
			SendData(strCMD);
		}
	}
}
