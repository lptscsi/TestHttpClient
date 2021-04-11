using System;

namespace TestWorker.Extensions
{
    public static class StringExt
    {
        public static string Left(this string str, int length)
        {
            return str?.Substring(0, Math.Min(length, str.Length));
        }
    }
}
