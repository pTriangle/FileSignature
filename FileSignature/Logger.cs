using System;
using System.Text.RegularExpressions;

namespace FileSignature
{
	internal static class Logger
	{
		public static void Log(Exception e)
		{
			Console.WriteLine($"Message: {e.Message}");
			Console.WriteLine("Stack trace:");
			foreach (Match m in Regex.Matches(e.StackTrace, @".*\)"))
				Console.WriteLine(m.Value);
		}
	}
}
