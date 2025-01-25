using System;
using System.IO;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        string dir = Directory.GetCurrentDirectory();
        foreach (var f in new DirectoryInfo(dir).GetFiles("*.cs", SearchOption.AllDirectories))
        {
            string content = File.ReadAllText(f.FullName, Encoding.Default);
            File.WriteAllText(f.FullName, content, Encoding.UTF8);
        }
    }
}