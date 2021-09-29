/***********************************************随笔说明***********************************************
 * Author:              ZhangDongRui (En: Keri)
 * FileName:         pubFunction.cs 
 * Description :    公共函数部分
 * Range of application:  C#平台下的公共接口
 * 
 * Comments:       本脚本(pubFunction.cs)参考delphi中"pubFunction"单元模块,  或者网络资源编写
 ******************************************************************************************************/

using System;
using System.IO;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//-----------------------------------上面几个命名空间的引用为添加项目时，自动生成； 根据需要可做删减






namespace Assets.Scripts.Public                              //zdr-1603: 自动生成的命名空间，移植程序时，可以根据需要进行修改
{
    //zdr-1603: 为便于外部进行对公共函数的"直接"调用，对外函数均声明为： public static;  而尽在该类内被使用的函数，则声明非静态的私有类型


    /*对外提供接口如下：
     * 1,    public static string Get_Data_CRC(string strData)
     * 
     * 
     * 
     * 
     */


    /*仅类内部使用的函数如下：-------->  根据需要可以修改private, public, static等关键字，从能够被外部调用
     * 1,  private static int[] crc16(int[] data) 
     * 
     * 
     * 
     * 
     */



    class PubFunction
    {
        //zdr-1603: 参考网络资源，修改的CRC16校验
        private static  int[] CRC16(int[] data)     // zdr-1603:  加上静态的，可以直接被内部使用，而不用在实例化对象上调用
        {
            // int[] temdata = new int[data.Length + 2];
            int[] temdata = new int[2];    //zdr-1603: 存放CRC计算结果
            int xda, xdapoly;
            int i, j, xdabit;
            xda = 0xFFFF;
            xdapoly = 0xA001;
            for (i = 0; i < data.Length; i++)
            {
                xda ^= data[i];
                for (j = 0; j < 8; j++)
                {
                    xdabit = (int)(xda & 0x01);
                    xda >>= 1;
                    if (xdabit == 1)
                        xda ^= xdapoly;
                }
            }
            //Array.Copy(data, 0, temdata, 0, data.Length);
            //temdata[temdata.Length - 2] = (int)(xda & 0xFF);
            //temdata[temdata.Length - 1] = (int)(xda >> 8);

            temdata[0] = (int)(xda & 0xFF);
            temdata[1] = (int)(xda >> 8);

            return temdata;
        }


        //zdr-1603: 参数为16进制的字符串，返回值为CRC16校验字符串， 2个字节(即：4个字符返回)

		public static string CheckSum7F(string strContent)
		{
			int sum = 0;
			for (int i = 0; i < strContent.Length / 2; i++)
			{
				sum += Convert.ToInt32(strContent.Substring(i * 2, 2), 16);
			}
			sum = sum & 0x7F;
			return sum.ToString("X2");
		}
        public static string GetDataCRC(string strData)
        {
            int[] iCMD = new int[strData.Length / 2];

            int[] iCRCResult = new int[2];

            for (int i = 0; i < iCMD.Length; i++)        
                iCMD[i] = Int32.Parse(strData.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);   //zdr-1603: 16进制字串，转10进制数字

            //zdr-1603: 下面例句为： 16进制字串，转10进制， 如有需要可参考使用
            //int.Parse(s, System.Globalization.NumberStyles.AllowHexSpecifier);//16转10


            iCRCResult = CRC16(iCMD);

            //zdr-1603: 将iCRCResult，转换成16进制字符串
            string strCrcResult = iCRCResult[0].ToString("X2") + iCRCResult[1].ToString("X2");
            return strCrcResult;

        }
		public static string Sum(string strData)
		{
			//Convert.ToInt32(buff.Substring(12, 2), 16)
			//int value ,i;
			//for(i=0;i<)


			return   "dd";
		}
        ////图像转为字节流
        //public static byte[] ConvertByte(Image img)
        //{
        //    MemoryStream ms = new MemoryStream();
        //    img.Save(ms, img.RawFormat);
        //    byte[] bytes = new byte[ms.Length];
        //    ms.Read(bytes, 0, Convert.ToInt32(ms.Length));
        //    return bytes;
        //}

        ////字节流转为图像
        //public static Image ConvertImg(byte[] datas)
        //{
        //    MemoryStream ms = new MemoryStream(datas);
        //    Image img = Image.FromStream(ms, true);//在这里出错   
        //    return img;
        //} 

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

    }




}
