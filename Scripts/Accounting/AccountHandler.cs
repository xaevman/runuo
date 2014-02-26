using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using RunUO;
using RunUO.Accounting;
using RunUO.Network;
using RunUO.Security;

namespace RunUO.Accounting
{
	public enum PasswordProtection
	{
		None,
		Crypt,
		NewCrypt
	}

	public class AccountHandler
	{
		private static int MaxAccountsPerIP = 1;
		private static bool AutoAccountCreation = true;
		private static TimeSpan DeleteDelay = TimeSpan.FromDays( 7.0 );

		public static PasswordProtection ProtectPasswords = PasswordProtection.NewCrypt;

		private static bool m_RestrictCharacterDeletion = true;

		public static bool RestrictCharacterDeletion
		{
			get { return m_RestrictCharacterDeletion; }
			set { m_RestrictCharacterDeletion = value; }
		}

		private static AccessLevel m_LockdownLevel;

		public static AccessLevel LockdownLevel
		{
			get{ return m_LockdownLevel; }
			set{ m_LockdownLevel = value; }
		}

		private static CityInfo[] StartingCities = new CityInfo[]
			{
				new CityInfo( "New Haven",	"New Haven Bank",	1150168, 3667,	2625,	0  ),
				new CityInfo( "Yew",		"The Empath Abbey",	1075072, 633,	858,	0  ),
				new CityInfo( "Minoc",		"The Barnacle",		1075073, 2476,	413,	15 ),
				new CityInfo( "Britain",	"The Wayfarer's Inn",	1075074, 1602,	1591,	20 ),
				new CityInfo( "Moonglow",	"The Scholars Inn",	1075075, 4408,	1168,	0  ),
				new CityInfo( "Trinsic",	"The Traveler's Inn",	1075076, 1845,	2745,	0  ),
				new CityInfo( "Jhelom",		"The Mercenary Inn",	1075078, 1374,	3826,	0  ),
				new CityInfo( "Skara Brae",	"The Falconer's Inn",	1075079, 618,	2234,	0  ),
				new CityInfo( "Vesper",		"The Ironwood Inn",	1075080, 2771,	976,	0  )
			};

		/* Old Haven/Magincia Locations
			new CityInfo( "Britain", "Sweet Dreams Inn", 1496, 1628, 10 );
			// ..
			// Trinsic
			new CityInfo( "Magincia", "The Great Horns Tavern", 3734, 2222, 20 ),
			// Jhelom
			// ..
			new CityInfo( "Haven", "Buckler's Hideaway", 3667, 2625, 0 )

			if ( Core.AOS )
			{
				//CityInfo haven = new CityInfo( "Haven", "Uzeraan's Mansion", 3618, 2591, 0 );
				CityInfo haven = new CityInfo( "Haven", "Uzeraan's Mansion", 3503, 2574, 14 );
				StartingCities[StartingCities.Length - 1] = haven;
			}
		*/

		public static void Initialize()
		{
			EventSink.DeleteRequest += new DeleteRequestEventHandler( EventSink_DeleteRequest );
			EventSink.AccountLogin += new AccountLoginEventHandler( EventSink_AccountLogin );
			EventSink.GameLogin += new GameLoginEventHandler( EventSink_GameLogin );
		}

		private static void EventSink_DeleteRequest( DeleteRequestEventArgs e )
		{
			NetState state = e.State;
			int index = e.Index;

			Account acct = state.Account as Account;

			if ( acct == null )
			{
				state.Dispose();
			}
			else if ( index < 0 || index >= acct.Length )
			{
				state.Send( new DeleteResult( DeleteResultType.BadRequest ) );
				state.Send( new CharacterListUpdate( acct ) );
			}
			else
			{
				Mobile m = acct[index];

				if ( m == null )
				{
					state.Send( new DeleteResult( DeleteResultType.CharNotExist ) );
					state.Send( new CharacterListUpdate( acct ) );
				}
				else if ( m.NetState != null )
				{
					state.Send( new DeleteResult( DeleteResultType.CharBeingPlayed ) );
					state.Send( new CharacterListUpdate( acct ) );
				}
				else if ( RestrictCharacterDeletion && DateTime.UtcNow < (m.CreationTime + DeleteDelay) )
				{
					state.Send( new DeleteResult( DeleteResultType.CharTooYoung ) );
					state.Send( new CharacterListUpdate( acct ) );
				}
				else if ( m.AccessLevel == AccessLevel.Player && Region.Find( m.LogoutLocation, m.LogoutMap ).GetRegion( typeof( Jail ) ) != null )	//Don't need to check current location, if netstate is null, they're logged out
				{
					state.Send( new DeleteResult( DeleteResultType.BadRequest ) );
					state.Send( new CharacterListUpdate( acct ) );
				}
				else
				{
					Console.WriteLine( "Client: {0}: Deleting character {1} (0x{2:X})", state, index, m.Serial.Value );

					acct.Comments.Add( new AccountComment( "System", String.Format( "Character #{0} {1} deleted by {2}", index + 1, m, state ) ) );

					m.Delete();
					state.Send( new CharacterListUpdate( acct ) );
				}
			}
		}

		public static bool CanCreate( IPAddress ip )
		{
			if ( !IPTable.ContainsKey( ip ) )
				return true;

			return ( IPTable[ip] < MaxAccountsPerIP );
		}

		private static Dictionary<IPAddress, Int32> m_IPTable;

		public static Dictionary<IPAddress, Int32> IPTable
		{
			get
			{
				if ( m_IPTable == null )
				{
					m_IPTable = new Dictionary<IPAddress, Int32>();

					foreach ( Account a in Accounts.GetAccounts() )
						if ( a.LoginIPs.Length > 0 )
						{
							IPAddress ip = a.LoginIPs[0];

							if ( m_IPTable.ContainsKey( ip ) )
								m_IPTable[ip]++;
							else
								m_IPTable[ip] = 1;
						}
				}

				return m_IPTable;
			}
		}

		private static readonly char[] m_ForbiddenChars = new char[]
		{
			'<', '>', ':', '"', '/', '\\', '|', '?', '*'
		};

		private static bool IsForbiddenChar( char c )
		{
			for ( int i = 0; i < m_ForbiddenChars.Length; ++i )
				if ( c == m_ForbiddenChars[i] )
					return true;

			return false;
		}

		private static Account CreateAccount( NetState state, string un, string pw )
		{
			if ( un.Length == 0 || pw.Length == 0 )
				return null;

			bool isSafe = !( un.StartsWith( " " ) || un.EndsWith( " " ) || un.EndsWith( "." ) );

			for ( int i = 0; isSafe && i < un.Length; ++i )
				isSafe = ( un[i] >= 0x20 && un[i] < 0x7F && !IsForbiddenChar( un[i] ) );

			for ( int i = 0; isSafe && i < pw.Length; ++i )
				isSafe = ( pw[i] >= 0x20 && pw[i] < 0x7F );

			if ( !isSafe )
				return null;

			if ( !CanCreate( state.Address ) )
			{
				Console.WriteLine( "Login: {0}: Account '{1}' not created, ip already has {2} account{3}.", state, un, MaxAccountsPerIP, MaxAccountsPerIP == 1 ? "" : "s" );
				return null;
			}

			Console.WriteLine( "Login: {0}: Creating new account '{1}'", state, un );

			Account a = new Account( un, pw );

			return a;
		}

		public static void EventSink_AccountLogin( AccountLoginEventArgs e )
		{
			if ( !IPLimiter.SocketBlock && !IPLimiter.Verify( e.State.Address ) )
			{
				e.Accepted = false;
				e.RejectReason = ALRReason.InUse;

				Console.WriteLine( "Login: {0}: Past IP limit threshold", e.State );

				using ( StreamWriter op = new StreamWriter( "ipLimits.log", true ) )
					op.WriteLine( "{0}\tPast IP limit threshold\t{1}", e.State, DateTime.UtcNow );

				return;
			}

			string un = e.Username;
			string pw = e.Password;

			e.Accepted = false;
			Account acct = Accounts.GetAccount( un ) as Account;

			if ( acct == null )
			{
				if ( AutoAccountCreation && un.Trim().Length > 0 ) // To prevent someone from making an account of just '' or a bunch of meaningless spaces
				{
					e.State.Account = acct = CreateAccount( e.State, un, pw );
					e.Accepted = acct == null ? false : acct.CheckAccess( e.State );

					if ( !e.Accepted )
						e.RejectReason = ALRReason.BadComm;
				}
				else
				{
					Console.WriteLine( "Login: {0}: Invalid username '{1}'", e.State, un );
					e.RejectReason = ALRReason.Invalid;
				}
			}
			else if ( !acct.HasAccess( e.State ) )
			{
				Console.WriteLine( "Login: {0}: Access denied for '{1}'", e.State, un );
				e.RejectReason = ( m_LockdownLevel > AccessLevel.Player ? ALRReason.BadComm : ALRReason.BadPass );
			}
			else if ( !acct.CheckPassword( pw ) )
			{
				Console.WriteLine( "Login: {0}: Invalid password for '{1}'", e.State, un );
				e.RejectReason = ALRReason.BadPass;
			}
			else if ( acct.Banned )
			{
				Console.WriteLine( "Login: {0}: Banned account '{1}'", e.State, un );
				e.RejectReason = ALRReason.Blocked;
			}
			else
			{
				Console.WriteLine( "Login: {0}: Valid credentials for '{1}'", e.State, un );
				e.State.Account = acct;
				e.Accepted = true;

				acct.LogAccess( e.State );
			}

			if ( !e.Accepted )
				AccountAttackLimiter.RegisterInvalidAccess( e.State );
		}

		public static void EventSink_GameLogin( GameLoginEventArgs e )
		{
			if ( !IPLimiter.SocketBlock && !IPLimiter.Verify( e.State.Address ) )
			{
				e.Accepted = false;

				Console.WriteLine( "Login: {0}: Past IP limit threshold", e.State );

				using ( StreamWriter op = new StreamWriter( "ipLimits.log", true ) )
					op.WriteLine( "{0}\tPast IP limit threshold\t{1}", e.State, DateTime.UtcNow );

				return;
			}

			string un = e.Username;
			string pw = e.Password;

			Account acct = Accounts.GetAccount( un ) as Account;

			if ( acct == null )
			{
				e.Accepted = false;
			}
			else if ( !acct.HasAccess( e.State ) )
			{
				Console.WriteLine( "Login: {0}: Access denied for '{1}'", e.State, un );
				e.Accepted = false;
			}
			else if ( !acct.CheckPassword( pw ) )
			{
				Console.WriteLine( "Login: {0}: Invalid password for '{1}'", e.State, un );
				e.Accepted = false;
			}
			else if ( acct.Banned )
			{
				Console.WriteLine( "Login: {0}: Banned account '{1}'", e.State, un );
				e.Accepted = false;
			}
			else
			{
				acct.LogAccess( e.State );

				Console.WriteLine( "Login: {0}: Account '{1}' at character list", e.State, un );
				e.State.Account = acct;
				e.Accepted = true;
				e.CityInfo = StartingCities;
			}

			if ( !e.Accepted )
				AccountAttackLimiter.RegisterInvalidAccess( e.State );
		}

		public static bool CheckAccount( Mobile mobCheck, Mobile accCheck )
		{
			if ( accCheck != null )
			{
				Account a = accCheck.Account as Account;

				if ( a != null )
				{
					for ( int i = 0; i < a.Length; ++i )
					{
						if ( a[i] == mobCheck )
							return true;
					}
				}
			}

			return false;
		}
	}
}