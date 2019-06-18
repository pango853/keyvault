using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SG.Algoritma;

namespace KeyVault
{
    internal static class SecretHelper
    {
        private static readonly Encoding ENCODING = Encoding.ASCII;

        public static string Encrypt(string str)
        {
            var salt = MainWindow.SALT;
            if (!String.IsNullOrWhiteSpace(salt))
                return Cipher.Encrypt(str, salt);
            else
                return Encode(str, ENCODING); // Just simple base64 it
        }

        private static string Encode(string str, Encoding encode)
        {
            return Convert.ToBase64String(encode.GetBytes(str));
        }

        public static string Decrypt(string str64)
        {
            var salt = MainWindow.SALT;
            if (!String.IsNullOrWhiteSpace(salt))
               return Cipher.Decrypt(str64, salt);
            else
                return Decode(str64, ENCODING);
        }

        private static string Decode(string base64Str, Encoding encode)
        {
            return encode.GetString(Convert.FromBase64String(base64Str));
        }
    }
}
