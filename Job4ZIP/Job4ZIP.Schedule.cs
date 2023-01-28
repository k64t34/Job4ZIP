using System.Xml.Linq;
using System;
namespace Job4ZIP
{
	partial class Program
	{
		static void doSchedule(XElement Schedule)
		{
			Console_WriteLine(String.Format("Shedule id={0}", Schedule.Attribute("id").ToString()));
		}
	}
	public class Backup
	{
		int id;
	}
	public class Plan
	{
		string Name;
		DateTime DateLastBackup;
		Backup Backups;
		public Plan(string xmlFile) { }
	}
	public class Period
		{
		int id;
        }
	public class Schedule
    {
		string id;
		int count = 0;
		int type;
		Period period;
		PeriodType periodType=0;

		Schedule(XElement xmlSchedule) 
		{ 
		
		}

		


	}
	enum PeriodType : byte { Day=0, Week=1, Mounth=2,Year=3};
}
	
