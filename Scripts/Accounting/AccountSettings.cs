using RunUO.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

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

			XDocument x = XDocument.Load(filePath);

			XElement e = x.Element("Configuration").Element("AccountSettings");

			bool auto = Utility.ToBoolean(e.Element("AutoAccountCreation").Value);
			int acctperip = Utility.GetXMLInt32(e.Element("MaxAccountsPerIP").Value, 1);
			int connperip = Utility.GetXMLInt32(e.Element("MaxConnectionsPerIP").Value, 10);
			bool restrictdelete = Utility.ToBoolean(e.Element("RestrictCharacterDeletion").Value);
			TimeSpan deletedelay = Utility.GetXMLTimeSpan(e.Element("CharacterDeletionDelay").Value, AccountHandler.DeleteDelay);
			PasswordProtection pp = PasswordProtection.NewCrypt;
			Enum.TryParse(e.Element("PasswordProtection").Value, true, out pp);
			TimeSpan youngduration = Utility.GetXMLTimeSpan(e.Element("YoungDuration").Value, Account.YoungDuration);
			TimeSpan inactiveduration = Utility.GetXMLTimeSpan(e.Element("InactiveDuration").Value, Account.InactiveDuration);
			TimeSpan emptyinactiveduration = Utility.GetXMLTimeSpan(e.Element("EmptyInactiveDuration").Value, Account.EmptyInactiveDuration);

			foreach(XElement c in e.Element("StartingLocations").Elements("CityInfo"))
			{
				new CityInfo(c.Attribute("cityName").Value, c.Attribute("buildingName").Value, Utility.GetXMLInt32(c.Attribute("description").Value, 0), Utility.GetXMLInt32(c.Attribute("x").Value, 0), Utility.GetXMLInt32(c.Attribute("y").Value, 0), Utility.GetXMLInt32(c.Attribute("z").Value, 0));
			}

			return true;
		}
	}
}