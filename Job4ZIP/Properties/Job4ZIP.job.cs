/*
 * Created by SharpDevelop.
 * User: skorik
 * Date: 31.05.2018
 * Time: 17:26
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
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