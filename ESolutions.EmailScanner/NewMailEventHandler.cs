using System;
using System.Collections.Generic;
using System.Text;

namespace ESolutions.EmailScanner
{
	public delegate void NewMailEventHandler (
		object sender,
		NewMailEventArgs e);
}
