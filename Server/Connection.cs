using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BordeauxRCServer
{
    internal class Connection
    {
        private MainClass main;

        internal int ID;

        internal bool connected = true;

        internal bool verified = false;
        internal bool inPass = false;
        internal string salt;

        internal Server unverifiedServer;
        internal Server verifiedServer;

        private Socket sckt;

        private List<string> sendQueue = new List<string>();

        private IPAddress IP;

        internal IPAddress GetIP()
        {
            return IP;
        }

        private byte[] data;

        private Thread loginWaiter;

        internal Connection(Socket scktArg, MainClass m, int ID_)
        {
            main = m;
            sckt = scktArg;
            IP = ((IPEndPoint)sckt.RemoteEndPoint).Address;
            ID = ID_;
            loginWaiter = new Thread(new ThreadStart(WaitForLogin));
            loginWaiter.Start();
            data = new Byte[1];
            new Thread(new ThreadStart(SendLoop)).Start();
            new Thread(new ThreadStart(Listen)).Start();
            sendQueue.Add("Hello " + IP.ToString() + ". We accepted your connection, and you have been assigned ID " + ID.ToString() + ".");
        }

        private void WaitForLogin()
        {
            Thread.Sleep(10000);
            if (!verified)
            {
                Program.MainDisplay("Client #" + ID.ToString() + " @" + IP.ToString() + " failed to send login info. Disconnecting.");
                ClientDisconnected(this, new Net.ConnectionArgs(this));
            }
        }

        private string conjMsg = "";

        private void Listen()
        {
            if (!connected)
            {
                return;
            }
            try
            {
                sckt.BeginReceive(data, 0, data.Length, SocketFlags.None, new AsyncCallback(OnDataReceived), null);
            }
            catch (SocketException ex)
            {
                Program.MainDisplay("Error on connection #" + ID.ToString() + " @" + IP.ToString() + ": " + ex.Message);
                ClientDisconnected(this, new Net.ConnectionArgs(this));
            }
            catch (ObjectDisposedException)
            {
                Program.MainDisplay("Encountered disposed object exception on connection #" + ID.ToString() + " @" + IP.ToString() + "; disconnecting.");
                ClientDisconnected(this, new Net.ConnectionArgs(this));
            }
        }

        private bool inString = false;
        private string lengthNums = "";
        private int msgLength = 0;

        private void OnDataReceived(IAsyncResult res)
        {
            if (!connected)
            {
                return;
            }
            if (inString)
            {
                msgLength--;
                conjMsg += Encoding.UTF8.GetString(data);
                if (msgLength == 0)
                {
                    DataReceived(this, new Net.ConnectionArgs(this), conjMsg);
                    conjMsg = "";
                    lengthNums = "";
                    inString = false;
                }
            }
            else
            {
                if (Encoding.UTF8.GetString(data) == "#")
                {
                    Program.MainDisplay("Client #" + ID.ToString() + " @" + GetIP().ToString() + " sent disconnect packet. Disconnecting.");
                    ClientDisconnected(this, new Net.ConnectionArgs(this));
                }
                else if (Encoding.UTF8.GetString(data) == "@")
                {
                    if (!int.TryParse(lengthNums, out msgLength))
                    {
                        SendMessage("Error: Invalid packet sent.");
                        Program.MainDisplay("Client #" + ID.ToString() + " @" + GetIP().ToString() + " sent invalid packet. Diconnecting.");
                        ClientDisconnected(this, new Net.ConnectionArgs(this));
                    }
                    inString = true;
                }
                else
                {
                    lengthNums += Encoding.UTF8.GetString(data);
                }
            }
            try
            {
                sckt.EndReceive(res);
            }
            catch (SocketException se)
            {
                Program.MainDisplay("Encountered socket exception from client #" + ID.ToString() + " @" + IP.ToString() + ": " + se.Message);
                Program.MainDisplay("Disconnecting " + IP.ToString() + ".");
                ClientDisconnected(this, new Net.ConnectionArgs(this));
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            Listen();
        }

        internal void SendMessage(string msg)
        {
            sendQueue.Add(msg);
        }

        private void SendLoop()
        {
            while (connected)
            {
                if (sendQueue.Count > 0)
                {
                    if (string.IsNullOrEmpty(sendQueue[0]))
                    {
                        sendQueue.RemoveAt(0);
                        continue;
                    }
                    try
                    {
                        sckt.Send(Encoding.UTF8.GetBytes(sendQueue[0].Length.ToString() + "@" + sendQueue[0]));
                    }
                    catch (SocketException se)
                    {
                        Program.MainDisplay("Encountered socket exception from client #" + ID.ToString() + " @" + GetIP().ToString() + "; detailed message: " + se.Message);
                    }
                    sendQueue.RemoveAt(0);
                }
                Thread.Sleep((sendQueue.Count > 10) ? 25 : 50);
            }
        }

        internal void Disconnect()
        {
            connected = false;
            try
            {
                sckt.Send(Encoding.UTF8.GetBytes("#"));
                sckt.Shutdown(SocketShutdown.Both);
                sckt.Close();
            }
            catch (SocketException) { }
            if (!(loginWaiter.ThreadState == ThreadState.Stopped))
            {
                loginWaiter.Abort();
            }
        }

        internal event Net.DataReceivedHandler DataReceived;

        internal event Net.ClientDisconnectedHandler ClientDisconnected;
    }
}