using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace ESolutions.EmailScanner
{
	/// <summary>
	/// Class used to write messages to the trace system.
	/// </summary>
	public static class Tracing
	{
		#region EnterMethod
		/// <summary>
		/// Traces the enterance of a method.
		/// </summary>
		public static void EnterMethod(MethodBase method)
		{
			/*Trace.WriteLine(
				Tracing.GetLeadingString() + 
				"Enter: " +
				method.Name);*/
		}
		#endregion

		#region ExitMethod
		/// <summary>
		/// Traces the exit of a method.
		/// </summary>
		public static void ExitMethod(MethodBase method)
		{
			/*Trace.WriteLine(
				Tracing.GetLeadingString() + 
				"Exit: " +
				method.Name);*/
		}
		#endregion

		#region Exception
		/// <summary>
		/// Traces an exception
		/// </summary>
		public static void Exception(Exception ex)
		{
			String innerexception = String.Empty;

			if (ex.InnerException != null)
			{
				innerexception = ex.InnerException.Message;
			}

			Trace.WriteLine(
				Tracing.GetLeadingString() + 
				"Exception - " + 
				ex.Message + 
				" | " + 
				innerexception);
		}
		#endregion

		#region Message
		/// <summary>
		/// Traces a free message.
		/// </summary>
		public static void Message(String message)
		{
			Trace.WriteLine(
				Tracing.GetLeadingString() + 
				message);
		}
		#endregion

		#region GetLeadingString
		public static String GetLeadingString()
		{
			return 
				DateTime.Now.ToLongDateString() +
				" - " +
				DateTime.Now.ToLongTimeString() +
				" - " +
				System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() +
				" - ";
		}
		#endregion
	}
}
