using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSharpener
{
    internal class TextBuilder
    {
        private int indent = 0;

        public TextBuilder()
        {
            this.StringBuilder = new StringBuilder();
        }

        public StringBuilder StringBuilder { get; set; }

        public void Indent(int times = 1)
        {
            indent = indent + times;
        }

        public void Unindent()
        {
            if (indent > 0) indent--;
        }

        public void ClearIndent()
        {
            indent = 0;
        }

        public TextBuilder Append(string s)
        {
            this.AppendIndent();
            this.StringBuilder.Append(s);
            return this;
        }

        public TextBuilder AppendFormat(string format, string s)
        {
            this.AppendIndent();
            this.StringBuilder.AppendFormat(format, s);
            return this;
        }

        public TextBuilder AppendFormat(string format, string s1, string s2)
        {
            this.AppendIndent();
            this.StringBuilder.AppendFormat(format, s1, s2);
            return this;
        }

        public TextBuilder AppendFormat(string format, string s1, string s2, string s3)
        {
            this.AppendIndent();
            this.StringBuilder.AppendFormat(format, s1, s2, s3);
            return this;
        }

        public TextBuilder AppendFormat(string format, string s1, string s2, string s3, string s4)
        {
            this.AppendIndent();
            this.StringBuilder.AppendFormat(format, s1, s2, s3, s4);
            return this;
        }

        public TextBuilder AppendFormatLine(string format, string s)
        {
            this.AppendFormat(format, s);
            this.StringBuilder.AppendLine();
            return this;
        }

        public TextBuilder AppendFormatLine(string format, string s1, string s2)
        {
            this.AppendFormat(format, s1, s2);
            this.StringBuilder.AppendLine();
            return this;
        }

        public TextBuilder AppendFormatLine(string format, string s1, string s2, string s3)
        {
            this.AppendFormat(format, s1, s2, s3);
            this.StringBuilder.AppendLine();
            return this;
        }

        public TextBuilder AppendFormatLine(string format, string s1, string s2, string s3, string s4)
        {
            this.AppendFormat(format, s1, s2, s3, s4);
            this.StringBuilder.AppendLine();
            return this;
        }

        public TextBuilder AppendLine()
        {
            this.AppendIndent();
            this.StringBuilder.AppendLine();
            return this;
        }

        public TextBuilder AppendLine(string s)
        {
            this.AppendIndent();
            this.StringBuilder.AppendLine(s);
            return this;
        }

        public override string ToString()
        {
            return this.StringBuilder.ToString();
        }

        private void AppendIndent()
        {
            if (indent > 0)
            {
                var tabs = new String('\t', indent);
                this.StringBuilder.Append(tabs);
            }
        }
    }
}
