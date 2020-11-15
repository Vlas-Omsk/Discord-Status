using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DiscordStatusGUI.Extensions
{
    static class SecureStringExtension
    {
        public static void Set(this SecureString secure, string value)
        {
            secure.Clear();

            if (string.IsNullOrEmpty(value))
                return;

            foreach (var ch in value)
                secure.AppendChar(ch);
        }

        public static string Get(this SecureString secure)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(secure);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }
    }
}
