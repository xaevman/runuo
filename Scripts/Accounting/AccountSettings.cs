using RunUO.Network;
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
			if (Load())
				Console.WriteLine("done");
			else
				Console.WriteLine("failed");
		}

		public static bool Load()
		{
			string filePath = Path.Combine( "Data", "Configuration.xml" );

			if ( !File.Exists( filePath ) )
				return false;

			XmlDocument x = new XmlDocument();
			x.Load(filePath);

			XmlElement e = x["Configuration"]["AccountSettings"];

			bool auto = Utility.ToBoolean(e["AutoAccountCreation"].InnerText);
			int acctperip = Utility.GetXMLInt32(e["MaxAccountsPerIP"].InnerText, 1);
			int connperip = Utility.GetXMLInt32(e["MaxConnectionsPerIP"].InnerText, 10);
			bool restrictdelete = Utility.ToBoolean(e["RestrictCharacterDeletion"].InnerText);
			TimeSpan deletedelay = Utility.GetXMLTimeSpan(e["CharacterDeletionDelay"].InnerText, AccountHandler.DeleteDelay);
			PasswordProtection pp = PasswordProtection.NewCrypt;
			Enum.TryParse(e["PasswordProtection"].InnerText, true, out pp);
			TimeSpan youngduration = Utility.GetXMLTimeSpan(e["YoungDuration"].InnerText, Account.YoungDuration);
			TimeSpan inactiveduration = Utility.GetXMLTimeSpan(e["InactiveDuration"].InnerText, Account.InactiveDuration);
			TimeSpan emptyinactiveduration = Utility.GetXMLTimeSpan(e["EmptyInactiveDuration"].InnerText, Account.EmptyInactiveDuration);

			foreach(XmlElement c in e["StartingLocations"]["CityInfo"])
			{
				new CityInfo(c.GetAttribute("cityName"), c.GetAttribute("buildingName"), Utility.GetXMLInt32(c.GetAttribute("description"), 0), Utility.GetXMLInt32(c.GetAttribute("x"), 0), Utility.GetXMLInt32(c.GetAttribute("y"), 0), Utility.GetXMLInt32(c.GetAttribute("z"), 0));
			}

			return true;
		}
	}
}