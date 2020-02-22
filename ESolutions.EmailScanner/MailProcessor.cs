using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace ESolutions.EmailScanner
{
	public class MailProcessor
	{
		//Fields
		#region emailWatcher
		/// <summary>
		/// Notifies the scanners if an incoming mail is detected.
		/// </summary>
		private EmailWatcher emailWatcher = new EmailWatcher();
		#endregion

		#region forwardingThread
		/// <summary>
		/// This thread fowards the mails.
		/// </summary>
		private Thread forwardingThread = null;
		#endregion

		#region forwardTimer
		/// <summary>
		/// Eacht time this timer elapsed the mails in the forward queue are forwarded.
		/// </summary>
		private System.Timers.Timer forwardTimer = new System.Timers.Timer();
		#endregion

		//Event handling
		#region Watcher_NewMail
		/// <summary>
		/// Whenever the emailwatcher detected incoming mails they scanned for virusses
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void Watcher_NewMail(object sender, NewMailEventArgs e)
		{
			try
			{
				ScanJob job = new ScanJob(e.File);
				job.BeginScan();
			}
			catch (Exception ex)
			{
				Tracing.Exception(ex);
			}
		}
		#endregion

		#region Timer_Elapsed
		/// <summary>
		/// Timer elapsed and all mails that are still remaining in the folder are processed.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
		void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			try
			{
				this.forwardTimer.Stop();
				this.BeginForwarding();
				this.forwardTimer.Start();
			}
			catch (Exception ex)
			{
				Tracing.Exception(ex);
			}
		}
		#endregion

		//Methods
		#region Start
		/// <summary>
		/// Starts the mail processor.
		/// </summary>
		public void Start()
		{
			try
			{
				this.emailWatcher.NewMail += new NewMailEventHandler(Watcher_NewMail);
				this.emailWatcher.StartWatching(new DirectoryInfo(Properties.Settings.Default.MailFolder));

				this.forwardingThread = new Thread(new ThreadStart(this.BeginForwardingThreadStart));

				this.forwardTimer.Interval = 1000 * 60;
				this.forwardTimer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);
				this.forwardTimer.Start();
			}
			catch (Exception ex)
			{
				throw new Exception("MailProcessor can not be started.", ex);
			}
		}
		#endregion

		#region Stop
		/// <summary>
		/// Stops this mail processor.
		/// </summary>
		public void Stop()
		{
			try
			{
				this.emailWatcher.StopWatching();
				this.forwardTimer.Stop();
				this.forwardingThread.Abort();
			}
			catch (Exception ex)
			{
				throw new Exception("Failure during stopping MailProcessor.", ex);
			}
		}
		#endregion

		#region BeginForwarding
		/// <summary>
		/// Begins forwarding the eml-files in the forwardingQueue
		/// </summary>
		private void BeginForwarding()
		{
			try
			{
				if (this.forwardingThread.IsAlive == false)
				{
					this.forwardingThread = new Thread(new ThreadStart(this.BeginForwardingThreadStart));
					this.forwardingThread.Start();
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Forwarding-Thread can not be started.", ex);
			}
		}
		#endregion

		#region BeginForwardingThreadStart
		/// <summary>
		/// Threadstart method for the BeginForwarding method.
		/// </summary>
		private void BeginForwardingThreadStart()
		{
			try
			{
				List<FileInfo> forwardList = new List<FileInfo>();
				this.GetUnblockedAndUnsentEmlFilesInDirectory(
					new DirectoryInfo(Properties.Settings.Default.MailFolder),
					ref forwardList);

				this.ForwardMails(forwardList);
			}
			catch (Exception ex)
			{
				Tracing.Exception(ex);
			}
		}
		#endregion

		#region ForwardMails
		/// <summary>
		/// Forwards all files in the files parameter to the configured destination.
		/// </summary>
		/// <param name="files">The files.</param>
		private void ForwardMails(List<FileInfo> files)
		{
			foreach (FileInfo current in files)
			{
				try
				{
					String ip = ForwardingConfiguration.IpOfDomain(EmlFile.GetRecepientDomain(current));
					String host = ForwardingConfiguration.HostnameOfDomain(EmlFile.GetRecepientDomain(current));

					if (EmlFile.Forward(current, ip, host))
					{
						EmlFile.MarkAsSent(current);
					}
				}
				catch (Exception ex)
				{
					Tracing.Exception(ex);
				}
			}
		}
		#endregion

		#region GetUnblockedAndUnsentEmlFilesInDirectory
		/// <summary>
		/// Gets all eml-files from the mailfolder that are marked as unsent.
		/// </summary>
		/// <param name="start">The start.</param>
		/// <param name="mails">The mails.</param>
		private void GetUnblockedAndUnsentEmlFilesInDirectory(DirectoryInfo start, ref List<FileInfo> mails)
		{
			foreach (DirectoryInfo current in start.GetDirectories())
			{
				this.GetUnblockedAndUnsentEmlFilesInDirectory(current, ref mails);
			}

			foreach (FileInfo current in start.GetFiles())
			{
				if (
					EmlFile.IsUnblocked(current) &&
					EmlFile.IsUnsentMail(current) &&
					ForwardingConfiguration.IsConfiguredForForwarding(EmlFile.GetRecepientDomain(current)))
				{
					mails.Add(current);
				}
			}
		}
		#endregion
	}
}
