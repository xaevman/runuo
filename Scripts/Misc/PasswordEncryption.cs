using Server.Accounting;
using Server.Network;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;

namespace Server.Misc
{
    public class PasswordEncryption
    {
        private static bool _SupportEncryptedLogin = true;

        public static bool SupportEncryptedLogin
        {
            get { return _SupportEncryptedLogin; }
            private set { _SupportEncryptedLogin = value; }
        }

        const int seedLen = 20;
        const int MaxReasonableEncodedPasswordLen = 80;

        static RandomNumberGenerator rnd = RandomNumberGenerator.Create();

        public static Dictionary<NetState, byte[]> LoginKeys=new Dictionary<NetState,byte[]>();

        static byte[] GetLoginKey(NetState state)
        {
            if (state == null) return null;

            if (!LoginKeys.ContainsKey(state))
            {
                byte[] bytes = new byte[seedLen];
                rnd.GetBytes(bytes);
                LoginKeys[state] = bytes;
            }
            return LoginKeys[state];
        }

        private static void RemoveLoginKey(NetState state)
        {
            if (state != null && LoginKeys.ContainsKey(state))
                LoginKeys.Remove(state);
        }

        public static void Configure()
        {
            if (SupportEncryptedLogin && AccountHandler.ProtectPasswords != PasswordProtection.NewCrypt)
            {
                Console.WriteLine("Encrypted Login Disabled: Password protection must be NewCrypt to use this feature.");
                SupportEncryptedLogin = false;
            }
        }

        public static void Initialize()
        {
            ProtocolExtensions.Register(0xFD, false, new OnPacketReceive(RequestEncryptedLogin));
            ProtocolExtensions.Register(0x80, false, new OnPacketReceive(EncryptedAccountLogin));
            ProtocolExtensions.Register(0x91, false, new OnPacketReceive(EncryptedGameLogin));

            Timer.DelayCall(TimeSpan.FromMinutes(2.0), TimeSpan.FromMinutes(1.2), CleanStates);
        }

        private static List<NetState> toRemove = new List<NetState>();
        private static void CleanStates()
        {
            foreach (NetState state in LoginKeys.Keys)
                if (!state.Running)
                    toRemove.Add(state);
            foreach (NetState state in toRemove)
                LoginKeys.Remove(state);
            toRemove.Clear();
        }

        private static void RequestEncryptedLogin(NetState state, PacketReader pvSrc)
        {
            EncryptedLoginResponse packet;
            packet = new EncryptedLoginResponse(SupportEncryptedLogin ? GetLoginKey(state) : null);
            Console.WriteLine("Login: {0}:Requested encrypted login.", state, SupportEncryptedLogin ? "" : "(Disabled)");
            state.Send(packet);
        }

        private static void EncryptedAccountLogin(NetState state, PacketReader pvSrc)
        {
            string username = pvSrc.ReadString();
            string encPass = pvSrc.ReadString();

            byte[] salt = GetLoginKey(state);
            if (salt != null)
            {
                RemoveLoginKey(state);

                if (encPass.Length <= MaxReasonableEncodedPasswordLen && salt.Length == seedLen)
                {
                    Account account = Accounts.GetAccount(username) as Account;
                    if (account != null && !string.IsNullOrEmpty(account.NewCryptPassword))
                    {
                        state.SentFirstPacket = true;
                        AccountLoginEventArgs args = new AccountLoginEventArgs(state, username, encPass);
                        Console.WriteLine("Login: {0}:Processing encrypted login.", state);
                        AccountHandler.AccountLogin(args, salt);
                        if (args.Accepted)
                            PacketHandlers.AccountLogin_ReplyAck(state);
                        else
                            PacketHandlers.AccountLogin_ReplyRej(state, args.RejectReason);
                        return;
                    }
                }
            }

            AccountAttackLimiter.RegisterInvalidAccess(state);
            state.Send(new EncryptedLoginDenied());
        }

        private static void EncryptedGameLogin(NetState state, PacketReader pvSrc)
        {
            //TODO:
        }


    }

    public sealed class EncryptedLoginResponse : ProtocolExtension
    {
        public EncryptedLoginResponse(byte[] key) : base(0xFD, (key == null ? 0 : key.Length) + 1)
        {
            if (key != null && key.Length > 0)
            {
                m_Stream.Write(true);
                for (int i = 0; i < key.Length; i++)
                    m_Stream.Write(key[i]);
            }
            else
            {
                m_Stream.Write(false);
            }
        }
    }

    public sealed class EncryptedLoginDenied : ProtocolExtension
    {
        public EncryptedLoginDenied()  : base(0x82, 0)
        {
        }
    }

}
