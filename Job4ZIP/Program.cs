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
using System.Text;

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
        #region Global Vars
        static int DowncountInterval = 10;//sec
		static XDocument XmlDoc;		
		static String FolderLog;
		static string SourcePath,TargetPath, TargetFile, ZIP_EXE; 
		#endregion
		public static void Main(string[] args)
		{
			#if DEBUG
			FolderLog = AppDomain.CurrentDomain.BaseDirectory;
			#else
            FolderLog = GetEnvironmentVariable("USERPROFILE")+"\\Documents";			
			#endif
			FolderLog += "job4zip.log";
			WriteLineLog("\n------------------------------------------");
			WriteLineLog(String.Format("Start time\t{0}", DateTime.Now));
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
			StartTime = DateTime.Now;
            #region Print Programm Title
            Console_ResetColor();
			Console.Clear();            //Console.BackgroundColor = ConsoleColor.Blue;						//Console.ForegroundColor = ConsoleColor.DarkBlue;	
			Console.ForegroundColor = FGcolorH1;
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.Title = ((FileVersionInfo.GetVersionInfo((Assembly.GetExecutingAssembly()).Location)).FileDescription + " " + (FileVersionInfo.GetVersionInfo((Assembly.GetExecutingAssembly()).Location)).FileVersion + " " + (FileVersionInfo.GetVersionInfo((Assembly.GetExecutingAssembly()).Location)).LegalCopyright);
			Console.WriteLine("-------------------------------------------------------");
			Console.WriteLine(Console.Title);
			Console.WriteLine("-------------------------------------------------------");
			Console.ForegroundColor = ConsoleColor.White;
			#endregion
			Console.WriteLine("Start time\t{0}", StartTime);
			#region Parsing arguments command line            
			Console.ForegroundColor = ConsoleColor.DarkGray;
			xmlFile = AppDomain.CurrentDomain.BaseDirectory;
#if DEBUG
			xmlFile = System.IO.Directory.GetParent(xmlFile).ToString();
			xmlFile = System.IO.Directory.GetParent(xmlFile).ToString();
			xmlFile = System.IO.Directory.GetParent(xmlFile).ToString();
			xmlFile = System.IO.Directory.GetParent(xmlFile).ToString() + @"\test\1C.xml";
#else
            
			if (args.Length==0) 
				xmlFile+="job4zip.xml";
			else				 
				xmlFile+=args[0];
#endif
			ConsoleWriteLineField("Config file is ", xmlFile);
            #endregion
            #region Check path to config file 
            if (!File.Exists(xmlFile))
			{
				ShowError_Exit(String.Format("ERR: Config File \"{0}\" not exist", xmlFile), 1);
			}
			Console.SetCursorPosition(0, Console.CursorTop - 1);
			int xmlFileStringNameLenght = xmlFile.Length;
			xmlFile = Path.GetFullPath(xmlFile);
			ConsoleWriteField("Config file is ", Path.GetFullPath(xmlFile), false);
            #endregion
            #region Print config file 
            if (xmlFile.Length < xmlFileStringNameLenght)
			{
				string str1 = "";
				for (int i = xmlFile.Length; i != xmlFileStringNameLenght; i++) str1 = str1 + " ";
				Console.Write(str1);
			}
			Console.WriteLine();
			ConsoleWriteLineField("Write log to ", FolderLog);
			#endregion
			#region Parsing config file
			Plan plan = new Plan(xmlFile);
			XmlDoc = new XDocument();
			try
			{
				XmlDoc = XDocument.Load(xmlFile);
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console_WriteLine(String.Format("ERR: Config File \"{0}\" not parsing.", xmlFile));

				Console.ForegroundColor = ConsoleColor.DarkYellow;
				Console_WriteLine(ex.Message);
				ShowError_Exit("", 2);
			}
			#endregion
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.WriteLine(XmlDoc);
			Console_ResetColor();
			#region Check presence of a minimum set of parameters
            if (XmlDoc.Element("PLAN") == null)
				ShowError_Exit("Root Tag PLAN not found in config file", 30);
			//if (XmlDoc.Element("PLAN").Element("Timetable") == null)
			//	ShowError_Exit("Tag Timetable not found in config file", 31);
			if (XmlDoc.Element("PLAN").Element("ZIP") == null)
				ShowError_Exit("Tag ARH not found in config file", 32);
			if (XmlDoc.Element("PLAN").Element("SourcePath") == null)
				ShowError_Exit("Tag SourcePath not found in config file", 33);
			if (XmlDoc.Element("PLAN").Element("TargetPath") == null)
				ShowError_Exit("Tag TargetPath not found in config file", 33);
			if (XmlDoc.Element("PLAN").Element("TargetFile") == null)
				ShowError_Exit("Tag TargetFile not found in config file", 33);
			TargetFile = XmlDoc.Element("PLAN").Element("TargetFile").Value;
				if (XmlDoc.Element("PLAN").Element("ZIP").Element("EXE")== null)
				ShowError_Exit("Tag EXE in ZIP node not found in config file", 34);
			//XElement Timetable = XmlDoc.Element("PLAN").Element("Timetable");
			//foreach (XElement Schedule in Timetable.Elements("Schedule"))
			//{
			//	doSchedule(Schedule);
			//}
			#endregion
			#region Print Plan name
			if (XmlDoc.Element("PLAN").HasAttributes)
			{//				foreach (XAttribute att in XmlDoc.Element("PLAN").Attributes()) 					Console.WriteLine("{0}={1}", att.Name, att.Value);
				if (XmlDoc.Element("PLAN").Attribute("name") != null)
					ConsoleWriteLineField("Plan", "\"" + XmlDoc.Element("PLAN").Attribute("name").Value + "\"",true);
			}
            #endregion
            #region Check Source path
            SourcePath = XmlDoc.Element("PLAN").Element("SourcePath").Value;
            #if DEBUG
			SourcePath = Path.GetDirectoryName(xmlFile) + "\\"+SourcePath; 
			#endif
			ConsoleWriteLineField("SourcePath", SourcePath, true);
			if (!Directory.Exists(SourcePath)) ShowError_Exit("SourcePath \""+SourcePath+"\" not exist", 34);
			//TODO: If source path is empty then rise warning
			#endregion
			#region Check Target path
			TargetPath = XmlDoc.Element("PLAN").Element("TargetPath").Value;
			#if DEBUG
			TargetPath = Path.GetDirectoryName(xmlFile) + "\\" + TargetPath;
			#endif
			ConsoleWriteLineField("TargetPath", TargetPath, true);
			if (!Directory.Exists(TargetPath)) ShowError_Exit("TargetPath \"" + TargetPath + "\" not exist", 34);
			#endregion
			#region Check ZIP
			ZIP_EXE = XmlDoc.Element("PLAN").Element("ZIP").Element("EXE").Value;
			ConsoleWriteLineField("Zip", ZIP_EXE, true);
			if (!File.Exists(ZIP_EXE)) ShowError_Exit("ZIP \"" + ZIP_EXE + "\" not exist", 34);
			#endregion
			#region Start ZIP
			Console.ResetColor(); Console.ForegroundColor = ConsoleColor.White;
			ConsoleWriteLineField("Start time", StartTime.ToString(), true);			
			ZIP();
			FinishTime = DateTime.Now;
			//FinishTime=FinishTime.AddMinutes (1.0);
			FinishTime = FinishTime.AddSeconds(11.0);
			ConsoleWriteLineField("Finish time", FinishTime.ToString(), true);
			SpendTime = FinishTime - StartTime;
			Console_WriteLine(String.Format("{0}", SpendTime.TotalDays));
			Console_WriteLine(String.Format("Spend time\t{0} sec", SpendTime.ToString()));//в хуман форме
			#endregion
			FinishDownCount();
		}
		//*********************************
		static void FinishDownCount() {
			//*********************************
			Console.Write("\nPress any key to continue . . . ");
			while (DowncountInterval != 0)
			{
				Console.Write("\b{0}", DowncountInterval);
				if (Console.KeyAvailable)
				{
					break;
				}
				Thread.Sleep(1000);
				DowncountInterval--;
			}
		}
		//*********************************
		static void ShowError_Exit(String Message, int ExitCode = 1) {
			//*********************************	
			Console.ForegroundColor = ConsoleColor.DarkRed;
			Console_WriteLine(Message);
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
		public static void Console_WriteLine(string str, bool WriteToLogToo=true)

		{
			Console.WriteLine(str);
			if (WriteToLogToo) WriteLineLog(str);
		}
		public static void Console_Write(string str, bool WriteToLogToo=true)
		{
			Console.Write(str);
			if (WriteToLogToo) WriteLog(str);
		}

		public static void ConsoleWriteField(string Name, string Value, bool WriteToLogToo=true)
		{
			Console.ForegroundColor = FGcolorFieldName;
			Console.Write(Name + "\t");
			Console.ForegroundColor = FGcolorFieldValue;
			Console.Write(Value);
			if (WriteToLogToo) 
				{
				WriteLog(Name + "\t" + Value);
				}
		}
		public static void ConsoleWriteLineField(string Name, string Value, bool WriteToLogToo = true)
		{
			Console.ForegroundColor = FGcolorFieldName;
			Console.Write(Name + "\t");
			Console.ForegroundColor = FGcolorFieldValue;
			Console.WriteLine(Value);
			if (WriteToLogToo)
			{
				WriteLineLog(Name + "\t" + Value);
			}
		}
		public static string ParentFolder(string Folder){return System.IO.Directory.GetParent(Folder.TrimEnd(new char[] { '\\' })).ToString() + "\\";}
	
		public static  void WriteLog(String str)
		{
			using (StreamWriter writer = new StreamWriter(FolderLog, true))
			{				
				 writer.Write(str);
				//await writer.WriteAsync("4,5");
			}
		}
		public static void WriteLineLog(String str)
		{
			using (StreamWriter writer = new StreamWriter(FolderLog, true))
			{
				writer.WriteLine(str);
				//await writer.WriteAsync("4,5");
			}
		}
		private static  int RUN(string FileName, string Arguments)// TODO:Записывать все ошибки в журнал
		{
#if DEBUG
			//this.textBox_Console.BeginInvoke(delegateConsoleWrite, FileName + " " + Arguments + Environment.NewLine);
#endif
			int RUN_return = 0;
			try
			{
				using (Process myProcess = new Process())
				{
					myProcess.StartInfo.UseShellExecute = false;
					myProcess.StartInfo.FileName = FileName;
					myProcess.StartInfo.Arguments = Arguments;
					//myProcess.StartInfo.UseShellExecute = false;
					//myProcess.StartInfo.CreateNoWindow = true;
					//myProcess.StartInfo.ErrorDialog = true;
					myProcess.StartInfo.RedirectStandardOutput = true;
					myProcess.StartInfo.RedirectStandardInput = true;
					myProcess.StartInfo.RedirectStandardError = true;
					StringBuilder so = new StringBuilder();
					myProcess.OutputDataReceived += (sender, args) => { so.AppendLine(args.Data); };
					myProcess.ErrorDataReceived += (sender, args) => { so.AppendLine(args.Data); };
					myProcess.Start();
					myProcess.BeginOutputReadLine();
					myProcess.BeginErrorReadLine();
					Random random = new Random();
					int BufLine = 0;
					while (!myProcess.HasExited)
					{
						Thread.Sleep(random.Next(100, 1000));
						if (so.Length > BufLine)
						{  
							Console.Write(so.ToString().Substring(BufLine));//Console.Write(OEM866_to_Win1251(so.ToString().Substring(BufLine)));
							WriteLineLog(so.ToString().Substring(BufLine));
							BufLine = so.Length;
						}
					}
					//if (so.Length > BufLine) this.textBox_Console.BeginInvoke(delegateConsoleWrite, so.ToString().Substring(BufLine));
					RUN_return = myProcess.ExitCode;
					myProcess.Dispose();
				}
			}
			catch (Exception e)
			{
				{
					RUN_return = -1;
					Console.WriteLine(e.Message + Environment.NewLine);
					WriteLineLog(e.Message);
				}
			}
			return (RUN_return);
		}
		private static int ZIP()
		{
			int  result = 0;

#if DEBUG
			//this.textBox_Console.BeginInvoke(delegateConsoleWrite, "Begin unzip file " + Source + " to " + workFolder + Environment.NewLine);
#endif
			//this.textBox_Console.BeginInvoke(delegateConsoleWrite, "Распаковка " + Source + ".");
			string Arguments = "U -r -ep1 -m5 -we:\\tmp -ilog"+FolderLog +" "+ TargetPath+"\\"+TargetFile+" "+SourcePath+"*.png";
			Console_WriteLine(String.Format("Exec {0} {1}", ZIP_EXE,Arguments), true);			
			result = RUN(ZIP_EXE, Arguments);
			string[] ReturnCodeText= {
				"Операция успешно завершена",//0
				"Предупреждение. Некритические ошибки",//1
				"Критическая ошибка",//2
				"Неверная контрольная сумма. Данные повреждены",//3
				"Попытка изменить заблокированный архив",//4
				"Ошибка записи на диск",//5
				"Ошибка открытия файла",//6
				"Неверный параметр в командной строке",//7
				"Недостаточно памяти для выполнения операции",//8
				"Ошибка при создании файла",//9
				"Нет параметров и файлов, удовлетворяющих указанной маске",//10
				"Неверный пароль",//11
				"Ошибка чтения",//12
				"Операция прервана пользователем",//255->13
				"Неизвестный код возврата"//14
			};
			Console_Write(string.Format("ERRORLEVEL={0} ", result),true);			
			if (result > 12) { if (result == 255) result = 13; else result = 14;}			
			Console_WriteLine(ReturnCodeText[result], true);
			return result;
		}
		public static string OEM866_to_Win1251(string line)
		{
			Encoding w1251 = Encoding.GetEncoding("Windows-1251");
			Encoding cp866 = Encoding.GetEncoding("cp866");
			return cp866.GetString(w1251.GetBytes(line));
		}

	}
}