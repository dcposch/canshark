using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Globalization;

namespace SSCP.Telem.CanShark {
    public abstract class TextBoxParser<T>
    {
        private TextBox textBox;
        public TextBox TextBox
        {
            get
            {
                return textBox;
            }
        }
        public TextBoxParser(TextBox tbx)
        {
            textBox = tbx;
            tbx.TextChanged += new EventHandler((sender, e) => Refresh());
            Refresh();
        }
        protected void Refresh()
        {
            if (IsValid)
            {
                textBox.BackColor = Color.LightGreen;
            }
            else
            {
                textBox.BackColor = Color.Pink;
            }
        }
        protected abstract bool ValidateString(String str);
        protected virtual bool ValidateVal(T val)
        {
            return true;
        }
        protected abstract T Parse(String str);
        public bool IsValid
        {
            get
            {
                return ValidateString(TextBox.Text) && ValidateVal(Parse(TextBox.Text));
            }
        }

        public T Value
        {
            get
            {
                Debug.Assert(ValidateString(TextBox.Text));
                T ret = Parse(TextBox.Text);
                Debug.Assert(ValidateVal(ret));
                return ret;
            }
        }
    }
    public abstract class RegexTextBoxParser<T> : TextBoxParser<T>
    {
        private String expr = "";
        public String Expression
        {
            get
            {
                return expr;
            }
            set
            {
                Debug.Assert(value != null);
                expr = value;
                Refresh();
            }
        }
        public RegexTextBoxParser(TextBox tbx) : base(tbx) { }
        protected override bool ValidateString(String str)
        {
            var expr= "^("+this.expr+")$";
            var ret = Regex.IsMatch(str, expr);
            Debug.WriteLine((ret ? "match" : "no match") + " :" + expr + "\t" + str);
            return ret;
        }
    }
    public class IntTextBoxParser : RegexTextBoxParser<int>
    {
        public IntTextBoxParser(TextBox tbx)
            : base(tbx)
        {
            Expression = "-?((0x[0-9a-fA-F]+)|[0-9]+)";
        }
        protected override int Parse(String str)
        {
            if (str.StartsWith("0x"))
            {
                return int.Parse(str.Substring(2), NumberStyles.AllowHexSpecifier);
            }
            else
            {
                return int.Parse(str);
            }
        }
    }
    public class HexBytesTextBoxParser : RegexTextBoxParser<byte[]>
    {
        public HexBytesTextBoxParser(TextBox tbx)
            : base(tbx)
        {
            Expression = "()|0x([a-fA-F0-9][a-fA-F0-9])+";
        }
        protected override byte[] Parse(String txt)
        {
            if (txt.Length == 0)
            {
                return new byte[0];
            }
            var ret = new byte[(txt.Length - 2) / 2];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = byte.Parse(txt.Substring(2 * i + 2, 2), NumberStyles.AllowHexSpecifier);
            }
            return ret;
        }
    }
}
