using RunUO.Network;
using RunUO.Security;
using System;
using System.IO;
using System.Xml;

namespace RunUO.Accounting
{
	public class AccountSettings
	{
		public static void Configure()
		{
			Console.Write("Accounting: Loading settings...");
			if (LoadSettings())
				Console.WriteLine("done");
			else
				Console.WriteLine("failed");
		}

		public static bool LoadSettings()
		{
			string filePath = Path.Combine( "Data", "Configuration.xml" );

			if ( !File.Exists( filePath ) )
				return false;

			XmlDocument x = new XmlDocument();
			x.Load(filePath);

			try
			{
				XmlElement e = x["Configuration"];
				if (e == null) return false;
				e = e["AccountSettings"];
				if (e == null) return false;

				foreach (XmlElement s in e)
				{
					switch (s.Name)
					{
						case "AutoAccountCreation": AccountHandler.AutoAccountCreation = Utility.ToBoolean(s.InnerText); break;
						case "MaxAccountsPerIP": AccountHandler.MaxAccountsPerIP = Utility.GetXMLInt32(s.InnerText, 1); break;
						case "MaxConnectionsPerIP": IPLimiter.MaxAddresses = Utility.GetXMLInt32(s.InnerText, 10); break;
						case "RestrictCharacterDeletion": AccountHandler.RestrictCharacterDeletion = Utility.ToBoolean(s.InnerText); break;
					}

					TimeSpan deletedelay = Utility.GetXMLTimeSpan(e["CharacterDeletionDelay"].InnerText, AccountHandler.DeleteDelay);
					PasswordProtection pp = PasswordProtection.NewCrypt;
					Enum.TryParse(e["PasswordProtection"].InnerText, true, out pp);
					TimeSpan youngduration = Utility.GetXMLTimeSpan(e["YoungDuration"].InnerText, Account.YoungDuration);
					TimeSpan inactiveduration = Utility.GetXMLTimeSpan(e["InactiveDuration"].InnerText, Account.InactiveDuration);
					TimeSpan emptyinactiveduration = Utility.GetXMLTimeSpan(e["EmptyInactiveDuration"].InnerText, Account.EmptyInactiveDuration);

					foreach (XmlElement c in e["StartingLocations"]["CityInfo"])
					{
						new CityInfo(c.GetAttribute("cityName"), c.GetAttribute("buildingName"), Utility.GetXMLInt32(c.GetAttribute("description"), 0), Utility.GetXMLInt32(c.GetAttribute("x"), 0), Utility.GetXMLInt32(c.GetAttribute("y"), 0), Utility.GetXMLInt32(c.GetAttribute("z"), 0));
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return false;
			}

			return true;
		}
	}
}