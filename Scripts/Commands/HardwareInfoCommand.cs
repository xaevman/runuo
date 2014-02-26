using RunUO.Accounting;
using RunUO.Targeting;
using System;
using System.Net;

namespace RunUO.Commands
{
	class HardwareInfoCommand
	{
		public static void Initialize()
		{
			CommandSystem.Register("HWInfo", AccessLevel.GameMaster, new CommandEventHandler(HWInfo_OnCommand));
		}

		[Usage("HWInfo")]
		[Description("Displays information about a targeted player's hardware.")]
		public static void HWInfo_OnCommand(CommandEventArgs e)
		{
			e.Mobile.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(HWInfo_OnTarget));
			e.Mobile.SendMessage("Target a player to view their hardware information.");
		}

		public static void HWInfo_OnTarget(Mobile from, object obj)
		{
			if (obj is Mobile && ((Mobile)obj).Player)
			{
				Mobile m = (Mobile)obj;
				Account acct = m.Account as Account;

				if (acct != null)
				{
					HardwareInfo hwInfo = acct.HardwareInfo;

					if (hwInfo != null)
						CommandLogging.WriteLine(from, "{0} {1} viewing hardware info of {2}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(m));

					if (hwInfo != null)
						from.SendGump(new Gumps.PropertiesGump(from, hwInfo));
					else
						from.SendMessage("No hardware information for that account was found.");
				}
				else
				{
					from.SendMessage("No account has been attached to that player.");
				}
			}
			else
			{
				from.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(HWInfo_OnTarget));
				from.SendMessage("That is not a player. Try again.");
			}
		}
	}
}
