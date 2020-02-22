using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace ESolutions.EmailScanner
{
	public static class VirusScanner
	{
		#region Scan
		/// <summary>
		/// Scans the specified file for virus infections
		/// </summary>
		/// <param name="file">The file.</param>
		/// <returns>State indicating whether the file is infected or not.</returns>
		public static VirusScanResult Scan(FileInfo file)
		{
			VirusScanResult result = VirusScanResult.Clean;

			try
			{
				Process process = new Process();
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.WorkingDirectory = Properties.Settings.Default.FprotFolder;
				process.StartInfo.FileName = Properties.Settings.Default.FprotFolder + Properties.Settings.Default.FprotExe;
				process.StartInfo.Arguments = String.Format(
					"{0} \"{1}\"",
					Properties.Settings.Default.FprotArguments,
					file.ToString());
				process.Start();
				process.WaitForExit();
				String message = process.StandardOutput.ReadToEnd();

				if (!message.Contains(Properties.Settings.Default.FprotInfectionIndicator))
				{
					result = VirusScanResult.Infected;
					Tracing.Message("Mail is infected: " + file.FullName);
				}
				else
				{
					result = VirusScanResult.Clean;
					Tracing.Message("Mail is clean: " + file.FullName);
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Virus scan failed. See inner exception for further detail", ex);
			}

			return result;
		}
		#endregion
	}
}
