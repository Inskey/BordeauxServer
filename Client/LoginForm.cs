using System;
using System.Windows.Forms;

namespace BordeauxRCClient
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        internal string getLoginDetails()
        {
            return string.Format("::login {0} {1} {2}", textBox1.Text, textBox2.Text, textBox3.Text);
        }

        private bool isValidDetails()
        {
            return !string.IsNullOrWhiteSpace(textBox1.Text) && !string.IsNullOrWhiteSpace(textBox2.Text) && !string.IsNullOrWhiteSpace(textBox3.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (isValidDetails())
            {
                DialogResult = DialogResult.OK;
            }
        }
    }
}
