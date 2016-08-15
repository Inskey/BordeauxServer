using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace BordeauxRCServer
{
    internal class MainClass
    {
        internal bool stopped = false;

        private int IDindex = 0;

        private Socket sckt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        internal List<Connection> connections = new List<Connection>();

        internal List<Server> servers = new List<Server>();

        private List<IPAddress> whitelist = new List<IPAddress>();
        private bool doWhitelist = false;

        private List<IPAddress> connectionThrottle = new List<IPAddress>();

        internal MainClass(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Initialising...");
                if (Directory.Exists(Directory.GetCurrentDirectory() + "/BordeauxRCServer"))
                {
                    if (Directory.Exists(Directory.GetCurrentDirectory() + "/BordeauxRCServer/Servers"))
                    {
                        Program.MainDisplay("Loading Servers...");
                        string[] serverFiles = Directory.GetFiles(Directory.GetCurrentDirectory() + "/BordeauxRCServer/Servers", "*.txt");
                        int v = 0;
                        foreach (string serverFile in serverFiles)
                        {
                            string[] lines = File.ReadAllLines(serverFile);
                            if (lines.Length < 4)
                            {
                                Program.MainDisplay("Could not load server in file \"" + serverFile + "\": Not enough information.");
                                continue;
                            }
                            if (lines[1][0] == '@')
                            {
                                lines[1] = lines[1].Remove(0, 1);
                            }
                            else
                            {
                                string encrypted = Util.Hash(lines[1]);
                                using (StreamWriter writer = new StreamWriter(serverFile))
                                {
                                    writer.WriteLine(lines[0]);
                                    writer.WriteLine("@" + encrypted);
                                    writer.WriteLine(lines[2]);
                                    writer.WriteLine(lines[3]);
                                }
                                lines[1] = encrypted;
                            }
                            Server s = new Server(lines[0], lines[1], lines[2], lines[3]);
                            if (s.failedToLoad)
                            {
                                Program.MainDisplay("Failed to load " + lines[0] + ".");
                            }
                            else
                            {
                                servers.Add(s);
                                Program.MainDisplay("Loaded \"" + lines[0] + "\".");
                            }
                            v++;
                        }
                        Program.MainDisplay("Loaded " + v.ToString() + " Servers.");
                        Program.MainDisplay("Loading IP whitelist...");
                        if (File.Exists(Directory.GetCurrentDirectory() + "/BordeauxRCServer/whitelist.txt"))
                        {
                            string[] lines = File.ReadAllLines(Directory.GetCurrentDirectory() + "/BordeauxRCServer/whitelist.txt");
                            v = 0;
                            foreach (string line in lines)
                            {
                                IPAddress ip;
                                if (String.IsNullOrEmpty(line))
                                {
                                    continue;
                                }
                                if (IPAddress.TryParse(line, out ip))
                                {
                                    Program.MainDisplay("Loaded " + ip.ToString() + ".");
                                    whitelist.Add(ip);
                                    v++;
                                }
                                else
                                {
                                    Program.MainDisplay("Failed to parse IP " + line + ".");
                                }
                            }
                            using (StreamWriter sw = File.CreateText(Directory.GetCurrentDirectory() + "/BordeauxRCServer/whitelist.txt"))
                            {
                                string text = "";
                                foreach (string line in lines)
                                {
                                    if (String.IsNullOrEmpty(line))
                                    {
                                        continue;
                                    }
                                    text += line + sw.NewLine;
                                }
                                sw.Write(text);
                                sw.Close();
                            }
                            Program.MainDisplay("Loaded " + v.ToString() + " IPs from whitelist.");

                            if (File.Exists(Directory.GetCurrentDirectory() + "/BordeauxRCServer/config.properties"))
                            {
                                Program.MainDisplay("Loading configuration values...");
                                List<string> propsRaw = new List<string>(File.ReadAllLines(Directory.GetCurrentDirectory() + "/BordeauxRCServer/config.properties"));
                                Dictionary<string, string> props = new Dictionary<string,string>();
                                foreach (string prop in propsRaw)
                                {
                                    if (prop[0] == '#' || String.IsNullOrEmpty(prop)) { continue; }
                                    string[] pair = prop.Split('=');
                                    if (pair.Length < 2)
                                    {
                                        Program.MainDisplay("Error: Invalid format. Skipping " + prop);
                                        continue;
                                    }
                                    props.Add(pair[0], pair[1]);
                                }

                                string whitelist;
                                if (props.TryGetValue("whitelist", out whitelist))
                                {
                                    if (whitelist == "true")
                                    {
                                        Program.MainDisplay("Whitelist: true");
                                        doWhitelist = true;
                                    }
                                    else
                                    {
                                        if (whitelist == "false")
                                        {
                                            Program.MainDisplay("Whitelist: false");
                                            doWhitelist = false;
                                        }
                                        else
                                        {
                                            Program.MainDisplay("Invalid value for whitelist. Using default \"false\".");
                                            doWhitelist = false;
                                        }
                                    }
                                }
                                else
                                {
                                    Program.MainDisplay("No value for whitelist. Using default \"false\", and adding it to the config file.");
                                    doWhitelist = false;
                                    propsRaw.Add("whitelist=false");
                                    FileStream fs = File.OpenWrite(Directory.GetCurrentDirectory() + "/BordeauxRCServer/config.properties");
                                    string configText = "";
                                    foreach (string line in propsRaw)
                                    {
                                        configText += line + "\n";
                                    }
                                    fs.Write(Encoding.UTF8.GetBytes(configText), 0, Encoding.UTF8.GetByteCount(configText));
                                    fs.Close();
                                }
                                Program.MainDisplay("Configuration loaded.");
                                break;
                            }
                            else
                            {
                                Program.MainDisplayNoLine("Config file doesn't exist. Creating one now...");
                                using (StreamWriter sw_ = File.CreateText(Directory.GetCurrentDirectory() + "/BordeauxRCServer/config.properties"))
                                {
                                    sw_.Write("# Bordeaux Server Configuration File #" + sw_.NewLine
                                        + "whitelist=false");
                                    sw_.Close();
                                }
                                Console.WriteLine("Done. Reloading...");
                                continue;
                            }
                        }
                        else
                        {
                            Program.MainDisplayNoLine("Whitelist file doesn't exist. Creating one now...");
                            FileStream stub = File.Create(Directory.GetCurrentDirectory() + "/BordeauxRCServer/whitelist.txt");
                            stub.Close();
                            Console.WriteLine("Done. Reloading...");
                            continue;
                        }
                    }
                    else
                    {
                        Program.MainDisplayNoLine("Servers folder not found. Creating one now... ");
                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/BordeauxRCServer/Servers");
                        Console.WriteLine("Done. Reloading...");
                        continue;
                    }
                }
                else
                {
                    Program.MainDisplayNoLine("Root folder not found. Creating one now...");
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/BordeauxRCServer");
                    Console.WriteLine("Done. Reloading...");
                    continue;
                } 
            }
            BeginClientAccept();
        }

        internal void BeginClientAccept()
        {
            Program.MainDisplay("Opening Socket...");
            try
            {
                sckt.Bind(new IPEndPoint(IPAddress.Any, 54011));
                sckt.Listen(4);
                sckt.BeginAccept(new AsyncCallback(AcceptLoop), null);
                Program.MainDisplay("Socket successfully created; listening on " + ((IPEndPoint) sckt.LocalEndPoint).Address.ToString() + ":" + ((IPEndPoint) sckt.LocalEndPoint).Port.ToString() + ".");
            }
            catch (SocketException se)
            {
                if (se.ErrorCode == 10048)
                {
                    Program.MainDisplay("Error: Port already in use!");
                }
                Program.MainDisplay(se.Message);
            }
            catch (Exception ex)
            {
                Program.MainDisplay(ex.Message);
            }
            Program.MainDisplay("Initialisation complete.");
            ConsoleLoop();
        }

        private string loginTooFastMessage = "Connection Denied: You logged in too fast after a previous login attempt.";

        internal void AcceptLoop(IAsyncResult result)
        {
            try
            {
                Socket s = sckt.EndAccept(result);
                if (stopped) { return; }
                IPAddress ip = ((IPEndPoint) s.RemoteEndPoint).Address;
                if (connectionThrottle.Contains(ip))
                {
                    s.Send(Encoding.UTF8.GetBytes(loginTooFastMessage.Length.ToString() + "@" + loginTooFastMessage));
                    s.Send(Encoding.UTF8.GetBytes("#"));
                    s.Disconnect(false);
                    s.Dispose();
                    sckt.BeginAccept(new AsyncCallback(AcceptLoop), null);
                }
                connectionThrottle.Add(ip);
                Thread conTimer = new Thread(() => connectionTimer(ip));
                conTimer.Start();
                bool inWhitelist;
                if (! doWhitelist)
                {
                    inWhitelist = true;
                }
                else
                {
                    inWhitelist = false;
                    if (ip.ToString() == "127.0.0.1")
                    {
                        inWhitelist = true;
                    }
                    else
                    {
                        foreach (IPAddress ip_ in whitelist)
                        {
                            if (ip_ == ip)
                            {
                                inWhitelist = true;
                            }
                        }
                    }
                }
                if (inWhitelist)
                {
                    Connection c = new Connection(s, this, IDindex);
                    c.DataReceived += new Net.DataReceivedHandler(OnDataReceived);
                    c.ClientDisconnected += new Net.ClientDisconnectedHandler(OnClientDisconnected);
                    connections.Add(c);
                    Program.MainDisplay("Client #" + c.ID.ToString() + " connected with IP " + ((IPEndPoint)s.RemoteEndPoint).Address.ToString());
                }
                else
                {
                    Program.MainDisplay("Client #" + IDindex.ToString() + " @" + ip.ToString() + " connected was but denied becuase they are not whitelisted.");
                    string whitelistFailMessage = "Error: Your IP (" + ip.ToString() + ") is not on the whitelist.";
                    s.Send(Encoding.UTF8.GetBytes(whitelistFailMessage.Length.ToString() + "@" + whitelistFailMessage));
                    s.Send(Encoding.UTF8.GetBytes("#"));
                }
                IDindex++;
                sckt.BeginAccept(new AsyncCallback(AcceptLoop), null);
            }
            catch (ObjectDisposedException)
            {
                if (!stopped)
                {
                    Program.MainDisplay("Socket accept loop closed.");
                }
            }
            catch (Exception ex)
            {
                if (! stopped)
                {
                    Program.MainDisplay(ex.Message);
                }
            }
        }

        private void connectionTimer(IPAddress ip)
        {
            Thread.Sleep(5000);
            connectionThrottle.Remove(ip);
        }

        private void ConsoleLoop()
        {
            while (true)
            {
                Console.Write("> ");
                string inp = Console.ReadLine();
                if (inp.Equals(""))
                {
                    continue;
                }
                string[] inpParts = inp.Split(new char[1] { ' ' });
                bool found = false;
                switch (inpParts[0])
                {
                    case "":
                        break;
                    case "fdip":
                    case "forcedisconnectip":
                        if (inpParts.Length < 2)
                        {
                            Program.MainDisplay("Error: Please specify an IP to disconnect.");
                        }
                        else
                        {
                            bool er = true;
                            foreach (Connection con in connections)
                            {
                                if (con.GetIP().ToString() == inpParts[1])
                                {
                                    con.SendMessage("Connection forcibly closed by administrator.");
                                    OnClientDisconnected(this, new Net.ConnectionArgs(con));
                                    Program.MainDisplay("Forcibly closed connection of client #" + con.ID.ToString() + " @" + con.GetIP().ToString());
                                    er = false;
                                    break;
                                }
                            }
                            if (er)
                            {
                                Program.MainDisplay("Error: Connection from " + inpParts[1] + " not found.");
                            }
                        }
                        break;
                    case "fd":
                    case "forcedisconnect":
                        if (inpParts.Length < 2)
                        {
                            Program.MainDisplay("Error: Please specify a connection ID to disconnect.");
                        }
                        else
                        {
                            bool er = true;
                            foreach (Connection con in connections)
                            {
                                if (con.ID.ToString() == inpParts[1])
                                {
                                    con.SendMessage("Connection forcibly closed by administrator.");
                                    OnClientDisconnected(this, new Net.ConnectionArgs(con));
                                    Program.MainDisplay("Forcibly closed connection of client #" + con.ID.ToString() + " @" + con.GetIP().ToString());
                                    er = false;
                                    break;
                                }
                            }
                            if (er)
                            {
                                Program.MainDisplay("Error: Connection with ID " + inpParts[1] + " not found.");
                            }
                        }
                        break;
                    case "lc":
                    case "listconnections":
                        Program.MainDisplay("Connections open: " + connections.Count.ToString());
                        foreach (Connection con in connections)
                        {
                            if (con.verified)
                            {
                                Program.MainDisplay("Connection #" + con.ID.ToString() + " from IP " + con.GetIP().ToString() + ", logged into server " + con.verifiedServer.name + ".");
                            }
                            else
                            {
                                Program.MainDisplay("Connection #" + con.ID.ToString() + " from IP " + con.GetIP().ToString() + ", unverified.");
                            }
                        }
                        break;
                    case "ls":
                    case "listservers":
                        int scount = 0;
                        foreach (Server s in servers)
                        {
                            Program.MainDisplay("\"" + s.name + "\":\nPswd: " + s.pass + "\nPath: " + s.path + "\nArgs: " + s.args + "\nCurrently running: " + s.running.ToString());
                            scount++;
                        }
                        Program.MainDisplay("Listed " + scount.ToString() + " servers.");
                        break;
                    case "st":
                    case "start":
                        if (inpParts.Length < 2)
                        {
                            Program.MainDisplay("Error: Not enough arguments.");
                            break;
                        }
                        found = false;
                        foreach (Server s in servers)
                        {
                            if (s.name.Equals(inpParts[1]))
                            {
                                Program.MainDisplay("Starting server " + inpParts[1] + "...");
                                s.Start(this);
                                found = true;
                                break;
                            }
                        }
                        if (! found)
                        {
                            Program.MainDisplay("Error: Server \"" + inpParts[1] + "\" not found.");
                        }
                        break;
                    case "stop":
                        if (inpParts.Length < 2)
                        {
                            Program.MainDisplay("Error: Not enough arguments.");
                            break;
                        }
                        int wait = 0;
                        if (inpParts.Length > 2) 
                        { 
                            if (! int.TryParse(inpParts[2], out wait))
                            {
                                Program.MainDisplay("Error: Time argument not an integer. Using default.");
                                wait = 20;
                            }
                        }
                        found = false;
                        foreach (Server s in servers)
                        {
                            if (s.name.Equals(inpParts[1]))
                            {
                                Program.MainDisplayNoLine("Stopping " + s.name + "... ");
                                Console.Write((s.SoftStop(wait)) ? "stopped." : "already stopped.");
                                found = true;
                            }
                        }
                        if (! found)
                        {
                            Program.MainDisplay("Error: Server \"" + inpParts[1] + "\" not found.");
                        }
                        break;
                    case "dc":
                    case "dispatchcommand":
                        if (inpParts.Length < 3)
                        {
                            Program.MainDisplay("Error: Not enough arguments.");
                            break;
                        }
                        string cmd = "";
                        for (int c = 2; c < inpParts.Length; c++)
                        {
                            cmd += inpParts[c] + ((inpParts.Length == c + 1) ? "" : " ");
                        }
                        if (inpParts[1].Equals("*"))
                        {
                            foreach (Server s in servers)
                            {
                                if (s.running)
                                {
                                    s.cmds.Add(cmd);
                                    Program.MainDisplay("Successfully dispacted command \"" + cmd + "\" to server " + s.name + ".");
                                }
                                else
                                {
                                    Program.MainDisplay("Error: Server \"" + s.name + "\" not running.");
                                }
                            }
                            break;
                        }
                        bool fail = true;
                        foreach (Server s in servers)
                        {
                            if (s.name.Equals(inpParts[1]))
                            {
                                if (s.running)
                                {
                                    s.cmds.Add(cmd);
                                    Program.MainDisplay("Successfully dispacted command \"" + cmd + "\" to server " + s.name + ".");
                                }
                                else
                                {
                                    Program.MainDisplay("Error: Server \"" + s.name + "\" not running.");
                                }
                                fail = false;
                                break;
                            }
                        }
                        if (fail)
                        {
                            Program.MainDisplay("Failed to find server with name \"" + inpParts[1] + "\"");
                        }
                        break;
                    case "bc":
                    case "broadcast":
                        if (inpParts.Length < 2)
                        {
                            Program.MainDisplay("Error: Not enough arguments.");
                            break;
                        }
                        string msg = "";
                        for (int c = 1; c < inpParts.Length; c++)
                        {
                            msg += inpParts[c] + ( (inpParts.Length == c + 1) ? "" : " " );
                        }
                        foreach (Connection con in connections)
                        {
                            con.SendMessage("[BROADCAST] " + msg);
                        }
                        Program.MainDisplay("Sent message \"" + msg + "\" to all connected clients.");
                        break;
                    /*case "reload":
                        Program.restart = true;
                        stopped = true;
                        List<Connection> cons = new List<Connection>(connections);
                        foreach(Connection con in cons)
                        {
                            Program.MainDisplay("Disconnecting client #" + con.ID.ToString() + " @" + con.GetIP().ToString() + ".");
                            OnClientDisconnected(this, new Net.ConnectionArgs(con));
                        }
                        Console.Write("Closing socket... ");
                        sckt.Close();
                        Console.WriteLine("closed.");
                        foreach(Server srvr in servers)
                        {
                            if (srvr.SoftStop(20))
                            {
                                Console.Write("Stopping " + srvr.name + "... ");
                                Console.WriteLine("stopped.");
                            }
                            else
                            {
                                Console.WriteLine("Server " + srvr.name + " already stopped.");
                            }
                        }
                        return;*/
                    case "wl":
                    case "whitelist":
                        if (inpParts.Length < 2)
                        {
                            Program.MainDisplay("Error: Not enough arguments");
                            break;
                        }
                        if (inpParts[1] == "add" || inpParts[1] == "a")
                        {
                            if (inpParts.Length < 3)
                            {
                                Program.MainDisplay("Error: Please specify an IP to add to the whitelist.");
                                break;
                            }
                            IPAddress ip;
                            if (IPAddress.TryParse(inpParts[2], out ip))
                            {
                                if (whitelist.Contains(ip))
                                {
                                    Program.MainDisplay("Error: \"" + inpParts[2] + "\" already on the whitelist.");
                                    break;
                                }
                                whitelist.Add(ip);
                                using (StreamWriter writer = File.AppendText(Directory.GetCurrentDirectory() + "/BordeauxRCServer/whitelist.txt"))
                                {
                                    writer.WriteLine(ip.ToString());
                                }
                                Program.MainDisplay("Added " + ip.ToString() + " to whitelist.");
                                break;
                            }
                            else
                            {
                                Program.MainDisplay("Error: Unable to parse " + inpParts[2] + " into an IP.");
                                break;
                            }
                        }
                        if (inpParts[1] == "list" || inpParts[1] == "l")
                        {
                            int v = 0;
                            foreach (IPAddress ip in whitelist)
                            {
                                Program.MainDisplay(ip.ToString());
                                v++;
                            }
                            Program.MainDisplay("Listed " + v.ToString() + " IPs on whitelist.");
                            break;
                        }
                        if (inpParts[1] == "remove" || inpParts[1] == "r")
                        {
                            if (inpParts.Length < 3)
                            {
                                Program.MainDisplay("Error: Not enough arguments");
                                break;
                            }
                            bool found_ = false;
                            foreach (IPAddress ip in whitelist)
                            {
                                if (ip.ToString() == inpParts[2])
                                {
                                    whitelist.Remove(ip);
                                    using (StreamWriter sw = File.CreateText(Directory.GetCurrentDirectory() + "/BordeauxRCServer/whitelist.txt"))
                                    {
                                        string text = "";
                                        foreach (IPAddress ip_ in whitelist)
                                        {
                                            text += ip_.ToString() + sw.NewLine;
                                        }
                                        sw.Write(text);
                                        sw.Close();
                                    }
                                    Program.MainDisplay("Removed " + ip.ToString() + " from whitelist.");
                                    found_ = true;
                                    break;
                                }
                            }
                            if (! found_)
                            {
                                Program.MainDisplay("Could not find IP " + inpParts[2] + " in whitelist.");
                            }
                            break;
                        }
                        if (inpParts[1] == "on")
                        {
                            doWhitelist = true;
                            Program.MainDisplay("Now whitelisting.");
                            break;
                        }
                        if (inpParts[1] == "off")
                        {
                            doWhitelist = false;
                            Program.MainDisplay("Now ignoring whitelist.");
                            break;
                        }
                        Program.MainDisplay("Error: Unknown argument \"" + inpParts[1] + "\".");
                        break;
                    case "help":
                        Program.MainDisplay("-=Command Help=-\n" +
                            "1. forcedisconnect (fd) <integer ID>: Force close a connection by Connection ID.\n" +
                            "2. forcedisconnectip (fdip) <IPAddress IP>: Force close a connection by IP.\n" +
                            "3. listconnections (lc): List all open connections with IP and Connection ID.\n" +
                            "4. listservers (ls): List all servers and basic info about them.\n" +
                            "5. start (st) <string name>: Start the specified server.\n" +
                            "6. stop <string name | *> <integer waitInSec (default 20)>: Stop the specified server (or * for all servers) softly, or forcibly kill the process if <waitInSec> is exceeded.\n" +
                            "7. dispatchcommand (dc) <string name | *> <string cmd>: Send the command to the specified server (or * for all servers).\n" +
                            "8. broadcast (bc) <string msg>: Broadcast a message to all connected clients.\n" +
                            "9. whitelist (wl):\n" +
                            "  a. list: List all IPs on whitelist.\n" +
                            "  b. add (a) <string ip>: Add the specified IP to the whitelist.\n" +
                            "  c. remove (r) <string ip>: Remove the specified IP from the whitelist.\n" +
                            "  d. on | off: Turn whitelist on or off.\n" +
                            "10. version: View server software version.\n" +
                            "11. exit: Safely exit the program.\n" +
                            "12. hardexit (he): Forcefully close the program. (servers will be killed and connections unexpectedly dropped)");
                        break;
                    case "version":
                        Program.MainDisplay("Server version: " + Program.version);
                        break;
                    case "exit":
                        stopped = true;
                        List<Connection> conns = new List<Connection>(connections);
                        foreach(Connection con in conns)
                        {
                            Program.MainDisplay("Disconnecting client #" + con.ID.ToString() + " @" + con.GetIP().ToString() + ".");
                            OnClientDisconnected(this, new Net.ConnectionArgs(con));
                        }
                        Program.MainDisplayNoLine("Closing socket... ");
                        sckt.Close();
                        Console.WriteLine("closed.");
                        foreach(Server srvr in servers)
                        {
                            if (srvr.SoftStop(20))
                            {
                                Program.MainDisplayNoLine("Stopping " + srvr.name + "... ");
                                Console.WriteLine("stopped.");
                            }
                            else
                            {
                                Program.MainDisplay("Server " + srvr.name + " already stopped.");
                            }
                        }
                        Console.Write("Ready to exit. Press any key.");
                        Console.ReadKey();
                        Environment.Exit(0);
                        return;
                    case "he":
                    case "hardexit":
                        stopped = true;
                        Program.MainDisplayNoLine("Closing socket... ");
                        sckt.Close();
                        Console.WriteLine("closed.");
                        foreach(Connection con in connections)
                        {
                            Program.MainDisplay("Disconnecting client #" + con.ID.ToString() + " @" + con.GetIP().ToString() + ".");
                            OnClientDisconnected(this, new Net.ConnectionArgs(con));
                        }
                        foreach(Server srvr in servers)
                        {
                            if (srvr.ForceStop(this))
                            {
                                Program.MainDisplayNoLine("Killing server " + srvr.name + "... ");
                                Console.WriteLine("killed.");
                            }
                            else
                            {
                                Program.MainDisplay("Server " + srvr.name + " already dead.");
                            }
                        }
                        Console.Write("Ready to exit. Press any key.");
                        Console.ReadKey();
                        Environment.Exit(0);
                        break;
                    default:
                        Program.MainDisplay("Error: Unknown command \"" + inpParts[0] + "\"");
                        break;
                }
            }
        }

        internal void OnDataReceived(object sender, Net.ConnectionArgs args, string received)
        {
            if (args.Connection.verified)
            {
                if (received[0] == ':')
                {
                    string[] parts = received.Remove(0, 1).Split(new char[1] { ' ' });
                    Program.MainDisplay("Client #" + args.Connection.ID.ToString() + " issued command " + parts[0]);
                    switch (parts[0])
                    {
                        case "start":
                            args.Connection.SendMessage("Starting " + args.Connection.verifiedServer.name + "...");
                            args.Connection.verifiedServer.Start(args.Connection);
                            break;
                        case "kill":
                            if (args.Connection.verifiedServer.running)
                            {
                                args.Connection.SendMessage("Killing server.");
                                args.Connection.verifiedServer.ForceStop(args.Connection);
                            }
                            else
                            {
                                args.Connection.SendMessage("Server is not currently running.");
                            }
                            break;
                        case "procspec":
                            args.Connection.SendMessage(args.Connection.verifiedServer.GetProcSpec());
                            break;
                        case "version":
                            args.Connection.SendMessage("Server version: " + Program.version);
                            break;
                        case "help":
                            args.Connection.SendMessage("-=Command Help=-\r\n" +
                                "start: Start the jarfile for the server.\r\n" +
                                "kill: Forcibly stop the server.\r\n" +
                                "procspec: Show info about resource usage.\r\n" +
                                "version: Display the server-side software version.\r\n" +
                                "disconnect: Safely close the connection.");
                            break;
                        case "disconnect":
                            OnClientDisconnected(args.Connection, new Net.ConnectionArgs(args.Connection));
                            break;
                        default:
                            args.Connection.SendMessage("Error: Command \"" + parts[0] + "\" not found.");
                            break;
                    }
                }
                else
                {
                    if (args.Connection.verifiedServer.running)
                    {
                        args.Connection.verifiedServer.cmds.Add(received);
                    }
                    else
                    {
                        args.Connection.SendMessage("Error: Server is not currently running.");
                    }
                }
            }
            else
            {
                if (! args.Connection.connected)
                {
                    return;
                }
                if (args.Connection.inPass)
                {
                    if (received == Util.Hash(args.Connection.unverifiedServer.pass + args.Connection.salt))
                    {
                        args.Connection.verifiedServer = args.Connection.unverifiedServer;
                        args.Connection.verifiedServer.ConnectUser(args.Connection);
                        args.Connection.verifiedServer.DataSend += new Net.DataSendHandler(args.Connection.SendMessage);
                        args.Connection.verified = true;
                        Program.MainDisplay("Client #" + args.Connection.ID.ToString() + " @" + args.Connection.GetIP().ToString() + " logged into server " + args.Connection.unverifiedServer.name + " successfully.");
                        args.Connection.SendMessage("Password good. You are now logged in.");
                    }
                    else
                    {
                        args.Connection.SendMessage("Error: Password for server " + args.Connection.unverifiedServer.name + " incorrect. Disconnecting.");
                        Program.MainDisplay("Client #" + args.Connection.ID.ToString() + " @" + args.Connection.GetIP().ToString() + " failed to log into server " + args.Connection.unverifiedServer.name + " and was disconnected.");
                        Thread.Sleep(500);
                        OnClientDisconnected(this, args);
                    }
                }
                else
                {
                    bool found = false;
                    foreach (Server s in servers)
                    {
                        if (s.name == received)
                        {
                            foreach (Connection con in s.connections)
                            {
                                if (args.Connection.GetIP().ToString() == con.GetIP().ToString())
                                {
                                    Program.MainDisplay("Client #" + args.Connection.ID.ToString() + " @" + args.Connection.GetIP().ToString() + " already has connection open to server \"" + s.name + "\". Disconnecting.");
                                    args.Connection.SendMessage("You already have a connection logged into " + s.name + ". Login denied.");
                                    Thread.Sleep(500);
                                    OnClientDisconnected(this, args);
                                    return;
                                }
                            }
                            args.Connection.unverifiedServer = s;
                            args.Connection.inPass = true;
                            args.Connection.salt = Util.RandomString();
                            args.Connection.SendMessage("salt:" + args.Connection.salt);
                            args.Connection.SendMessage("Found requested login \"" + s.name + "\". Waiting for passwd.");
                            found = true;
                        }
                    }
                    if (! found)
                    {
                        Program.MainDisplay("Client #" + args.Connection.ID.ToString() + " @" + args.Connection.GetIP().ToString() + " did not send valid username. Disconnecting.");
                        args.Connection.SendMessage("We could not find the login \"" + "\". Login denied.");
                        Thread.Sleep(500);
                        OnClientDisconnected(this, args);
                    }
                }
            }   
        }

        internal void OnClientDisconnected(object sender, Net.ConnectionArgs args)
        {
            Program.MainDisplay(args.Connection.GetIP().ToString() + " disconnected by " + ((sender.GetType() == typeof(MainClass)) ? "an administrator." : "itself."));
            if (args.Connection.verified)
            {
                args.Connection.verifiedServer.DisconnectUser(args.Connection);
            }
            connections.Remove(args.Connection);
            args.Connection.Disconnect();
        }
    }
}