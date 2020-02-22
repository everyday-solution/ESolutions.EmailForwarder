using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;

namespace ESolutions.EmailScanner
{
	public class ScanJob
	{
		#region file
		private FileInfo file = null;
		#endregion

		#region ScanJob
		/// <summary>
		/// Initializes a new instance of the <see cref="ScanJob"/> class.
		/// </summary>
		/// <param name="file">The file to scan</param>
		public ScanJob(FileInfo file)
		{
			this.file = file;
		}
		#endregion

		#region BeginScan
		/// <summary>
		/// Begins the scanning the file specified when the scanjob was created.
		/// </summary>
		public void BeginScan()
		{
			try
			{
				Thread scanThread = new Thread(new ThreadStart(this.BeginScanThreadStart));
				scanThread.Start();
			}
			catch (Exception ex)
			{
				throw new Exception("The scan could not be started.", ex);
			}
		}
		#endregion

		#region BeginScanThreadStart
		/// <summary>
		/// Waits until the specified file is accessible and then sets its file attributes to
		/// readonly to block the access of other processes such as pop3 clients. The mail is
		/// scanned for viruses first and deleted when infected. Second step is to scan the
		/// mail for spam issues. If the mail seem to be spam its subject is marked. Finally
		/// the file attributes are set to normal and the thread terminates.
		/// </summary>
		private void BeginScanThreadStart()
		{
			try
			{
				this.WaitForAccessAndBlock(this.file);

				//Virusscann
				if (VirusScanner.Scan(this.file) == VirusScanResult.Infected)
				{
					this.UnblockFile(this.file);
					this.file.Delete();
				}
				else
				{
					//Spamm scann
					if (SpamScanner.Scan(this.file) == SpamScanResult.Spam)
					{
						EmlFile.MarkAsSpam(this.file);
					}
				}
			}
			catch (Exception ex)
			{
				Tracing.Exception(ex);
			}
			finally
			{
				if (this.file.Exists)
				{
					this.UnblockFile(this.file);
				}
			}
		}
		#endregion

		#region WaitForAccessAndBlock
		/// <summary>
		/// Waits until the file can be accessed exlusivly and then sets the writeprotection of the file.
		/// </summary>
		/// <exception cref="Exception">Is thrown if the file can not be exclusively accessed within 120 second.</exception>
		private void WaitForAccessAndBlock(FileInfo file)
		{
			System.IO.FileStream stream = null;

			try
			{
				Int32 waitCounter = 0;

				while (stream == null)
				{
					if (waitCounter >= 120)
					{
						throw new Exception("Waiting times out.");
					}

					try
					{
						stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
						stream.Close();
						file.Attributes = FileAttributes.ReadOnly;
					}
					catch
					{
						System.Threading.Thread.Sleep(1000);
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("The file could not be access exclusivly within 120 seconds.", ex);
			}
			finally
			{
				if (stream != null)
				{
					stream.Close();
				}
			}
		}
		#endregion

		#region UnblockFile
		/// <summary>
		/// Unblocks the file by removing the readonly file attribute.
		/// </summary>
		/// <param name="file">The file.</param>
		private void UnblockFile(FileInfo file)
		{
			try
			{
				if (file.Exists)
				{
					file.Attributes = FileAttributes.Normal;
				}
			}
			catch (Exception ex)
			{
				throw new Exception("The file could not be unblocked. See inner exception for further detail.", ex);
			}
		}
		#endregion
	}
}
