using System;
using System.Threading;
using System.IO;
using System.Xml;
using System.Xml.Linq;

using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Timers;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Collections;
using System.Windows;
using System.Windows.Forms;

namespace Job4ZIP
{
	partial class Program
	{
		#region Windows size & position //https://www.cyberforum.ru/csharp-beginners/thread300550.html
		public struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}
		static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);//https://studassistent.ru/charp/centralnoe-polozhenie-okna-konsoli-c
		static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
		static readonly IntPtr HWND_TOP = new IntPtr(0);
		const UInt32 SWP_NOSIZE = 0x0001;
		const UInt32 SWP_NOMOVE = 0x0002;
		const UInt32 SWP_NOZORDER = 0x0004;
		const UInt32 SWP_NOREDRAW = 0x0008;
		const UInt32 SWP_NOACTIVATE = 0x0010;
		const UInt32 SWP_FRAMECHANGED = 0x0020;
		const UInt32 SWP_SHOWWINDOW = 0x0040;
		const UInt32 SWP_HIDEWINDOW = 0x0080;
		const UInt32 SWP_NOCOPYBITS = 0x0100;
		const UInt32 SWP_NOOWNERZORDER = 0x0200;
		const UInt32 SWP_NOSENDCHANGING = 0x0400;

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
		[DllImport("user32.dll")]// https://studassistent.ru/charp/centralnoe-polozhenie-okna-konsoli-c
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
		#endregion
		static int DowncountInterval=10;//sec
		static XDocument XmlDoc;
		static String ArcPath=Environment.GetEnvironmentVariable("ProgramFiles");
		static String ArcEXE="7z.exe";
		public static void Main(string[] args)
		{
			#region Set console windows size
			IntPtr ConsoleHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
			const UInt32 WINDOW_FLAGS = SWP_SHOWWINDOW;
			var wndRect = new RECT();
			GetWindowRect(ConsoleHandle, out wndRect); // Получили  размеры текущего она консоли
			var cWidth = wndRect.Right - wndRect.Left;
			var cHeight = wndRect.Bottom - wndRect.Top;
			//Rectangle waRectangle;			

			//int ScreenHeight = SystemInformation.PrimaryMonitorSize.Height;
			int ScreenHeight = SystemInformation.WorkingArea.Height;

			SetWindowPos(ConsoleHandle, HWND_NOTOPMOST, 0, 0, cWidth, cHeight, WINDOW_FLAGS);
			SetWindowPos(ConsoleHandle, HWND_NOTOPMOST, 0, 0, cWidth, ScreenHeight, WINDOW_FLAGS);
			#endregion
			DateTime StartTime;
			DateTime FinishTime;	
			TimeSpan SpendTime;	
			
			String xmlFile;			
			StartTime= DateTime.Now;
			Console_ResetColor();
			Console.Clear();            //Console.BackgroundColor = ConsoleColor.Blue;						//Console.ForegroundColor = ConsoleColor.DarkBlue;	
			Console.ForegroundColor = FGcolorH1;
			Console.ForegroundColor = ConsoleColor.DarkYellow;									
			Console.Title= ((FileVersionInfo.GetVersionInfo((Assembly.GetExecutingAssembly()).Location)).FileDescription + " " +(FileVersionInfo.GetVersionInfo((Assembly.GetExecutingAssembly()).Location)).FileVersion + " " +				(FileVersionInfo.GetVersionInfo((Assembly.GetExecutingAssembly()).Location)).LegalCopyright								);
			Console.WriteLine("-------------------------------------------------------");
			Console.WriteLine(Console.Title);
			Console.WriteLine("-------------------------------------------------------");
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("Start time\t{0}",StartTime);            
            #region Parsing arguments command line            
            Console.ForegroundColor = ConsoleColor.DarkGray;
			xmlFile =AppDomain.CurrentDomain.BaseDirectory;
			#if DEBUG
			xmlFile = AppDomain.CurrentDomain.BaseDirectory;
			xmlFile = System.IO.Directory.GetParent(xmlFile).ToString();
			xmlFile = System.IO.Directory.GetParent(xmlFile).ToString();
			xmlFile = System.IO.Directory.GetParent(xmlFile).ToString();			
			xmlFile = System.IO.Directory.GetParent(xmlFile).ToString()+ @"\test\1C.xml";
#else
			if (args.Length==0) 
				xmlFile+="job4zip.xml";
			else				 
				xmlFile+=args[0];
#endif
			//Console.WriteLine("Config file is {0}",xmlFile);
			ConsoleWriteField("Config file is ", xmlFile);
			#endregion
			if (!File.Exists(xmlFile))
			{
				ShowError_Exit(String.Format("ERR: Config File \"{0}\" not exist",xmlFile),1);
			}
			Console.SetCursorPosition(0, Console.CursorTop - 1);
			int xmlFileStringNameLenght = xmlFile.Length;
			xmlFile = Path.GetFullPath(xmlFile);
			ConsoleWriteField("Config file is ", Path.GetFullPath(xmlFile),false);
			if (xmlFile.Length < xmlFileStringNameLenght) 
				{
				string str1 = ""; 
				for (int i = xmlFile.Length; i != xmlFileStringNameLenght; i++) str1 = str1 + " ";
				Console.Write(str1);
			}
			Console.WriteLine();

#region Parsing config file
			XmlDoc = new XDocument();
			try
			{
				XmlDoc = XDocument.Load(xmlFile);
			}
        	catch (Exception ex)
			{							
        		Console.ForegroundColor = ConsoleColor.DarkRed;
        		Console.WriteLine("ERR: Config File \"{0}\" not parsing.",xmlFile);
        		Console.ForegroundColor = ConsoleColor.DarkYellow;
        		Console.WriteLine(ex.Message);
        		ShowError_Exit("",2);	        	 
			}
#endregion

            Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.WriteLine(XmlDoc);
			Console.ResetColor();
			
			if (XmlDoc.Element("PLAN")==null)
				ShowError_Exit("Root Tag PLAN not found in config file",30);
			
			if (XmlDoc.Element("PLAN").Element("ARH")==null)
				ShowError_Exit("Tag ARH not found in config file",32);
			
			if (XmlDoc.Element("PLAN").Element("JOBs")==null)
				ShowError_Exit("Tag JOBs not found in config file",31);
			
			XElement JOBs = XmlDoc.Element("PLAN").Element("JOBs");
			foreach ( XElement job in JOBs.Elements("JOB"))
			{
				doJob(job);
			}
				
			//Console.ResetColor();Console.ForegroundColor = ConsoleColor.White;
			 
			Console.WriteLine("Start time\t{0}",StartTime);			
			FinishTime= DateTime.Now;
			//FinishTime=FinishTime.AddMinutes (1.0);
			FinishTime=FinishTime.AddSeconds(11.0);
			Console.WriteLine("Finish time\t{0}",FinishTime);
			
			SpendTime=FinishTime-StartTime;
			Console.WriteLine(String.Format("{0}",SpendTime.TotalDays));
			Console.WriteLine("Spend time\t{0} sec",SpendTime.ToString( ));
			//в хуман форме
			FinishDownCount();
			
		}
		//*********************************
		static void FinishDownCount() {
		//*********************************
		 Console.Write("\nPress any key to continue . . . ");
			while (DowncountInterval!=0)
			{
				Console.Write("\b{0}",DowncountInterval);
				if (Console.KeyAvailable)
				{
					break;
				}
				Thread.Sleep(1000);
				DowncountInterval--;
			} 
		}  
		//*********************************
		static void ShowError_Exit(String Message, int ExitCode=1){
		//*********************************	
			Console.ForegroundColor = ConsoleColor.DarkRed;
        	Console.WriteLine(Message);
			Console_ResetColor();
			FinishDownCount();
			Environment.ExitCode = ExitCode;
			Environment.Exit(ExitCode); 
		}
		const ConsoleColor BGcolor = ConsoleColor.Black;
		const ConsoleColor FGcolor = ConsoleColor.White;
		const ConsoleColor FGcolorH1 = ConsoleColor.Yellow;
		const ConsoleColor FGcolorFieldName = ConsoleColor.DarkGray;
		const ConsoleColor FGcolorFieldValue = ConsoleColor.White;
		public static void Console_ResetColor() { Console.BackgroundColor = BGcolor; Console.ForegroundColor = FGcolor; }
		public static void Console_SetH1Color() { Console.BackgroundColor = BGcolor; Console.ForegroundColor = FGcolorH1; }
		public static void ConsoleWriteField(string Name, string Value, bool CR = true)
		{
			Console.ForegroundColor = FGcolorFieldName;
			Console.Write(Name + "\t");
			Console.ForegroundColor = FGcolorFieldValue;
			Console.Write(Value + "\t");
			if (CR) Console.WriteLine();
		}
		public static string ParentFolder(string Folder){return System.IO.Directory.GetParent(Folder.TrimEnd(new char[] { '\\' })).ToString() + "\\";}

	}
}