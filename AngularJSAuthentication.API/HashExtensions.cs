using Microsoft.Owin.Security.DataHandler.Encoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace AngularJSAuthentication.API
{
    public static class HashExtensions
    {
        public static string GetHash(this string input)
        {
            using (HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider())
            {
                byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(input);

                byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);

                return TextEncodings.Base64Url.Encode(byteHash);
            }
        }
    }
}