using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace OwnCloud
{
    class Utility
    {
        static public string FormatBytes(long input)
        {
            string postfix = "";
            string format_code = "{0:0.0} {1:g}";
            int c = 1;
            double size = (double)input;
            
            if (size < 1024)
            {
                postfix = "KB";
                format_code = "{0:0} {1:g}";
                c = 1;
                size = 1;
            }
            else if (size >= 1024 && size < 1048576)
            {
                postfix = "KB";
                c = 1024;
            }
            else if (size >= 1048576 && size < 1073741824)
            {
                postfix = "MB";
                c = 1048576;
            }
            else
            {
                postfix = "GB";
                c = 1073741824;
            }

            return String.Format(format_code, size / c, postfix);
        }

        static public string EncodeString(string input)
        {
            if (input == null) return "";
            byte[] crypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(input), null);
            return System.Convert.ToBase64String(crypted, 0, crypted.Length);
        }

        static public string DecodeString(string input)
        {
            byte[] decrypted = ProtectedData.Unprotect(System.Convert.FromBase64String(input), null);
            return Encoding.UTF8.GetString(decrypted, 0, decrypted.Length);
        }

        static public void Debug(string input)
        {
            var max = 100;
            for (int index = 0; index < input.Length; index += max)
            {
                System.Diagnostics.Debug.WriteLine(input.Substring(index, Math.Min(max, input.Length - index)));
            }
        }

        static public void Debug(byte[] input)
        {
            var max = 100;
            for (int index = 0; index < input.Length; index += max)
            {
                var str = Encoding.UTF8.GetString(input, index, Math.Min(max, input.Length - index));
                System.Diagnostics.Debug.WriteLine(str);
            }
        }

        static public void DebugBytes(byte[] input)
        {
            System.Diagnostics.Debug.WriteLine("-------------------------------");
            System.Diagnostics.Debug.WriteLine("Debug Bytes Length: "+input.Length);
            string bytes = "";
            for (int index = 0; index < input.Length; ++index)
            {
                bytes += String.Format("{0:x2}", input[index]) + " ";


                if (index > 0 && (index+1) % 8 == 0)
                {
                    bytes += " ";
                }
                if (index > 0 && (index+1) % 16 == 0)
                {
                    System.Diagnostics.Debug.WriteLine(bytes);
                    bytes = "";
                }
            }
            if (bytes.Length > 0) System.Diagnostics.Debug.WriteLine(bytes);
            System.Diagnostics.Debug.WriteLine("-------------------------------");
        }
    }
}
