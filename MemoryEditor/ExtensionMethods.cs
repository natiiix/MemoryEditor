using System;
using System.Runtime.InteropServices;

namespace MemoryEditor
{
    public static class ExtensionMethods
    {
        public static bool ContainsCaseInsensitive(this String str, String value)
        {
            return str.ToLower().Contains(value.ToLower());
        }

        public static string ToHexString(this IntPtr ptr)
        {
            int strLen = Marshal.SizeOf(ptr) * 2;
            int partLen = 4;

            string hexStr = ptr.ToString("X").PadLeft(strLen, '0');

            string[] parts = new string[strLen / partLen];
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = hexStr.Substring(i * partLen, partLen);
            }

            return "0x " + string.Join(" ", parts);
        }
    }
}