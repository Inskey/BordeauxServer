using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace BordeauxRCClient.Core
{
    class Connection
    {
        private Form1 form;

        public bool connected = false;

        public Socket sckt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private List<string> sendQueue = new List<string>();

        public Connection(Form1 form_)
        {
            form = form_;
            data = new byte[1];
        }

        public IPAddress hostIP;

        private bool waitSalt = true;
        private string salt;
        public void Connect(string host, string user, string pass)
        {
            IPAddress[] hostIPs = Dns.GetHostAddresses(host);
            bool found = false;
            foreach (IPAddress ip in hostIPs)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    hostIP = ip;
                    found = true;
                    break;
                }
            }
            if (! found)
            {
                form.dispQueue.Add("Could not find IPv4 address for " + host);
                return;
            }
            try
            {
                sckt.Connect(hostIP, 54011);

            }
            catch (SocketException se)
            {
                form.dispQueue.Add("Encountered socket exception. Detailed error: " + se.Message);
                return;
            }
            connected = true;
            form.dispQueue.Add("TCP connection established to host @" + hostIP.ToString());
            form.dispQueue.Add("Attempting to send username and passwd.");
            new Thread(new ThreadStart(SendLoop)).Start();
            new Thread(new ThreadStart(Listen)).Start();
            Send(user);
            while (waitSalt)
            {
                Thread.Sleep(100);
            }
            Send(Util.Hash(Util.Hash(pass) + salt));
        }

        public void Send(string str)
        {
            sendQueue.Add(str);
        }

        public void ForceSend(string str)
        {
            sckt.Send(Encoding.UTF8.GetBytes(str.Length.ToString() + "@" + str));
        }

        private void SendLoop()
        {
            try
            {
                while (form.running)
                {
                    if (connected && sendQueue.Count > 0)
                    {
                        sckt.Send(Encoding.UTF8.GetBytes(sendQueue[0].Length.ToString() + "@" + sendQueue[0]));
                        sendQueue.RemoveAt(0);
                    }
                    Thread.Sleep(50);
                }
            }
            catch (SocketException e)
            {
                form.dispQueue.Add("SocketException while sending: " + e.Message);
            }
        }

        private byte[] data;

        private void Listen()
        {
            if (! connected)
            {
                return;
            }
            try
            {
                sckt.BeginReceive(data, 0, data.Length, SocketFlags.None, new AsyncCallback(OnDataReceived), null);
            }
            catch (SocketException se)
            {
                form.dispQueue.Add("Encountered socket exception, disconnecting.");
                form.dispQueue.Add("Detailed error: " + se.Message);
                Disconnect();
                connected = false;
            }
        }

        private bool inString = false;
        private string lengthNums = "";
        private int msgLength = 0;
        private string conjMsg = "";

        private void OnDataReceived(IAsyncResult res)
        {
            //form.dispQueue.Add("Got " + Encoding.UTF8.GetString(data));
            if (inString)
            {
                msgLength--;
                conjMsg += Encoding.UTF8.GetString(data);
                if (msgLength == 0)
                {
                    if (conjMsg.StartsWith("salt:"))
                    {
                        string[] split = conjMsg.Split(':');
                        if (split.Length < 2)
                        {
                            Disconnect();
                            return;
                        }
                        salt = split[1];
                        waitSalt = false;
                    }
                    else
                    {
                        form.dispQueue.Add(hostIP.ToString() + ": " + conjMsg);
                    }
                    conjMsg = "";
                    lengthNums = "";
                    inString = false;
                }
            }
            else
            {
                if (Encoding.UTF8.GetString(data) == "#")
                {
                    form.dispQueue.Add("Received disconnection packet from server.");
                    Disconnect();
                    conjMsg = "";
                    lengthNums = "";
                    return;
                }
                if (Encoding.UTF8.GetString(data) == "@")
                {
                    if (! int.TryParse(lengthNums, out msgLength))
                    {
                        form.dispQueue.Add("Server sent bad packet, disconnecting.");
                        Disconnect();
                    }
                    inString = true;
                    return;
                }
                if (Encoding.UTF8.GetString(data) == "&")
                {
                    //keepalive-ignore
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
            catch (SocketException)
            {
                form.dispQueue.Add("Encountered socket exception while receiving data. Disconnecting.");
                return;
            }
            catch (ArgumentException)
            {
                return;
            }
            Listen();
        }

        public void Disconnect()
        {
            connected = false;
            sckt.Shutdown(SocketShutdown.Both);
            sckt.Disconnect(false);
            sckt.Close();
            sckt.Dispose();
            form.dispQueue.Add("Disconnected from server.");
            conjMsg = "";
            lengthNums = "";
            waitSalt = true;
            form.Disconnect();
        }
    }
}
