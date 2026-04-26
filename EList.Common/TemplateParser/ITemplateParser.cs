using System.Collections.Generic;

namespace EList.Common.TemplateParser
{
    public interface ITemplateParser
    {
        string Parse(string template, IDictionary<string, string> tokens);
    }
}
