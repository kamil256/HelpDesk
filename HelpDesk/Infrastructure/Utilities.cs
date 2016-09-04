using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace HelpDesk.Infrastructure
{
    public static class Utilities
    {
        public static string HashPassword(string password, string salt)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(password + salt);
            byte[] hash = System.Security.Cryptography.SHA256.Create().ComputeHash(buffer);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash)
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }
    }
}