using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace ESolutions.EmailScanner
{
	/// <summary>
	/// This class is used to monitor a directory for incoming eml files.
	/// </summary>
	public class EmailWatcher
	{
		#region watcher
		/// <summary>
		/// File system watcher on the mail directory.
		/// </summary>
		private System.IO.FileSystemWatcher watcher = null;
		#endregion

		#region StartWatching
		/// <summary>
		/// Starts the FileSystemWatcher on the mailroot directory and all its subfolders.
		/// </summary>
		public void StartWatching (DirectoryInfo di)
		{
			Tracing.EnterMethod (MethodBase.GetCurrentMethod ());

			if (watcher == null && di.Exists)
			{
				this.watcher = new FileSystemWatcher (di.FullName);
				this.watcher.IncludeSubdirectories = true;
				this.watcher.EnableRaisingEvents = true;
				this.watcher.Created += new FileSystemEventHandler (this.Watcher_Created);
			}

			Tracing.ExitMethod (MethodBase.GetCurrentMethod ());
		}
		#endregion

		#region StopWatching
		/// <summary>
		/// Stops the FileSystemWatcher
		/// </summary>
		public void StopWatching ()
		{
			Tracing.EnterMethod (MethodBase.GetCurrentMethod ());

			if (this.watcher != null)
			{
				watcher.EnableRaisingEvents = false;
				watcher = null;
			}

			Tracing.ExitMethod (MethodBase.GetCurrentMethod ());
		}
		#endregion

		#region Watcher_Created
		/// <summary>
		/// Handles the file system watcher created event.
		/// </summary>
		private void Watcher_Created (object sender, FileSystemEventArgs e)
		{
			FileInfo file = new FileInfo (e.FullPath);

			if (EmlFile.IsUnsentMail (file))
			{
				this.OnNewEmail (file);
			}
		}
		#endregion

		#region OnNewEmail
		/// <summary>
		/// Fires the NewEmail event.
		/// </summary>
		protected void OnNewEmail (System.IO.FileInfo file)
		{
			Tracing.EnterMethod (MethodBase.GetCurrentMethod ());

			if (this.NewMail != null)
			{
				this.NewMail (this, new NewMailEventArgs (file));
			}

			Tracing.ExitMethod (MethodBase.GetCurrentMethod ());
		}
		#endregion

		#region NewMail
		/// <summary>
		/// Fired each time a new mail is detected.
		/// </summary>
		public event NewMailEventHandler NewMail;
		#endregion
	}
}
