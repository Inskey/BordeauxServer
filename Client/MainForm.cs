using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace BordeauxRCClient
{
    public partial class MainForm : Form
    {
        public bool running;

        private Core.Connection con;
        private LoginForm loginForm;

        public MainForm()
        {
            InitializeComponent();
            running = true;
            con = new Core.Connection(this);
            new Thread(new ThreadStart(DisplayLoop)).Start();
            new Thread(new ThreadStart(CmdLoop)).Start();

            openLoginForm();
        }

        public List<string> dispQueue = new List<string>();

        public void DisplayLoop()
        {
            while (running)
            {
                Thread.Sleep((dispQueue.Count > 10) ? 25 : 50);
                if (dispQueue.Count > 0)
                {
                    SafeAppend("[" + DateTime.UtcNow.ToString("hh:mm:ss") + "] " + dispQueue[0]);
                    dispQueue.RemoveAt(0);
                }
            }
        }

        private delegate void SafeAppendCallback(string msg);

        private void SafeAppend(string msg)
        {
            if (!running) { return; }
            try
            {
                if (textBox1.InvokeRequired)
                {
                    SafeAppendCallback d = new SafeAppendCallback(SafeAppend);
                    Invoke(d, new object[] { msg });
                }
                else
                {
                    textBox1.AppendText(msg + "\r\n");
                }
            }
            catch (ObjectDisposedException)
            {
                return;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cmds.Add(textBox2.Text);
            while (oldCommands.Count > 24)
            {
                oldCommands.RemoveAt(24);
            }
            if (!(oldCommands.Count > 0 && oldCommands[0] == textBox2.Text))
            {
                oldCommands.Insert(0, textBox2.Text);
            }
            cmdIndex = -1;
            textBox2.Clear();
        }

        public List<string> cmds = new List<string>();

        private void CmdLoop()
        {
            while (running)
            {
                Thread.Sleep(500);
                if (cmds.Count > 0)
                {
                    string cmd = cmds[0];
                    if (cmd == "") { continue; }
                    if (cmd[0] == ':' && cmd[1] == ':')
                    {
                        cmd = cmd.Remove(0, 2);
                        string[] cmdParts = cmd.Split(new char[1] { ' ' });
                        switch (cmdParts[0])
                        {
                            case "login":
                                if (cmdParts.Length < 4)
                                {
                                    dispQueue.Add("[login] Error: Not enough arguments. Usage is ::login <ip> <username> <password>");
                                    break;
                                }
                                con.Connect(cmdParts[1], cmdParts[2], cmdParts[3]);
                                break;

                            case "forcedisconnect":
                                if (con.connected)
                                {
                                    con.Disconnect();
                                }
                                else
                                {
                                    dispQueue.Add("[forcedisconnect] Error: Not connected to a server.");
                                }
                                break;

                            case "help":
                                dispQueue.Add("-=Command Help=-\r\n" +
                                    "login <IP> <username> <passwd>: Try connecting and logging in to a server.\r\n" +
                                    "forcedisconnect: Forcibly disconnect from the server.\r\n" +
                                    "version: Display version.\r\n" +
                                    "exit: Exit the program safely.");
                                break;

                            case "version":
                                dispQueue.Add("Client version: " + Program.version);
                                break;

                            case "exit":
                                running = false;
                                if (con.connected)
                                {
                                    dispQueue.Add("Disconnecting from server...");
                                    con.ForceSend(":disconnect");
                                    con.Disconnect();
                                }
                                Environment.Exit(0);
                                return;

                            default:
                                dispQueue.Add("Error: Unknown command \"" + cmdParts[0] + "\". For a list of commands, use ::help.");
                                break;
                        }
                    }
                    else
                    {
                        if (con.connected)
                        {
                            con.Send(cmd);
                        }
                        else
                        {
                            dispQueue.Add("Error: Not connected to a server.");
                        }
                    }
                    cmds.RemoveAt(0);
                }
            }
        }

        private void openLoginForm()
        {
            if (loginForm == null || loginForm.IsDisposed) loginForm = new LoginForm();

            DialogResult res = loginForm.ShowDialog();

            if (res == DialogResult.OK)
            {
                cmds.Add(loginForm.getLoginDetails());
            }
        }

        private List<string> oldCommands = new List<string>();
        private int cmdIndex = -1;

        private void textBox2_KeyPress(object sender, PreviewKeyDownEventArgs e)
        {
            if (textBox2.Focused)
            {
                if (e.KeyData == Keys.Up)
                {
                    OnUpArrow();
                }
                else if (e.KeyData == Keys.Down)
                {
                    OnDownArrow();
                }
            }
        }

        private void OnUpArrow()
        {
            if (cmdIndex == 24 || cmdIndex == oldCommands.Count - 1) { return; }
            cmdIndex++;
            textBox2.Text = oldCommands[cmdIndex];
        }

        private void OnDownArrow()
        {
            if (cmdIndex == -1) { return; }
            cmdIndex--;
            if (cmdIndex == -1)
            {
                textBox2.Clear();
            }
            else
            {
                textBox2.Text = oldCommands[cmdIndex];
            }
        }

        private void OnFormClose(object sender, FormClosingEventArgs e)
        {
            if (con.connected) con.Disconnect();
            running = false;
        }

        internal void Disconnect()
        {
            con.Disconnect();
            con = new Core.Connection(this);
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openLoginForm();
        }
    }
}