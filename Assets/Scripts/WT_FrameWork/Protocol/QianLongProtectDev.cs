using System;
using Assets.Scripts.Public;

namespace Assets.Scripts.WT_FrameWork.Protocol
{
    public delegate void OnQianLongProtectDataEvent(int devID, string devName, string devAddr, string dataNode, string data);
    public class QianLongProtectDev : WTClientSocket
    {
        private string fReceiveBuffer; //接收数据缓存

        public int ProtectType { get; set; } //协议类型 0:97协议 1:07协议

        public event OnQianLongProtectDataEvent onQianLongProtectDataEvent;

        public QianLongProtectDev():base()
        {
            ProtectType = 0;
        }

        private string Add33H(string strHexData)
        {
            string strValue = "";
            for (int i = 0; i < strHexData.Length / 2; i++)
            {
                strValue += ((Convert.ToInt32(strHexData.Substring(2 * i, 2), 16) + 0x33) & 0xff).ToString("X2");
            }
            return strValue;
        }

        private string Dec33H(string strHexData)
        {
            string strValue = "";
            for (int i = 0; i < strHexData.Length / 2; i++)
            {
                strValue += ((Convert.ToInt32(strHexData.Substring(2 * i, 2), 16) + 0xCD) & 0xff).ToString("X2");
            }
            return strValue;
        }

        private string ReverseData(string strHexData)
        {
            string strValue = "";
            for (int i = 0; i < strHexData.Length / 2; i++)
            {
                strValue += strHexData.Substring(strHexData.Length - 2 * i - 2, 2);
            }
            return strValue;
        }

        protected override void ReSolveReceiverData(string strReceiveData)
        {
            fReceiveBuffer += strReceiveData;
            if (fReceiveBuffer.Length > 400)
            {
                fReceiveBuffer = "";
                return;
            }
            if(ProtectType == 0)
            {
                ReSolveReceiverData_1997();
            }
            else
            {
                ReSolveReceiverData_2007();
            }
        }

        private void ReSolveReceiverData_1997()
        {
            if (fReceiveBuffer.Length < 24)
            {
                return;
            }
            int num68Index = fReceiveBuffer.IndexOf("68");
            if (num68Index < 0)
            {
                return;
            }
            else if (num68Index > 0)
            {
                fReceiveBuffer = fReceiveBuffer.Substring(num68Index, fReceiveBuffer.Length - num68Index);
            }
            if (fReceiveBuffer.Substring(14, 2) != "68")
            {
                fReceiveBuffer = string.Empty;
                return;
            }
            string strAddr = fReceiveBuffer.Substring(2, 12);
            strAddr = ReverseData(strAddr);
            strAddr = strAddr.Replace("AA", "");
            int dataLen = Convert.ToInt32(strAddr.Substring(18, 2));
            //判断数据是否完整
            if (fReceiveBuffer.Length < 24 + dataLen)
            {
                return;
            }
            string strTemp = fReceiveBuffer.Substring(16, 2);
            switch (strTemp)
            {
                case "C1":
                    {
                        fReceiveBuffer = "";
                        if (onQianLongProtectDataEvent != null)
                        {
                            onQianLongProtectDataEvent(DevID, DevName, strAddr, "读数据异常应答", "");
                        }
                        return;
                    }
                case "C4":
                    {
                        fReceiveBuffer = "";
                        if (onQianLongProtectDataEvent != null)
                        {
                            onQianLongProtectDataEvent(DevID, DevName, strAddr, "写数据异常应答", "");
                        }
                        return;
                    }
                case "84":
                    {
                        fReceiveBuffer = "";
                        if (onQianLongProtectDataEvent != null)
                        {
                            onQianLongProtectDataEvent(DevID, DevName, strAddr, "写数据正常应答", "");
                        }
                        return;
                    }
                case "8A":
                    {
                        fReceiveBuffer = "";
                        if (onQianLongProtectDataEvent != null)
                        {
                            onQianLongProtectDataEvent(DevID, DevName, strAddr, "写设备地址正常应答", "");
                        }
                        break;
                    }
                case "81":
                    {
                        string strData = fReceiveBuffer.Substring(20, dataLen * 2);
                        InterpretData_1997(strAddr, strData);
                        fReceiveBuffer = "";
                        break;
                    }
            }
        }

        private void InterpretData_1997(string strAddr, string strData)
        {
            string dataMask = Dec33H(ReverseData(strData.Substring(0, 4)));
            string dataState = Dec33H(strData.Substring(4, 2));
            string data = Dec33H(strData.Substring(6, strData.Length - 6));

            #region 状态处理
            string strStateTemp = "无效数据相数";
            switch (Convert.ToInt32(dataState.Substring(0, 1)))
            {
                case 1:
                    {
                        strStateTemp = "A相合闸未锁";
                        break;
                    }
                case 2:
                    {
                        strStateTemp = "B相合闸未锁";
                        break;
                    }
                case 3:
                    {
                        strStateTemp = "C相合闸未锁";
                        break;
                    }
                case 5:
                    {
                        strStateTemp = "A相合闸锁死";
                        break;
                    }
                case 6:
                    {
                        strStateTemp = "B相合闸锁死";
                        break;
                    }
                case 7:
                    {
                        strStateTemp = "C相合闸锁死";
                        break;
                    }
                case 9:
                    {
                        strStateTemp = "A相分闸未锁";
                        break;
                    }
                case 10:
                    {
                        strStateTemp = "B相分闸未锁";
                        break;
                    }
                case 11:
                    {
                        strStateTemp = "C相分闸未锁";
                        break;
                    }
                case 13:
                    {
                        strStateTemp = "A相分闸锁死";
                        break;
                    }
                case 14:
                    {
                        strStateTemp = "B相分闸锁死";
                        break;
                    }
                case 15:
                    {
                        strStateTemp = "C相分闸锁死";
                        break;
                    }
            }
            switch (Convert.ToInt32(dataState.Substring(1, 1)))
            {
                case 0:
                    {
                        strStateTemp += "漏电跳闸";
                        break;
                    }
                case 1:
                    {
                        strStateTemp += "突变跳闸";
                        break;
                    }
                case 2:
                    {
                        strStateTemp += "特波跳闸";
                        break;
                    }
                case 3:
                    {
                        strStateTemp += "过载跳闸";
                        break;
                    }
                case 4:
                    {
                        strStateTemp += "过压跳闸";
                        break;
                    }
                case 5:
                    {
                        strStateTemp += "欠压跳闸";
                        break;
                    }
                case 6:
                    {
                        strStateTemp += "短路跳闸";
                        break;
                    }
                case 7:
                    {
                        strStateTemp += "手动跳闸";
                        break;
                    }
                case 8:
                    {
                        strStateTemp += "停电跳闸";
                        break;
                    }
                case 9:
                    {
                        strStateTemp += "互感器故障跳闸";
                        break;
                    }
                case 10:
                    {
                        strStateTemp += "远程跳闸";
                        break;
                    }
                case 11:
                    {
                        strStateTemp += "其它原因跳闸";
                        break;
                    }
                case 12:
                    {
                        strStateTemp += "合闸过程中";
                        break;
                    }
                case 13:
                    {
                        strStateTemp += "合闸失败";
                        break;
                    }

            }
            #endregion

            #region 数据解析
            string strTempData = "";
            string strNodeName = "";
            if(dataMask == "B66F") // XXXX XXXX XXXX XXXX XXXX XXXX XXXX       三相电压电流，剩余电流
            {
                strTempData += RemoveIntFrontZero(ReverseData(data.Substring(0, 4))) + "/"; //Ua
                strTempData += RemoveIntFrontZero(ReverseData(data.Substring(4, 4))) + "/"; //Ub
                strTempData += RemoveIntFrontZero(ReverseData(data.Substring(8, 4))) + "/"; //Uc
                strTempData += RemoveIntFrontZero(ReverseData(data.Substring(12, 4))) + "/"; //Ia
                strTempData += RemoveIntFrontZero(ReverseData(data.Substring(16, 4))) + "/"; //Ib
                strTempData += RemoveIntFrontZero(ReverseData(data.Substring(20, 4))) + "/"; //Ic
                strTempData += RemoveIntFrontZero(ReverseData(data.Substring(24, 4)));       //Is
                strNodeName = "三相电压电流剩余电流";
            }
            else if (dataMask == "C012") // ssmmhhWWDDMMYY    日期时间
            {
                strTempData = ReverseData(data.Substring(0,14));
                strNodeName = "日期时间";
            }
            else if (dataMask == "C032")
            {
                strTempData = ReverseData(data);
                strNodeName = "设备地址";
            }
            else if (dataMask == "C040") // NN+XXXX+mmhhDDMM 当前开关信息及动作值
            {
                strTempData += data.Substring(0,2) + "/";
                strTempData += ReverseData(data.Substring(2, 4)) + "/";
                strTempData += ReverseData(data.Substring(6,8));
                strNodeName = "开关信息及动作值";
            }
            else if (dataMask == "C04F") // XXX XXXX XXXX NN NN  全部参数
            {
                strTempData += ReverseData(data.Substring(0, 4)) + "/";
                strTempData += ReverseData(data.Substring(6, 4)) + "/";
                strTempData += ReverseData(data.Substring(12, 4)) + "/";
                strTempData += data.Substring(16, 2) + "/";
                strTempData += data.Substring(18, 2);
                strNodeName = "全部参数";
            }
            else if(Convert.ToInt32(dataMask,16) > 58639 && Convert.ToInt32(dataMask, 16) < 58661)
            {
                strTempData += (Convert.ToInt32(dataMask, 16) - 58639).ToString() + "/";
                strTempData += ReverseData(data.Substring(0, 4)) + "/";
                strTempData += ReverseData(data.Substring(4, 8)) + "/";
                strTempData += strStateTemp;
                strNodeName = "最近20次跳闸";
            }
            else
            {
                return;
            }
            #endregion

            if (onQianLongProtectDataEvent != null)
            {
                onQianLongProtectDataEvent(DevID,DevName,strAddr,strNodeName,strTempData);
            }

        }

        private void ReSolveReceiverData_2007()
        {
            if (fReceiveBuffer.Length < 24)
            {
                return;
            }
            int num68Index = fReceiveBuffer.IndexOf("68");
            if (num68Index < 0)
            {
                return;
            }
            else if (num68Index > 0)
            {
                fReceiveBuffer = fReceiveBuffer.Substring(num68Index, fReceiveBuffer.Length - num68Index);
            }
            if (fReceiveBuffer.Substring(14, 2) != "68")
            {
                fReceiveBuffer = string.Empty;
                return;
            }
            string strAddr = fReceiveBuffer.Substring(2, 12);
            strAddr = ReverseData(strAddr);
            strAddr = strAddr.Replace("AA", "");
            int dataLen = Convert.ToInt32(strAddr.Substring(18, 2));
            //判断数据是否完整
            if (fReceiveBuffer.Length < 24 + dataLen)
            {
                return;
            }
            string strTemp = fReceiveBuffer.Substring(16, 2);
            switch (strTemp)
            {
                case "D1":
                    {
                        fReceiveBuffer = "";
                        if (onQianLongProtectDataEvent != null)
                        {
                            onQianLongProtectDataEvent(DevID, DevName, strAddr, "读数据异常应答", "");
                        }
                        return;
                    }
                case "D4":
                    {
                        fReceiveBuffer = "";
                        if (onQianLongProtectDataEvent != null)
                        {
                            onQianLongProtectDataEvent(DevID, DevName, strAddr, "写数据异常应答", "");
                        }
                        return;
                    }
                case "94":
                    {
                        fReceiveBuffer = "";
                        if (onQianLongProtectDataEvent != null)
                        {
                            onQianLongProtectDataEvent(DevID, DevName, strAddr, "写数据正常应答", "");
                        }
                        return;
                    }
                case "93":
                    {
                        fReceiveBuffer = "";
                        if (onQianLongProtectDataEvent != null)
                        {
                            onQianLongProtectDataEvent(DevID, DevName, strAddr, "读地址正常应答", "");
                        }
                        break;
                    }
                case "91":
                    {
                        string strData = fReceiveBuffer.Substring(20, dataLen * 2);
                        InterpretData_2007(strAddr, strData);
                        fReceiveBuffer = "";
                        break;
                    }
            }
        }

        private void InterpretData_2007(string strAddr, string strData)
        {
            string dataMask = Dec33H(ReverseData(strData.Substring(0, 8)));
            string data = Dec33H(strData.Substring(8, strData.Length - 8));

            #region 数据解析
            string strTempData = "";
            string strNodeName = "";
            if (dataMask == "0201FF00") //三相电压
            {
                strTempData += RemoveFloatFrontZero(ReverseData(data.Substring(0, 4)), 1) + "/";
                strTempData += RemoveFloatFrontZero(ReverseData(data.Substring(4, 4)), 1) + "/";
                strTempData += RemoveFloatFrontZero(ReverseData(data.Substring(8, 4)), 1);
                strNodeName = "当前三相电压值";
            }
            else if (dataMask == "0202FF00") //三相电流
            {
                strTempData += RemoveFloatFrontZero(ReverseData(data.Substring(0, 6)), 1) + "/";
                strTempData += RemoveFloatFrontZero(ReverseData(data.Substring(6, 6)), 1) + "/";
                strTempData += RemoveFloatFrontZero(ReverseData(data.Substring(12, 6)), 1);
                
                strNodeName = "当前三相电流值";
            }
            else if (dataMask == "02900100") //当前剩余电流值
            {
                strTempData += ReverseData(data.Substring(0, 4));
                strNodeName = "当前剩余电流值";
            }
            else if (dataMask == "02910100") //当前额定剩余电流动作值
            {
                strTempData += ReverseData(data.Substring(0, 4));
                strNodeName = "当前额定剩余电流动作值";
            }
            else if (dataMask == "04000411") 
            {
                strTempData += ReverseData(data.Substring(0, 4)) + "/";
                strTempData += ReverseData(data.Substring(4, 4)) + "/";
                strTempData += ReverseData(data.Substring(8, 4)) + "/";
                strTempData += ReverseData(data.Substring(12, 4)) + "/";
                strTempData += ReverseData(data.Substring(16, 4)) + "/";
                strTempData += ReverseData(data.Substring(20, 4)) + "/";
                strTempData += ReverseData(data.Substring(24, 4)) + "/";
                strTempData += ReverseData(data.Substring(28, 4)) + "/";
                strTempData += ReverseData(data.Substring(32, 4)) + "/";
                strTempData += ReverseData(data.Substring(36, 4));
                strNodeName = "额定剩余电流动作值数组";
            }
            else if (dataMask == "04000413")
            {
                strTempData += ReverseData(data.Substring(0, 4)) + "/";
                strTempData += ReverseData(data.Substring(4, 4)) + "/";
                strTempData += ReverseData(data.Substring(8, 4)) + "/";
                strTempData += ReverseData(data.Substring(12, 4)) + "/";
                strTempData += ReverseData(data.Substring(16, 4));
                strNodeName = "额定分断时间参数数组";
            }
            else if (dataMask == "04001401")
            {
                strTempData += RemoveFloatFrontZero(ReverseData(data.Substring(0, 4)), 1);
                strNodeName = "额定电流整定值";
            }
            else if (dataMask == "040005FF")
            {
                string strTemp = "";
                strTemp = ReverseData(data.Substring(0, 1));
                strTemp = Convert.ToString(Convert.ToInt32(strTemp,16),2);
                if(strTemp.Substring(0,1) == "1")
                {
                    strTempData += "有告警，";
                }
                else
                {
                    strTempData += "无告警，";
                }

                if(strTemp.Substring(1,2) == "00")
                {
                    strTempData += "合闸，";
                }
                else if (strTemp.Substring(1, 2) == "10")
                {
                    strTempData += "重合闸，";
                }
                else if (strTemp.Substring(1, 2) == "11")
                {
                    strTempData += "跳闸，";
                }

                if(strTemp.Substring(3,5) == "00000")
                {
                    strTempData += "正常运行，";
                }
                else if (strTemp.Substring(3, 5) == "00010")
                {
                    strTempData += "告警原因：剩余电流跳闸，";
                }
                else if (strTemp.Substring(3, 5) == "00101")
                {
                    strTempData += "告警原因：过载跳闸，";
                }
                else if (strTemp.Substring(3, 5) == "00110")
                {
                    strTempData += "告警原因：短路跳闸，";
                }
                else if (strTemp.Substring(3, 5) == "00111")
                {
                    strTempData += "告警原因：缺相，";
                }
                else if (strTemp.Substring(3, 5) == "01000")
                {
                    strTempData += "告警原因：欠压，";
                }
                strNodeName = "运行状态";
            }
            else
            {
                //控制状态、当前日期、当前时间、保护器跳闸事件记录用到时补充
                return;
            }
            #endregion
        }

        private string RemoveIntFrontZero(string data)
        {
            try
            {
                return Convert.ToInt32(data).ToString();
            }
            catch
            {
                return data;
            }
        }

        private string RemoveFloatFrontZero(string data,int decimalNums)
        {
            try
            {
                string dataFormat = "#.";
                for(int i = 0; i < decimalNums; i++)
                {
                    dataFormat += "0";
                }
                data = data.Substring(0, data.Length - decimalNums) + "." + data.Substring(data.Length - decimalNums,decimalNums);
                return float.Parse(data).ToString(dataFormat);
            }
            catch
            {
                return data;
            }
        }

        public void SendReadAddr()
        {
            if(ProtectType == 0)
            {
                ReadMeterData("999999999999","C032");
            }
            else
            {
                string strCmd = "68AAAAAAAAAAAA681300";
                strCmd += PubFunction.CheckSum(strCmd);
                strCmd = "FEFEFEFE" + strCmd + "16";
                SendData(strCmd);
            }
        }

        public void ReadMeterData(string addr,string dataNode)
        {
            string strCmd = "68";
            if (ProtectType == 0)
            {
                strCmd += ReverseData(StandardAddr(addr)) + "680102";
            }
            else
            {
                strCmd += ReverseData(StandardAddr(addr)) + "681104";
            }
            strCmd += ReverseData(Add33H(dataNode));
            strCmd += PubFunction.CheckSum(strCmd);
            strCmd = "FEFEFEFE" + strCmd + "16";
            SendData(strCmd);
        }

        public void SetOpenOrClose(string addr,string data)
        {
            if(ProtectType == 0)
            {
                if(data == "合")
                {
                    WriteMeterData(addr, "C036", "5F");
                }
                else
                {
                    WriteMeterData(addr, "C036", "50");
                }
            }
            else if (ProtectType == 1)
            {
                if (data == "合")
                {
                    WriteMeterData(addr, "06010201", "0002");
                }
                else
                {
                    WriteMeterData(addr, "06010301", "0002");
                }
            }
        }


        private void WriteMeterData(string addr,string dataMask,string data)
        {
            string strCmd = "68";
            if (ProtectType == 0)
            {                
                strCmd += ReverseData(StandardAddr(addr)) + "6804" + (2 + data.Length / 2).ToString("X2");
                strCmd += ReverseData(Add33H(dataMask));
                strCmd += ReverseData(Add33H(data));
                strCmd += PubFunction.CheckSum(strCmd);
                strCmd = "FEFEFEFE" + strCmd + "16";
                SendData(strCmd);
            }
            else
            {
                strCmd += ReverseData(StandardAddr(addr)) + "681C" + (12 + data.Length / 2).ToString("X2");
                strCmd += ReverseData(Add33H(dataMask));
                strCmd += ReverseData(Add33H("00000000")) + ReverseData(Add33H("00000000")) + ReverseData(Add33H(data));
                strCmd += PubFunction.CheckSum(strCmd);
                strCmd = "FE" + strCmd + "16";
                SendData(strCmd);
            }
        }


        private string StandardAddr(string addr)
        {
            if(addr.Length % 2 == 1)
            {
                addr = "0" + addr;
            }
            while(addr.Length < 12)
            {
                addr = "00" + addr;
            }
            return addr;
        }
    }
}