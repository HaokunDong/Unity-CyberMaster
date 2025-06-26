using System.Collections.Generic;

namespace Localization.Reader
{
    public class TxtReader : Singleton<TxtReader>, IReader
    {
        public List<List<string>> Parse(string src)
        {
            var rows = new List<List<string>>();
            var lines = src.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var cols = new List<string>(line.Split('\t'));
                rows.Add(cols);
            }

            return rows;
        }
    }
}