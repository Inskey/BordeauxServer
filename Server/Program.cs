using System;
using System.IO;

namespace BordeauxRCServer
{
    class Program
    {
        static void Main(string[] args)
        {
            new MainClass(args);
        }

        internal static string version = "Prerelease";

        internal static void MainDisplay(string msg)
        {
            string msgFormatted = "[" + DateTime.Now.ToString("hh:mm:ss") + "] " + msg;
            Console.WriteLine(msgFormatted);
        }

        internal static void MainDisplayNoLine(string msg)
        {
            string msgFormatted = "[" + DateTime.Now.ToString("hh:mm:ss") + "] " + msg;
            Console.Write(msgFormatted);
        }
    }
}
