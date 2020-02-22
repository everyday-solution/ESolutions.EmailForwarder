using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ESolutions.EmailScanner
{
	public class EmlFile
	{
		#region MarkAsSent
		/// <summary>
		/// Marks the eml file as send. Before this method call 
		/// </summary>
		public static void MarkAsSent(FileInfo file)
		{
			try
			{
				String newName = Path.Combine(
					file.DirectoryName,
					file.Name.Replace("P3_", "S_"));

				file.MoveTo(newName);
				file = new System.IO.FileInfo(newName);
				file.Attributes = FileAttributes.Normal;
				Tracing.Message("Mail marked as sent: " + file.FullName);
			}
			catch (Exception ex)
			{
				throw new Exception("File could not be marked as sent", ex);
			}
			finally
			{
			}
		}
		#endregion

		#region IsUnsentMail
		/// <summary>
		/// Determines whether the specified file is a unsent eml files.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <returns>
		/// 	<c>true</c> if the specified file is mail; otherwise, <c>false</c>.
		/// </returns>
		public static Boolean IsUnsentMail(FileInfo file)
		{
			Boolean result = false;

			if (
				file.Exists &&
				file.Extension == Properties.Settings.Default.MailFileExtension &&
				file.Name.StartsWith(Properties.Settings.Default.MailFilePrefix))
			{
				result = true;
			}

			return result;
		}
		#endregion

		#region IsUnblocked
		/// <summary>
		/// Determines whether the specified file is blocked for scanning or not.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <returns>
		/// 	<c>true</c> if the specified file is blocked; otherwise, <c>false</c>.
		/// </returns>
		public static Boolean IsUnblocked(FileInfo file)
		{
			Boolean result = false;

			if (
				file.Exists &&
				((file.Attributes & FileAttributes.ReadOnly) == 0))
			{
				result = true;
			}

			return result;
		}
		#endregion

		#region IsSentMail
		/// <summary>
		/// Determines whether the specified file is a eml-file that is already sent.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <returns>
		/// 	<c>true</c> if the specified file is mail; otherwise, <c>false</c>.
		/// </returns>
		public static Boolean IsSentMail(FileInfo file)
		{
			Boolean result = false;

			if (
				file.Exists &&
				file.Extension == Properties.Settings.Default.MailFileExtension &&
				file.Name.StartsWith(Properties.Settings.Default.SentMailFilePrefix))
			{
				result = true;
			}

			return result;
		}
		#endregion

		#region MarkAsSpam
		public static void MarkAsSpam(FileInfo file)
		{
			try
			{
				//Open
				file.Attributes = FileAttributes.Normal;
				StreamReader reader = File.OpenText(file.FullName);
				String messageText = reader.ReadToEnd();
				

				//Modify
				Int32 insertPosition = messageText.IndexOf(Properties.Settings.Default.SubjectKeyword);
				insertPosition += Properties.Settings.Default.SubjectKeyword.Length;
				messageText = messageText.Insert(
					 insertPosition,
					 Properties.Settings.Default.SubjectSpamExtensionKeyword);

				//Write
				reader.Close();
				File.WriteAllBytes(file.FullName, Encoding.Default.GetBytes(messageText));
				file.Attributes = FileAttributes.ReadOnly;

				Tracing.Message("Mail marked as spam: " + file.FullName);
			}
			catch (Exception ex)
			{
				throw new Exception("Eml-File could not be marked as spam", ex);
			}
		}
		#endregion

		#region Forward
		/// <summary>
		/// Forwards the eml file to the recepient that is determined from the folder of the eml file.
		/// </summary>
		/// <param name="destinationIp">The destinationIp.</param>
		/// <param name="destinationHostname">The destinationHostname.</param>
		/// <returns>True if the mail was forwarded. False otherwise.</returns>
		public static Boolean Forward(FileInfo file, String destinationIp, String destinationHostname)
		{
			Boolean result = false;

			try
			{
				LumiSoft.Net.Mime.Mime mimemessage = LumiSoft.Net.Mime.Mime.Parse(file.FullName);
				MemoryStream messageStream = new MemoryStream();
				mimemessage.ToStream(messageStream);
				messageStream.Position = 0;

				LumiSoft.Net.SMTP.Client.SmtpClientEx smtpClient = new LumiSoft.Net.SMTP.Client.SmtpClientEx();
				smtpClient.Connect(destinationIp, 25);
				smtpClient.Ehlo(destinationHostname);
				smtpClient.SetSender(EmlFile.GetSender(mimemessage), messageStream.Length);
				smtpClient.AddRecipient(EmlFile.GetRecepient(file));
				smtpClient.SendMessage(messageStream);
				smtpClient.Disconnect();

				messageStream.Close();
				result = true;
			}
			catch (Exception ex)
			{
				throw new Exception("Forwarding failed", ex);
			}

			return result;
		}
		#endregion

		#region GetSender
		private static String GetSender(LumiSoft.Net.Mime.Mime mimemessage)
		{
			String messageInMime = null;

			try
			{
				messageInMime = (mimemessage.MainEntity.From[0] as LumiSoft.Net.Mime.MailboxAddress).EmailAddress;
				messageInMime = new System.Net.Mail.MailAddress(messageInMime).Address;
			}
			catch
			{
				messageInMime = "spam@spam.org";
			}

			messageInMime = String.Format("<{0}>", messageInMime);

			return messageInMime;
		}
		#endregion

		#region GetRecepientDomain
		public static String GetRecepientDomain(FileInfo file)
		{
			return file.Directory.Parent.Name;
		}
		#endregion

		#region GetRecepientMailbox
		private static String GetRecepientMailbox(FileInfo file)
		{
			String result = file.Directory.Name;
			result = result.Replace("P3_", String.Empty);
			result = result.Replace(".mbx", String.Empty);
			return result;
		}
		#endregion

		#region GetRecepient
		private static String GetRecepient(FileInfo file)
		{
			String result = "<{0}@{1}>";
			result = String.Format(
				result,
				EmlFile.GetRecepientMailbox(file),
				EmlFile.GetRecepientDomain(file));
			return result;
		}
		#endregion
	}
}
