using System.Collections.Generic;

namespace Localization.Reader
{
    public interface IReader
    {
        List<List<string>> Parse(string src);
    }
}
