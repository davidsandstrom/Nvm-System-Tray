using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nvm.Forms
{
	internal class NvmWrapper
	{
		public NvmWrapper()
		{
			
		}

		private IEnumerable<string> Command(string str)
		{
			List<string> lines = new List<string>();

			var process = new Process();
			process.StartInfo.FileName = "cmd.exe";
			process.StartInfo.Verb = "runas";
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardInput = true;
			process.Start();

			process.StandardInput.WriteLine(str);
			process.StandardInput.Flush();
			process.StandardInput.Close();

			string line;

			while ((line = process.StandardOutput.ReadLine()) != null)
			{
				if (!string.IsNullOrWhiteSpace(line))
				{
					lines.Add(line.Trim());
				}
			}

			process.WaitForExit();

			if(lines.Count > 3)
			{
				lines.RemoveRange(0, 3);
				lines.RemoveAt(lines.Count - 1);
			}

			return lines;
		}

		private void CommandElevated(string str) {
			List<string> lines = new List<string>();

			var process = new Process();
			process.StartInfo.FileName = "cmd.exe";
			process.StartInfo.Verb = "runas";
			process.StartInfo.UseShellExecute = true;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.Arguments = "/c " + str;
			process.Start();

			process.WaitForExit();
		}

		public KeyValuePair<string, string>[] Versions()
		{
			var items = Command("nvm list");
			var list = new List<KeyValuePair<string, string>>();

			foreach(var item in items)
			{
				var version = new KeyValuePair<string, string>(GetVersionNumber(item), item);
				list.Add(version);
			}

			return list.ToArray();
		}

		public string Current()
		{
			var version = Command("nvm current");
			return GetVersionNumber(version.First());
		}

		public void Use(string version)
		{
			CommandElevated("nvm use " + version);
		}

		private string GetVersionNumber(string input)
		{
			string pattern = @"([0-9]{1,2}\.[0-9]{1,2}\.[0-9]{1,2})";
			var match = Regex.Match(input, pattern);

			if (match.Success)
			{
				return match.Value;
			}

			return input;
		}

		private static void ProcessOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
		{
			if (!String.IsNullOrEmpty(outLine.Data))
			{
				//cmdOutput.Append(Environment.NewLine + outLine.Data);
			}
		}
	}
}
