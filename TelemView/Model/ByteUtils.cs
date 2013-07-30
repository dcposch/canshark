using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Globalization;

namespace SSCP.Telem.CanShark
{
    public static class ByteUtils
    {
        public static unsafe byte[] ToByteArray(byte* bytep, int len)
        {
            byte[] ret = new byte[len];
            for (int i = 0; i < len; i++)
            {
                ret[i] = bytep[i];
            }
            return ret;
        }
        public static string BytesToHex(byte[] bytes)
        {
            unsafe
            {
                fixed (byte* bytep = &bytes[0])
                {
                    return BytesToHex(bytep, bytes.Length);
                }
            }
        }
        public static unsafe string BytesToHex(byte *bytes, int len)
        {
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < len; i++)
            {
                ret.Append(string.Format("{0:x2}", bytes[i]));
            }
            return ret.ToString();
        }
        public static unsafe ulong BytesToUlong(byte[] bytes)
        {
            Debug.Assert(bytes.Length <= 8);
            ulong ret = 0;
            byte* bytep = (byte*)&ret;
            for (int i = 0; i < bytes.Length; i++)
            {
                bytep[i] = bytes[i];
            }
            return ret;
        }

        public static bool TryParseHex(string hex, out int result)
        {
            if (hex.StartsWith("0x"))
                return int.TryParse(hex.Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier, null, out result);
            return int.TryParse(hex, out result);
        }

    }
}
