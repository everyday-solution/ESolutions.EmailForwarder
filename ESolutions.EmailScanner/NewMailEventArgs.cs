using System;
using System.Collections.Generic;
using System.Text;

namespace ESolutions.EmailScanner
{
	public class NewMailEventArgs : EventArgs
	{
		#region file
		/// <summary>
		/// Backing field of File. The new email file.
		private System.IO.FileInfo file;
		#endregion

		#region File
		/// <summary>
		/// The new email file.
		/// </summary>
		public System.IO.FileInfo File
		{
			get
			{
				return this.file;
			}
		}
		#endregion

		#region NewMailEventArgs
		/// <summary>
		/// Constructor
		/// </summary>
		public NewMailEventArgs (System.IO.FileInfo newEmailFile)
		{
			this.file = newEmailFile;
		}
		#endregion
	}
}
