namespace BordeauxRCClient
{
    partial class LoginForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBox_Host = new System.Windows.Forms.TextBox();
            this.textBox_User = new System.Windows.Forms.TextBox();
            this.textBox_Pass = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_Connect = new System.Windows.Forms.Button();
            this.checkBox_RememberMe = new System.Windows.Forms.CheckBox();
            this.checkBox_RememberPass = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox_Host.Location = new System.Drawing.Point(62, 12);
            this.textBox_Host.Name = "textBox1";
            this.textBox_Host.Size = new System.Drawing.Size(278, 20);
            this.textBox_Host.TabIndex = 0;
            // 
            // textBox2
            // 
            this.textBox_User.Location = new System.Drawing.Point(62, 38);
            this.textBox_User.Name = "textBox2";
            this.textBox_User.Size = new System.Drawing.Size(278, 20);
            this.textBox_User.TabIndex = 1;
            // 
            // textBox3
            // 
            this.textBox_Pass.Location = new System.Drawing.Point(62, 64);
            this.textBox_Pass.Name = "textBox3";
            this.textBox_Pass.PasswordChar = '●';
            this.textBox_Pass.Size = new System.Drawing.Size(278, 20);
            this.textBox_Pass.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Host";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "User";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Pass";
            // 
            // button1
            // 
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(12, 95);
            this.button_Cancel.Name = "button1";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 6;
            this.button_Cancel.Text = "Cancel";
            this.button_Cancel.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button_Connect.Location = new System.Drawing.Point(265, 95);
            this.button_Connect.Name = "button2";
            this.button_Connect.Size = new System.Drawing.Size(75, 23);
            this.button_Connect.TabIndex = 7;
            this.button_Connect.Text = "Connect";
            this.button_Connect.UseVisualStyleBackColor = true;
            this.button_Connect.Click += new System.EventHandler(this.button2_Click);
            // 
            // checkBox1
            // 
            this.checkBox_RememberMe.AutoSize = true;
            this.checkBox_RememberMe.Location = new System.Drawing.Point(103, 90);
            this.checkBox_RememberMe.Name = "checkBox1";
            this.checkBox_RememberMe.Size = new System.Drawing.Size(129, 17);
            this.checkBox_RememberMe.TabIndex = 8;
            this.checkBox_RememberMe.Text = "Remember My Details";
            this.checkBox_RememberMe.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox_RememberPass.AutoSize = true;
            this.checkBox_RememberPass.Location = new System.Drawing.Point(103, 109);
            this.checkBox_RememberPass.Name = "checkBox2";
            this.checkBox_RememberPass.Size = new System.Drawing.Size(153, 17);
            this.checkBox_RememberPass.TabIndex = 9;
            this.checkBox_RememberPass.Text = "Remember Pass (Insecure)";
            this.checkBox_RememberPass.UseVisualStyleBackColor = true;
            // 
            // LoginForm
            // 
            this.AcceptButton = this.button_Connect;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(352, 130);
            this.Controls.Add(this.checkBox_RememberPass);
            this.Controls.Add(this.checkBox_RememberMe);
            this.Controls.Add(this.button_Connect);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_Pass);
            this.Controls.Add(this.textBox_User);
            this.Controls.Add(this.textBox_Host);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginForm";
            this.Text = "LoginForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_Host;
        private System.Windows.Forms.TextBox textBox_User;
        private System.Windows.Forms.TextBox textBox_Pass;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_Connect;
        private System.Windows.Forms.CheckBox checkBox_RememberMe;
        private System.Windows.Forms.CheckBox checkBox_RememberPass;
    }
}