using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Globalization;
using SSCP.Telem.Can;

namespace SSCP.Telem.CanShark {
    public partial class SendMessageForm : Form
    {
        private IntTextBoxParser parserDevid, parserMsgid;
        private IntTextBoxParser parserIde, parserRtr;
        private HexBytesTextBoxParser parserData;

        public CanMessage CanMessage { get; private set; }
        public SendMessageForm()
        {
            InitializeComponent();
            InitParsers();
        }

        private void InitParsers()
        {
            parserDevid = new IntTextBoxParser(textBoxDevid);
            parserMsgid = new IntTextBoxParser(textBoxMsgid);
            parserIde = new IntTextBoxParser(textBoxIde);
            parserRtr = new IntTextBoxParser(textBoxRtr);
            parserData = new HexBytesTextBoxParser(textBoxData);
            textBoxData.Text = "0x";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (CheckValid())
            {
                //send message
                var msg = new CanMessage();
                msg.data = Util.BytesToUlong(parserData.Value);
                msg.id = parserDevid.Value << 6 | parserMsgid.Value;
                msg.ide = parserIde.Value > 0;
                msg.dlc = (byte)parserData.Value.Length;
                msg.rtr = parserRtr.Value > 0;
                CanMessage = msg;
                
                //close, return "OK"
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show(this, "Fix invalid fields before sending...");
            }
        }

        private bool CheckValid()
        {
            return
                parserData.IsValid &&
                parserDevid.IsValid &&
                parserMsgid.IsValid &&
                parserIde.IsValid &&
                parserRtr.IsValid;
        }

        private void textBoxData_TextChanged(object sender, EventArgs e)
        {
            if (parserData.IsValid)
            {
                textBoxDlc.Text = ((byte[])parserData.Value).Length.ToString();
            }
        }
    }
}
