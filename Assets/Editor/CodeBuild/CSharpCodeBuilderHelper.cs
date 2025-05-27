using System;
using System.IO;
using System.Text;

namespace CodeBuild
{
    public static class CSharpCodeBuilderHelper
    {
        public static string CombineFunctionCallInvoke(string funcName, string[] paramNames)
        {
            StringBuilder sb = new StringBuilder(50);
            sb.Append(funcName);
            sb.Append('(');
            for (var i = 0; i < paramNames.Length; i++)
            {
                if(i > 0) sb.Append(", ");
                sb.Append(paramNames[i]);
            }
            sb.Append(");");
            return sb.ToString();
        }

        //考虑Keyword的类型转换
        public static string GetTypeNameKeyword(Type paramType)
        {
            if (paramType == typeof(UInt64)) return "ulong";
            else if (paramType == typeof(Int64)) return "long";
            else if (paramType == typeof(Int32)) return "int";
            else if (paramType == typeof(UInt32)) return "uint";
            else if (paramType == typeof(String)) return "string";
            else if (paramType == typeof(Boolean)) return "bool";
            else if (paramType == typeof(void)) return "void";
            else if (paramType == typeof(Single)) return "float";
            else if (paramType == typeof(Double)) return "double";
            return paramType.Name;
        }

        //首字母改为大写
        public static string ToUpperFirstChar(string source)
        {
            if (!Char.IsUpper(source[0]))
            {
                return $"{Char.ToUpper(source[0])}{source.Substring(1)}";
            }

            return source;
        }
    }
}