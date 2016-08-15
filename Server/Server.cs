using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace BordeauxRCServer
{
    internal class Server
    {
        internal List<Connection> connections = new List<Connection>();

        internal List<string> cmds = new List<string>();

        private StreamWriter STDIN;

        private Process p;

        internal bool failedToLoad = false;
        internal bool running = false;

        private bool stopping = false;

        internal string name = null;
        internal string pass = null;
        internal string path = null;
        internal string args = null;

        internal Server(string nameArg, string passArg, string pathArg, string JVMArgs)
        {
            name = nameArg;
            pass = passArg;
            path = pathArg.Replace('\\', '/');
            args = JVMArgs;
            if(! File.Exists(path))
            {
                Program.MainDisplay("[" + name + "] Error: Failed to Load: Path to jar \"" + path + "\" does not exist.");
                failedToLoad = true;
            }
        }

        private void CommandLoop()
        {
            while (running)
            {
                if (cmds.Count > 0 && (! stopping))
                {
                    STDIN.WriteLine(cmds[0]);
                    cmds.RemoveAt(0);
                }
                Thread.Sleep(250);
            }
        }

        internal void ConnectUser(Connection conn)
        {
            connections.Add(conn);
        }

        internal void DisconnectUser(Connection conn)
        {
            connections.Remove(conn);
        }

        internal void Start(object starter)
        {
            if(running)
            {
                DataSend("Server already running.");
                return;
            }

            p = new Process();

            /*string javaPath = Environment.GetEnvironmentVariable("JAVA_HOME");
            Console.WriteLine(javaPath);
            if (string.IsNullOrEmpty(javaPath))
            {
                if (starter.GetType() == typeof(MainClass))
                {
                    Program.MainDisplay("Error: Could not find java");
                }
                else
                {
                    DataSend("Error: Could not find java");
                }
                return;
            }*/
            string javaPath = Util.IsLinux() ? "/usr/bin/java" : "C:/Program Files/Java/jre1.8.0_66/bin/java.exe";

            p.StartInfo.FileName = javaPath;
            p.StartInfo.Arguments = args + " -jar " + path;
            p.StartInfo.WorkingDirectory = path.Substring(0, path.LastIndexOf('/'));

            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;

            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            p.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            STDIN = p.StandardInput;
            running = true;
            new Thread(new ThreadStart(CommandLoop)).Start();
            new Thread(new ThreadStart(WaitForServerExit)).Start();
            Program.MainDisplay("[" + name + "] Started by " + ((starter.GetType() == typeof(MainClass)) ? "administrator" : "client #" + ((Connection) starter).ID.ToString() + " @" + ((Connection) starter).GetIP().ToString()));
        }

        public string GetProcSpec()
        {
            return (running ? string.Format("Memory used: {0} bytes\r\nThread count: {1}", p.WorkingSet64, p.Threads.Count) : "Server not online.");
        }

        private void WaitForServerExit()
        {
            p.WaitForExit();
            Program.MainDisplay("[" + name + "] Process has exited.");
            DataSend("Server has exited.");
            stopping = false;
            running = false;
        }

        internal bool ForceStop(object sender)
        {
            if (running)
            {
                p.Kill();
                running = false;
                DataSend("Server forcibly stopped by " + ((sender.GetType() == typeof(MainClass)) ? "administrator" : "another client"));
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ForceStop_()
        {
            Thread.Sleep(pause * 1000);
            if (running)
            {
                stopping = true;
                Program.MainDisplay("[" + name + "] Exceeded wait limit of " + pause.ToString() + " seconds. Forcing exit.");
                p.Kill();
                Program.MainDisplay("[" + name + "] Exited.");
                stopping = false;
            }
        }

        private int pause;

        internal bool SoftStop(int maxWait)
        {
            if (running)
            {
                stopping = true;
                STDIN.WriteLine("stop");
                pause = maxWait;
                new Thread(new ThreadStart(ForceStop_)).Start();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OutputHandler(object sender, DataReceivedEventArgs args)
        {
            DataSend(args.Data);
        }

        internal event Net.DataSendHandler DataSend = delegate { };
    }
}
