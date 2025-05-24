using System;
using System.Linq;

namespace EverlastingEditor.Base
{
    public class AutoIndentedTextWriter : IndentedTextWriter
    {
        public const char DEFAULT_START_ELEMENT = '{';
        public const char DEFAULT_END_ELEMENT = '}';

        private readonly char startElement;
        private readonly char endElement;

        public AutoIndentedTextWriter(char startElement, char endElement)
            : base()
        {
            this.startElement = startElement;
            this.endElement = endElement;
            NewLine = "\n";
        }

        public AutoIndentedTextWriter()
            : this(DEFAULT_START_ELEMENT, DEFAULT_END_ELEMENT)
        {
        }

        public override void Write(string value)
        {
            int indentCount = value.Count(ch => ch == startElement) - value.Count(ch => ch == endElement);
            if(indentCount < 0)
                Indent += indentCount;
            base.Write(value);
            if(indentCount > 0)
                Indent += indentCount;
        }

        public override void Write(string format, object arg0)
        {
            var value = string.Format(this.FormatProvider, format, arg0);
            int indentCount = value.Count(ch => ch == startElement) - value.Count(ch => ch == endElement);
            if(indentCount < 0)
                Indent += indentCount;
            base.Write(value);
            if(indentCount > 0)
                Indent += indentCount;
        }

        public override void Write(string format, object arg0, object arg1)
        {
            var value = string.Format(this.FormatProvider, format, arg1);
            int indentCount = value.Count(ch => ch == startElement) - value.Count(ch => ch == endElement);
            if(indentCount < 0)
                Indent += indentCount;
            base.Write(value);
            if(indentCount > 0)
                Indent += indentCount;
        }

        public override void Write(string format, params object[] arg)
        {
            var value = string.Format(this.FormatProvider, format, arg);
            int indentCount = value.Count(ch => ch == startElement) - value.Count(ch => ch == endElement);
            if(indentCount < 0)
                Indent += indentCount;
            base.Write(value);
            if(indentCount > 0)
                Indent += indentCount;
        }

        public override void WriteLine(string format, object arg0)
        {
            var value = string.Format(this.FormatProvider, format, arg0);
            int indentCount = value.Count(ch => ch == startElement) - value.Count(ch => ch == endElement);
            if(indentCount < 0)
                Indent += indentCount;
            base.WriteLine(value);
            if(indentCount > 0)
                Indent += indentCount;
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            var value = string.Format(this.FormatProvider, format, arg0, arg1);
            int indentCount = value.Count(ch => ch == startElement) - value.Count(ch => ch == endElement);
            if(indentCount < 0)
                Indent += indentCount;
            base.WriteLine(value);
            if(indentCount > 0)
                Indent += indentCount;
        }

        public override void WriteLine(string format, params object[] arg)
        {
            var value = string.Format(this.FormatProvider, format, arg);
            int indentCount = value.Count(ch => ch == startElement) - value.Count(ch => ch == endElement);
            if(indentCount < 0)
                Indent += indentCount;
            base.WriteLine(value);
            if(indentCount > 0)
                Indent += indentCount;
        }

        public override void WriteLine(string value)
        {
            int indentCount = value.Count(ch => ch == startElement) - value.Count(ch => ch == endElement);
            if(indentCount < 0)
                Indent += indentCount;
            base.WriteLine(value);
            if(indentCount > 0)
                Indent += indentCount;
        }

        public override void WriteLine(char[] buffer)
        {
            int indentCount = buffer.Count(ch => ch == startElement) - buffer.Count(ch => ch == endElement);
            if(indentCount < 0)
                Indent += indentCount;
            base.WriteLine(buffer);
            if(indentCount > 0)
                Indent += indentCount;
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            var segment = new ArraySegment<char>(buffer, index, count);
            int indentCount = segment.Count(ch => ch == startElement) - segment.Count(ch => ch == endElement);
            if(indentCount < 0)
                Indent += indentCount;
            base.WriteLine((object)segment);
            if(indentCount > 0)
                Indent += indentCount;
        }

        public override void Write(char[] buffer)
        {
            int indentCount = buffer.Count(ch => ch == startElement) - buffer.Count(ch => ch == endElement);
            if(indentCount < 0)
                Indent += indentCount;
            base.Write(buffer);
            if(indentCount > 0)
                Indent += indentCount;
        }

        public override void Write(char[] buffer, int index, int count)
        {
            var segment = new ArraySegment<char>(buffer, index, count);
            int indentCount = segment.Count(ch => ch == startElement) - segment.Count(ch => ch == endElement);
            if(indentCount < 0)
                Indent += indentCount;
            base.Write((object)segment);
            if(indentCount > 0)
                Indent += indentCount;
        }
    }
}