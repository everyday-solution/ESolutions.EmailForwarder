using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Reflection;

namespace ESolutions.EmailScanner
{
	public partial class EmailScannerService : ServiceBase
	{
		#region mailProcessor
		/// <summary>
		/// The VirusScanner-object scanning new emails.
		/// </summary>
		private MailProcessor mailProcessor = null;
		#endregion

		#region EmailScannerService
		/// <summary>
		/// Standardconstructor
		/// </summary>
		public EmailScannerService ()
		{
			InitializeComponent ();

			if (Debugger.IsAttached)
			{
				this.OnStart (null);
			}
		}
		#endregion

		#region OnStart
		/// <summary>
		/// Starts the file system watcher on the smtp directory.
		/// </summary>
		/// <param name="args"></param>
		protected override void OnStart (string[] args)
		{
			this.mailProcessor = new MailProcessor ();
			this.mailProcessor.Start();
		}
		#endregion

		#region OnStop
		/// <summary>
		/// Stops the file system watcher.
		/// </summary>
		protected override void OnStop ()
		{
			this.mailProcessor.Stop();
		}
		#endregion
	}
}
