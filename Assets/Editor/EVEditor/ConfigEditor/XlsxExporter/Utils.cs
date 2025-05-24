using System;
using System.Collections.Generic;
using System.Text;

namespace EverlastingEditor.Config
{
    public static class Utils
    {        
        //
        // ColNum转Label
        //
        public static string ColumnNum2Label(int v0)
        {
            if (v0 <= 0) return string.Empty;

            List<char> buf = new List<char>();

            var v = v0 - 1;

            // 末尾是26个字符
            buf.Add((char)('A' + v % 26));
            v /= 26;

            // 上面的实际是27进位
            // v = 0 => Nil
            // v = 1 => A
            // v = 26 => Z
            // v = 27 => AA 这个时候呢就挂了
            while (v > 0)
            {
                buf.Add((char)('A' + (v - 1) % 26));
                v /= 27;
            }

            buf.Reverse();
            return new string(buf.ToArray());
        }
        
        public static int Label2ColumnNum(string label)
        {
            if (string.IsNullOrEmpty(label))
            {
                return 0;
            }

            var colNum = 0;
            foreach (var c in label)
            {
                if (!char.IsLetter(c))
                {
                    colNum = 0;
                    break;
                }
                
                colNum = colNum * 26 + char.ToLower(c) - 'a' + 1;
            }
            
            return colNum;
        }

        public static ConfigTypes ParseConfigTypes(string input)
        {
            ConfigTypes types = ConfigTypes.None;
            if (input.Contains("c"))
                types |= ConfigTypes.Client;
            if (input.Contains("s"))
                types |= ConfigTypes.Server;
            if (types == ConfigTypes.None)
                types = ConfigTypes.Both;
                //throw new Exception($"ConfigType格式不正确，{input}");
            return types;
        }
    }
    
}
