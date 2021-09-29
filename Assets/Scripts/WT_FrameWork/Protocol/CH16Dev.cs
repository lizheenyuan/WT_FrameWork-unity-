using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

namespace Assets.Scripts.WT_FrameWork.Protocol
{
    public struct TCH16FaultInfo
    {
        public string name;//故障点名称 此值要保证唯一
        //string pha;//故障现象
        //string cause;//故障原因
        public int devID;//控制箱ID
        public string faultID;//继电器控制ID；
        public int setFlag;//设置标示  相同的标示不能 同时设置故障
        public int leakID; // 故障点对应的 漏电电流

        public TCH16FaultInfo(string name,int devID,string faultID,int setFlag,int leakID)
        {
            this.name = name;
            this.devID = devID;
            this.faultID = faultID;
            this.setFlag = setFlag;
            this.leakID = leakID;
        }
    }

    public class CH16Dev: WTClientSocket
    {
        public TCH16FaultInfo[] ch16FaultInfo;
        public int ID { get; set; }
        
        public CH16Dev()
        {
            ID = 0;
            ch16FaultInfo = new TCH16FaultInfo[25];
            ch16FaultInfo[0] = new TCH16FaultInfo("K1", 1, "0F", 1, 1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K2", 1, "0D", 2, 1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K3", 1, "0B", 3, 1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K4", 1, "0A", 0, -1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K5", 1, "1E", 1, 1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K6", 1, "18", 2, 1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K7", 1, "28", 3, 1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K8", 1, "23", 0, -1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K9", 1, "1D", 1, 1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K10", 1, "17", 2, 1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K11", 1, "27", 3, 1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K12", 1, "22", 0, -1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K13", 1, "1C", 1, 1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K14", 1, "16", 2, 1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K15", 1, "26", 3, 1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K16", 1, "13", 0, -1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K17", 1, "1B", 1, 1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K18", 1, "15", 2, 1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K19", 1, "25", 3, 1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K20", 1, "12", 0, -1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K21", 1, "1A", 1, 1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K22", 1, "14", 2, 1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K23", 1, "24", 3, 1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K24", 1, "11", 0, -1);
            ch16FaultInfo[0] = new TCH16FaultInfo("K25", 1, "09", 0, -1);
        }

        public void Start()
        {
            string strC  = "A1";
            string strL  = "02";
            string strID = ID.ToString("X2");
            string strH = CheckSum7F(strC + strL + strID);
            SendData(strC + strL + strID + strH);
        }

        public void Stop()
        {
            string strC = "A2";
            string strL = "02";
            string strID = ID.ToString("X2");
            string strH = CheckSum7F(strC + strL + strID);
            SendData(strC + strL + strID + strH);
        }

        public void Rest()
        {
            string strC = "A4";
            string strL = "02";
            string strID = ID.ToString("X2");
            string strH = CheckSum7F(strC + strL + strID);
            SendData(strC + strL + strID + strH);
        }

        public void SetFault(List<string> ch16Fault)
        {
            string strC = "A6";
            string strL = "04";
            string strID = ID.ToString("X2");
            string strM1 = "";
            string strM2 = "01";
            string strH = "";
            for (int i = 0;i < ch16FaultInfo.Length; i++)
            {
                for(int j = 0;j < ch16Fault.Count; j++)
                {
                    if(ch16Fault[j] == ch16FaultInfo[i].name)
                    {
                        strM1 = ch16FaultInfo[i].faultID;
                        strH = CheckSum7F(strC + strL + strID + strM1 + strM2);
                        SendData(strC + strL + strID + strM1 + strM2 + strH);
                        Thread.Sleep(100);
                        if(ch16FaultInfo[i].leakID >= 0)
                        {
                            SetLeakDZ(ch16FaultInfo[i].name, ch16FaultInfo[i].setFlag, ch16FaultInfo[i].leakID);
                            Thread.Sleep(100);
                        }
                    }
                }
            }
        }

        private void SetLeakDZ(string faultName,int setFlag,int dzValue)
        {
            string strC = "A8";
            string strL = "05";
            string strID = ID.ToString("X2");
            string strM1 = setFlag.ToString("X2");
            string strM2M3 = CreateDZValue(dzValue);
            string strH = CheckSum7F(strC + strL + strID + strM1 + strM2M3);
            SendData(strC + strL + strID + strM1 + strM2M3 + strH);
        }

        private string CreateDZValue(int dzValue)
        {
            List<int> valueList = new List<int>();
            int number = 0;
            int[] ohmList = {1,2,4,8,10,20,40,80,100,200,400};
            int[] k1k11List = {8,4,2,1,80,40,20,10,400,200,100};
            for(int i = 10;i >= 0; i--)
            {
                if(ohmList[i] < dzValue)
                {
                    for(int j = 0; j < 11; j++)
                    {
                        if(k1k11List[j] == ohmList[i])
                        {
                            number = j;
                            valueList.Add(j);
                            break;
                        }
                    }
                    dzValue -= k1k11List[number];
                }
                else if(ohmList[i] == dzValue)
                {
                    for (int j = 0; j < 11; j++)
                    {
                        if (k1k11List[j] == ohmList[i])
                        {
                            valueList.Add(j);
                            break;
                        }                        
                    }
                    break;
                }
            }
            bool find = false;
            string strReturn = "";
            for(int i = 15; i >= 0; i--)
            {
                find = false;
                for(int j =0;j < valueList.Count; j++)
                {
                    if(valueList[j] == i)
                    {
                        find = true;
                        break;
                    }
                }
                if (find)
                {
                    strReturn += "1";
                }
                else
                {
                    strReturn += "0";
                }
            }
            return Convert.ToInt32(strReturn,2).ToString("X4");
        }

        private string CheckSum7F(string strContent)
        {
            int sum = 0;
            for(int i = 0;i < strContent.Length / 2; i++)
            {
                sum += Convert.ToInt32(strContent.Substring(i * 2,2),16);
            }
            sum = sum & 0x7F;
            return sum.ToString("X2");
        }

    }
}
