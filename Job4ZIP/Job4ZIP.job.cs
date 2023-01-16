using System.Xml.Linq;
using System;
namespace Job4ZIP
{
	partial  class Program
	{
	static void doJob(XElement job)
	{
	
		Console.WriteLine(job.Attribute("id"));
	}
	}
}