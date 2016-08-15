using System;
using System.Text;
using System.Security.Cryptography;

namespace BordeauxRCServer
{
    static class Util
    {
        internal static bool IsLinux()
        {
            int p = (int) Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);
        }

        static internal string Hash(string strPlain)
        {
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] HashValue, MessageBytes = UE.GetBytes(strPlain);
            SHA512Managed SHhash = new SHA512Managed();
            string strHex = "";
 
            HashValue = SHhash.ComputeHash(MessageBytes);
            foreach (byte b in HashValue)
            {
                strHex += String.Format("{0:x2}", b);
            }
            return strHex;
        }

        private static Random r = new Random();
        internal static string RandomString()
        {
            char[] str = new char[64];

            byte b = 0;
            while (b < 64)
            {
                int t = r.Next(3);
                if (t == 0)
                {
                    str[b] = (char)(r.Next(10) + 48); // ASCII 0-9
                }
                else if (t == 1)
                {
                    str[b] = (char)(r.Next(24) + 65); // ASCII A-Z
                }
                else
                {
                    str[b] = (char)(r.Next(24) + 97); // ASCII a-z
                }
                b++;
            }

            return new string(str);
        }
    }
}