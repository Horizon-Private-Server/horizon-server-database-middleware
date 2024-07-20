using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Horizon.Database
{

    public static class Utils
    {

#region SHA-256

        public static string ComputeSHA256(string input)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    builder.Append(bytes[i].ToString("x2"));

                return builder.ToString();
            }
        }

        public static string ComputeSHA512(string input)
        {
            // Convert input string to a byte array
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            // Compute hash value
            SHA512 sha512 = SHA512.Create();
            byte[] hashBytes = sha512.ComputeHash(inputBytes);

            // Convert hash bytes to a string
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

#endregion

    }
}
