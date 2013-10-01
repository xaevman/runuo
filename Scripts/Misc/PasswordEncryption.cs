using Server.Network;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Server.Misc
{
    public class PasswordEncryption
    {
        static RandomNumberGenerator rnd = RandomNumberGenerator.Create();
        static RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
        static SHA1Managed hash = new SHA1Managed();

        public const bool SupportEncryptedLogin = true;

        public static Dictionary<NetState, byte[]> LoginKeys=new Dictionary<NetState,byte[]>();

        public static byte[] GetLoginKey(NetState state)
        {
            if (state == null) return null;

            if (!LoginKeys.ContainsKey(state))
            {
                byte[] bytes=new byte[128];
                rnd.GetBytes(bytes);
                LoginKeys[state] = bytes;
            }
            return LoginKeys[state];
        }

        public static void Initialize()
        {
            ProtocolExtensions.Register(0xFD, false, new OnPacketReceive(RequestEncryptedLogin));
            ProtocolExtensions.Register(0x80, false, new OnPacketReceive(EncryptedAccountLogin));
            ProtocolExtensions.Register(0x91, false, new OnPacketReceive(EncryptedGameLogin));
        }

        private static void RequestEncryptedLogin(NetState state, PacketReader pvSrc)
        {
            EncryptedLoginResponse packet;
            packet = new EncryptedLoginResponse(SupportEncryptedLogin ? GetLoginKey(state) : null);
            state.Send(packet);
        }

        private static void EncryptedAccountLogin(NetState state, PacketReader pvSrc)
        {
            //TODO:
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

}
