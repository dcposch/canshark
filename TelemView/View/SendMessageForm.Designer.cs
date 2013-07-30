namespace SSCP.Telem.CanShark {
    partial class SendMessageForm
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
            this.textBoxDevid = new System.Windows.Forms.TextBox();
            this.textBoxMsgid = new System.Windows.Forms.TextBox();
            this.textBoxData = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxRtr = new System.Windows.Forms.TextBox();
            this.textBoxIde = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox9 = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.textBox11 = new System.Windows.Forms.TextBox();
            this.textBox12 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.textBoxDlc = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBoxDevid
            // 
            this.textBoxDevid.Location = new System.Drawing.Point(109, 48);
            this.textBoxDevid.Name = "textBoxDevid";
            this.textBoxDevid.Size = new System.Drawing.Size(64, 20);
            this.textBoxDevid.TabIndex = 0;
            this.textBoxDevid.Text = "0x0";
            // 
            // textBoxMsgid
            // 
            this.textBoxMsgid.Location = new System.Drawing.Point(179, 48);
            this.textBoxMsgid.Name = "textBoxMsgid";
            this.textBoxMsgid.Size = new System.Drawing.Size(64, 20);
            this.textBoxMsgid.TabIndex = 1;
            this.textBoxMsgid.Text = "0x0";
            // 
            // textBoxData
            // 
            this.textBoxData.Location = new System.Drawing.Point(109, 100);
            this.textBoxData.Name = "textBoxData";
            this.textBoxData.Size = new System.Drawing.Size(210, 20);
            this.textBoxData.TabIndex = 2;
            this.textBoxData.Text = "0x00";
            this.textBoxData.TextChanged += new System.EventHandler(this.textBoxData_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 22);
            this.label2.TabIndex = 7;
            this.label2.Text = "Message";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "devid / msgid:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(72, 103);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "data:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(34, 77);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "ide / rtr / dlc:";
            // 
            // textBoxRtr
            // 
            this.textBoxRtr.Location = new System.Drawing.Point(179, 74);
            this.textBoxRtr.Name = "textBoxRtr";
            this.textBoxRtr.Size = new System.Drawing.Size(64, 20);
            this.textBoxRtr.TabIndex = 11;
            this.textBoxRtr.Text = "0";
            // 
            // textBoxIde
            // 
            this.textBoxIde.Enabled = false;
            this.textBoxIde.Location = new System.Drawing.Point(109, 74);
            this.textBoxIde.Name = "textBoxIde";
            this.textBoxIde.Size = new System.Drawing.Size(64, 20);
            this.textBoxIde.TabIndex = 10;
            this.textBoxIde.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(49, 214);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 22;
            this.label5.Text = "interface:";
            // 
            // textBox9
            // 
            this.textBox9.Enabled = false;
            this.textBox9.Location = new System.Drawing.Point(109, 211);
            this.textBox9.Name = "textBox9";
            this.textBox9.Size = new System.Drawing.Size(64, 20);
            this.textBox9.TabIndex = 20;
            this.textBox9.Text = "0";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(53, 188);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(47, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "ip / port:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(12, 146);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(104, 22);
            this.label8.TabIndex = 17;
            this.label8.Text = "Destination";
            // 
            // textBox11
            // 
            this.textBox11.Enabled = false;
            this.textBox11.Location = new System.Drawing.Point(179, 185);
            this.textBox11.Name = "textBox11";
            this.textBox11.Size = new System.Drawing.Size(64, 20);
            this.textBox11.TabIndex = 15;
            this.textBox11.Text = "0x0";
            // 
            // textBox12
            // 
            this.textBox12.Enabled = false;
            this.textBox12.Location = new System.Drawing.Point(109, 185);
            this.textBox12.Name = "textBox12";
            this.textBox12.Size = new System.Drawing.Size(64, 20);
            this.textBox12.TabIndex = 14;
            this.textBox12.Text = "0x0";
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(12, 259);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(104, 31);
            this.button1.TabIndex = 24;
            this.button1.Text = "Send";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(144, 259);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(104, 31);
            this.button2.TabIndex = 25;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // textBoxDlc
            // 
            this.textBoxDlc.Enabled = false;
            this.textBoxDlc.Location = new System.Drawing.Point(249, 74);
            this.textBoxDlc.Name = "textBoxDlc";
            this.textBoxDlc.Size = new System.Drawing.Size(64, 20);
            this.textBoxDlc.TabIndex = 13;
            this.textBoxDlc.Text = "0";
            // 
            // SendMessageForm
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button2;
            this.ClientSize = new System.Drawing.Size(413, 332);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox9);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.textBox11);
            this.Controls.Add(this.textBox12);
            this.Controls.Add(this.textBoxDlc);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxRtr);
            this.Controls.Add(this.textBoxIde);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxData);
            this.Controls.Add(this.textBoxMsgid);
            this.Controls.Add(this.textBoxDevid);
            this.Name = "SendMessageForm";
            this.Text = "SendMessageForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxDevid;
        private System.Windows.Forms.TextBox textBoxMsgid;
        private System.Windows.Forms.TextBox textBoxData;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxRtr;
        private System.Windows.Forms.TextBox textBoxIde;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox9;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBox11;
        private System.Windows.Forms.TextBox textBox12;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBoxDlc;
    }
}