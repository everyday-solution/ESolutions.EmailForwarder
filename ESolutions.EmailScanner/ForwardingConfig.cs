using System;
using System.Collections.Generic;
using System.Text;

namespace ESolutions.EmailScanner
{
	public static class ForwardingConfiguration
	{
		#region GetForwardingIpOfDomain
		/// <summary>
		/// Gets the forwarding destinationIp of domain.
		/// </summary>
		/// <param name="domain">The domain.</param>
		/// <returns></returns>
		public static string IpOfDomain(String domainName)
		{
			String result = String.Empty;

			try
			{
				SingleEntry entry = null;

				foreach (String current in Properties.Settings.Default.ForwardingList)
				{
					entry = new SingleEntry(current);

					if (entry.Domain == domainName)
					{
						result = entry.Ip;
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Could not determine the forwarding ip for the domain : " + domainName, ex);
			}

			return result;
		}
		#endregion

		#region GetForwardingIpOfDomain
		/// <summary>
		/// Gets the forwarding destinationIp of domain.
		/// </summary>
		/// <param name="domain">The domain.</param>
		/// <returns></returns>
		public static string HostnameOfDomain(String domainName)
		{
			String result = String.Empty;

			try
			{
				SingleEntry entry = null;

				foreach (String current in Properties.Settings.Default.ForwardingList)
				{
					entry = new SingleEntry(current);	

					if (entry.Domain == domainName)
					{
						result = entry.Hostname;
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Could not determine the forwarding hostname for the domain : " + domainName, ex);
			}

			return result;
		}
		#endregion

		#region IsConfiguredForForwarding
		/// <summary>
		/// Determines whether or not a specific domain is configured for mail forwarding.
		/// </summary>
		/// <param name="domainName">Name of the domain.</param>
		/// <returns>
		/// 	<c>true</c> if [is configured for forwarding] [the specified domain name]; otherwise, <c>false</c>.
		/// </returns>
		public static Boolean IsConfiguredForForwarding(String domainName)
		{
			Boolean result = false;

			try
			{
				foreach (String current in Properties.Settings.Default.ForwardingList)
				{
					if (new SingleEntry(current).Domain == domainName)
					{
						result = true;
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Could not determine whether or not the domain is configured for forwarding: " + domainName, ex);
			}

			return result;
		}
		#endregion

		//Classes
		#region SingleEntry
		private class SingleEntry
		{
			public String Domain;
			public String Ip;
			public String Hostname;

			#region SingleEntry
			public SingleEntry(String settingsString)
			{
				string[] data = settingsString.Split(new char[] { ';' });
				this.Domain = data[0];
				this.Ip = data[1];
				this.Hostname = data[2];
			}
			#endregion
		}
		#endregion
	}
}
