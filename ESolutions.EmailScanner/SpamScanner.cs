using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace ESolutions.EmailScanner
{
	public static class SpamScanner
	{
		#region PerformSpamScan
		/// <summary>
		/// Scanns the mail for spam.
		/// </summary>
		/// <returns>True if the file contains a spam mail. False otherwise.</returns>
		public static SpamScanResult Scan(FileInfo file)
		{
			SpamScanResult result = SpamScanResult.Ham;

			try
			{
				//Build message for spamd service. First line contains the command. The rest is
				//made up by the mail text
				Byte[] mailText = File.ReadAllBytes(file.FullName);
				Byte[] spamdCommand = Encoding.Default.GetBytes(Properties.Settings.Default.SpamdCommand + "\n");

				MemoryStream buffer = new MemoryStream();
				buffer.Write(spamdCommand, 0, spamdCommand.Length);
				buffer.Write(mailText, 0, mailText.Length);

				//Connect to spamd service and send 
				System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(
					 System.Net.Sockets.AddressFamily.InterNetwork,
					 System.Net.Sockets.SocketType.Stream,
					 System.Net.Sockets.ProtocolType.Tcp);

				System.Net.IPEndPoint spamdIpEndPoint = new System.Net.IPEndPoint(
					 System.Net.IPAddress.Parse(Properties.Settings.Default.SpamdIpAddress),
					 Properties.Settings.Default.SpamdPort);

				socket.Connect(spamdIpEndPoint);
				socket.Send(buffer.ToArray());
				socket.Shutdown(System.Net.Sockets.SocketShutdown.Send);

				//Get response of spamd service
				byte[] readBuffer = new byte[1024];
				Int32 bytesRead = 0;
				String message = String.Empty;

				do
				{
					bytesRead = socket.Receive(readBuffer);
					message += System.Text.Encoding.Default.GetString(readBuffer, 0, bytesRead);
				}
				while (bytesRead > 0);

				//Evaluate response
				Tracing.Message("Response of spamd: " + message.Replace ('\n',' ').Replace('\r', ' '));
				if (message.Contains(Properties.Settings.Default.SpamdSpamIndicator))
				{
					result = SpamScanResult.Spam;
					Tracing.Message("Mail is spam: " + file.FullName);
				}
				else
				{
					result = SpamScanResult.Ham;
					Tracing.Message("Mail is ham: " + file.FullName);
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Scanning for spam failed. See inner exception for further detail.", ex);
			}

			return result;
		}
		#endregion
	}
}
