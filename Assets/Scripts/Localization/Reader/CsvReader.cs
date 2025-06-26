using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Localization.Reader
{
    public class CsvReader : Singleton<CsvReader>, IReader
    {
        private enum ParsingMode
        {
            None,
            OutQuote,
            InQuote
        }
        
        public List<List<string>> Parse(string src)
        {
            var rows = new List<List<string>>();
            var cols = new List<string>();
            var buffer = new StringBuilder();

            ParsingMode mode = ParsingMode.OutQuote;
            bool requireTrimLineHead = false;
            var isBlank = new Regex(@"\s");

            int len = src.Length;

            for (int i = 0; i < len; ++i)
            {

                char c = src[i];

                // remove whitespace at beginning of line
                if (requireTrimLineHead)
                {
                    if (isBlank.IsMatch(c.ToString()))
                    {
                        continue;
                    }

                    requireTrimLineHead = false;
                }

                // finalize when c is the last character
                if ((i + 1) == len)
                {
                    // final char
                    switch (mode)
                    {
                        case ParsingMode.InQuote:
                            if (c == '"')
                            {
                                // ignore
                            }
                            else
                            {
                                // if close quote is missing
                                buffer.Append(c);
                            }

                            cols.Add(buffer.ToString());
                            rows.Add(cols);
                            return rows;

                        case ParsingMode.OutQuote:
                            if (c == ',')
                            {
                                // if the final character is comma, add an empty cell
                                // next col
                                cols.Add(buffer.ToString());
                                cols.Add(string.Empty);
                                rows.Add(cols);
                                return rows;
                            }

                            // if the final line is empty, ignore it. 
                            if (cols.Count == 0 && string.Empty.Equals(c.ToString().Trim()))
                            {
                                return rows;
                            }

                            buffer.Append(c);
                            cols.Add(buffer.ToString());
                            rows.Add(cols);
                            return rows;
                    }
                }

                // the next character
                char n = src[i + 1];

                switch (mode)
                {
                    case ParsingMode.OutQuote:
                        // out quote
                        if (c == '"')
                        {
                            // to in-quote
                            mode = ParsingMode.InQuote;
                            continue;
                        }
                        else if (c == ',')
                        {
                            // next cell
                            cols.Add(buffer.ToString());
                            buffer.Remove(0, buffer.Length);
                        }
                        else if (c == '\r' && n == '\n')
                        {
                            // new line(CR+LF)
                            cols.Add(buffer.ToString());
                            rows.Add(cols);
                            cols = new List<string>();
                            buffer.Remove(0, buffer.Length);
                            ++i; // skip next code
                            requireTrimLineHead = true;
                        }
                        else if (c == '\n' || c == '\r')
                        {
                            // new line
                            cols.Add(buffer.ToString());
                            rows.Add(cols);
                            cols = new List<string>();
                            buffer.Remove(0, buffer.Length);
                            requireTrimLineHead = true;
                        }
                        else
                        {
                            // get one char
                            buffer.Append(c);
                        }
                        break;

                    case ParsingMode.InQuote:
                        // in quote
                        if (c == '"' && n != '"')
                        {
                            // to out-quote
                            mode = ParsingMode.OutQuote;
                        }
                        else if (c == '"' && n == '"')
                        {
                            // get "
                            buffer.Append('"');
                            ++i;
                        }
                        else
                        {
                            // get one char
                            buffer.Append(c);
                        }
                        break;
                }
            }
            return rows;
        }
    }
}