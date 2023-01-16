using System;
using System.Threading;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Job4ZIP
{
	partial class Program
	{
		static int DowncountInterval=10;//sec
		static XDocument XmlDoc;
		static String ArcPath=Environment.GetEnvironmentVariable("ProgramFiles");
		static String ArcEXE="7z.exe";
		public static void Main(string[] args)
		{
			DateTime StartTime;
			DateTime FinishTime;	
			TimeSpan  SpendTime;	
			
			String xmlFile;			
			StartTime= DateTime.Now;
			//Console.BackgroundColor = ConsoleColor.Black;
			//Console.ForegroundColor = ConsoleColor.White;
			Console.ResetColor();
			Console.Clear();
			Console.Title=" Job for 7-Zip";
			Console.WriteLine("-------------------------------------------------------");
			Console.WriteLine("{0}                           {1}",Console.Title,"by Skorik 2023");
			Console.WriteLine("-------------------------------------------------------");
			Console.WriteLine("Start time\t{0}",StartTime);
			//
			//Parsing arguments command line
			//
			xmlFile=AppDomain.CurrentDomain.BaseDirectory;
			if (args.Length==0) 
				xmlFile+="job4zip.xml";
			else				 
				xmlFile+=args[0];
			
			Console.WriteLine("Config file is {0}",xmlFile);
			
			if (!File.Exists(xmlFile))
			{
				ShowError_Exit(String.Format("ERR: Config File \"{0}\" not exist",xmlFile),1);	
			}
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
        	Console.ResetColor();
	       	FinishDownCount();
			Environment.ExitCode = ExitCode;
			Environment.Exit(ExitCode); 
		}
		
 
	}
}