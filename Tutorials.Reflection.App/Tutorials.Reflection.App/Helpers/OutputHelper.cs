using System;
using System.Text;

namespace Tutorials.Reflection.App.Helpers
{
    public static class OutputHelper
    {
        public static void OutputResult(StringBuilder stringBuilder)
        {
            Console.WriteLine(stringBuilder.ToString());
            Console.ReadKey();
            Console.Clear();
        }

        public static void WriteHeader(string value)
        {
            Console.WriteLine("************************************************");
            Console.WriteLine($"* {value.PadRight(48, ' ').Substring(0, 45)}*");
            Console.WriteLine("************************************************");
        }
    }
}
