using System;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace MachineCodeProject
{
	 Token 0x02000002 RID 2
	internal class Program
	{
		 Token 0x06000001 RID 1 RVA 0x00002050 File Offset 0x00000250
		[STAThread]
		private static void Main(string[] args)
		{
			string code = Program.GetCode();
			Console.WriteLine(code);
		}


		[DllImport(ProjectMD5.dll, CharSet = CharSet.Ansi)]
		protected static extern void GETREGIDC(ref StringBuilder ResultData);

		protected static string GetCode()
		{
			StringBuilder stringBuilder = new StringBuilder(15000);
			try
			{
				Program.GETREGIDC(ref stringBuilder);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			return stringBuilder.ToString();
		}

		private static string GetSystemInfo()
		{
			string input = Program.GetCpuID() + Program.GetMacAddress() + Program.GetDiskID();
			string str = Regex.Replace(input, [ ], );
			return Program.GetMd5_32byte(str);
		}

		public static string GetMd5_32byte(string str)
		{
			string text = string.Empty;
			MD5 md = MD5.Create();
			byte[] array = md.ComputeHash(Encoding.UTF8.GetBytes(str));
			for (int i = 0; i  array.Length; i++)
			{
				text += array[i].ToString(X);
			}
			return text;
		}

		public static string GetCpuID()
		{
			string result;
			try
			{
				string text = ;
				ManagementClass managementClass = new ManagementClass(Win32_Processor);
				ManagementObjectCollection instances = managementClass.GetInstances();
				foreach (ManagementBaseObject managementBaseObject in instances)
				{
					ManagementObject managementObject = (ManagementObject)managementBaseObject;
					text = managementObject.Properties[ProcessorId].Value.ToString();
				}
				result = text;
			}
			catch
			{
				result = unknow;
			}
			finally
			{
			}
			return result;
		}

		public static string GetMacAddress()
		{
			string result;
			try
			{
				string text = ;
				ManagementClass managementClass = new ManagementClass(Win32_NetworkAdapterConfiguration);
				ManagementObjectCollection instances = managementClass.GetInstances();
				foreach (ManagementBaseObject managementBaseObject in instances)
				{
					ManagementObject managementObject = (ManagementObject)managementBaseObject;
					bool flag = (bool)managementObject[IPEnabled];
					if (flag)
					{
						text = managementObject[MacAddress].ToString();
						break;
					}
				}
				result = text;
			}
			catch
			{
				result = unknow;
			}
			finally
			{
			}
			return result;
		}

		public static string GetDiskID()
		{
			string result;
			try
			{
				string text = ;
				ManagementClass managementClass = new ManagementClass(Win32_DiskDrive);
				ManagementObjectCollection instances = managementClass.GetInstances();
				foreach (ManagementBaseObject managementBaseObject in instances)
				{
					ManagementObject managementObject = (ManagementObject)managementBaseObject;
					text = (string)managementObject.Properties[Model].Value;
				}
				result = text.Split(new char[]
				{
					' '
				})[0];
			}
			catch
			{
				result = unknow;
			}
			finally
			{
			}
			return result;
		}
	}
}
