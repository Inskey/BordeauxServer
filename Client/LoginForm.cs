using System;
using System.Windows.Forms;

namespace BordeauxRCClient
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            Shown += LoginForm_Shown;
        }

        private void LoginForm_Shown(object sender, EventArgs e)
        {
            textBox_Host.Text = Properties.Settings.Default.Host;
            textBox_User.Text = Properties.Settings.Default.User;
            textBox_Pass.Text = Properties.Settings.Default.Pass;
        }

        internal string getLoginDetails()
        {
            return string.Format("::login {0} {1} {2}", textBox_Host.Text, textBox_User.Text, textBox_Pass.Text);
        }

        private bool isValidDetails()
        {
            return !string.IsNullOrWhiteSpace(textBox_Host.Text) && !string.IsNullOrWhiteSpace(textBox_User.Text) && !string.IsNullOrWhiteSpace(textBox_Pass.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (isValidDetails())
            {
                if (checkBox_RememberMe.Checked)
                {
                    Properties.Settings.Default.Host = textBox_Host.Text;
                    Properties.Settings.Default.User = textBox_User.Text;
                    if (checkBox_RememberPass.Checked) Properties.Settings.Default.Pass = textBox_Pass.Text;

                    Properties.Settings.Default.Save();
                }
                DialogResult = DialogResult.OK;
            }
        }
    }
}
