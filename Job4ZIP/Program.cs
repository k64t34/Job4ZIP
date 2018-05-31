/*
 * Created by SharpDevelop.
 * User: skorik
 * Date: 31.05.2018
 * Time: 11:29
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Threading; 
namespace Job4ZIP
{
	class Program
	{
		public static void Main(string[] args)
		{
			DateTime StartTime;
			DateTime FinishTime;	
			int DowncountInterval=5;//sec			
			StartTime= DateTime.Now;
			Console.BackgroundColor = ConsoleColor.White;
			Console.ForegroundColor = ConsoleColor.Black;
			Console.Clear();
			Console.Title="Job for 7-Zip";
			Console.WriteLine(Console.Title);
			Console.WriteLine("------------------------------------------------------- by Skorik 2018");
			Console.WriteLine("Start time {0}",StartTime);
			FinishTime= DateTime.Now;
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
			//Console.ReadKey(true);
		}
	}
}