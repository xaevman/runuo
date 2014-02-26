using RunUO.Accounting;
using RunUO.Engines.Help;
using RunUO.Network;
using System;
using System.Net;

namespace RunUO.Commands
{
	class PasswordCommand
	{
		private static bool PasswordCommandEnabled = false;

		public static void Initialize()
		{
			if (PasswordCommandEnabled)
				CommandSystem.Register("Password", AccessLevel.Player, new CommandEventHandler(Password_OnCommand));
		}

		[Usage("Password <newPassword> <repeatPassword>")]
		[Description("Changes the password of the commanding players account. Requires the same C-class IP address as the account's creator.")]
		public static void Password_OnCommand(CommandEventArgs e)
		{
			Mobile from = e.Mobile;
			Account acct = from.Account as Account;

			if (acct == null)
				return;

			IPAddress[] accessList = acct.LoginIPs;

			if (accessList.Length == 0)
				return;

			NetState ns = from.NetState;

			if (ns == null)
				return;

			if (e.Length == 0)
			{
				from.SendMessage("You must specify the new password.");
				return;
			}
			else if (e.Length == 1)
			{
				from.SendMessage("To prevent potential typing mistakes, you must type the password twice. Use the format:");
				from.SendMessage("Password \"(newPassword)\" \"(repeated)\"");
				return;
			}

			string pass = e.GetString(0);
			string pass2 = e.GetString(1);

			if (pass != pass2)
			{
				from.SendMessage("The passwords do not match.");
				return;
			}

			bool isSafe = true;

			for (int i = 0; isSafe && i < pass.Length; ++i)
				isSafe = (pass[i] >= 0x20 && pass[i] < 0x7F);

			if (!isSafe)
			{
				from.SendMessage("That is not a valid password.");
				return;
			}

			try
			{
				IPAddress ipAddress = ns.Address;

				if (Utility.IPMatchClassC(accessList[0], ipAddress))
				{
					acct.SetPassword(pass);
					from.SendMessage("The password to your account has changed.");
				}
				else
				{
					PageEntry entry = PageQueue.GetEntry(from);

					if (entry != null)
					{
						if (entry.Message.StartsWith("[Automated: Change Password]"))
							from.SendMessage("You already have a password change request in the help system queue.");
						else
							from.SendMessage("Your IP address does not match that which created this account.");
					}
					else if (PageQueue.CheckAllowedToPage(from))
					{
						from.SendMessage("Your IP address does not match that which created this account.  A page has been entered into the help system on your behalf.");

						from.SendLocalizedMessage(501234, "", 0x35); /* The next available Counselor/Game Master will respond as soon as possible.
																	    * Please check your Journal for messages every few minutes.
																	    */

						PageQueue.Enqueue(new PageEntry(from, String.Format("[Automated: Change Password]<br>Desired password: {0}<br>Current IP address: {1}<br>Account IP address: {2}", pass, ipAddress, accessList[0]), PageType.Account));
					}

				}
			}
			catch
			{
			}
		}
	}
}
