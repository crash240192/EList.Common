using System.Collections.Generic;

namespace EList.Common.TemplateParser
{
    public class TemplateParser : ITemplateParser
    {
        public string Parse(string template, IDictionary<string, string> tokens)
        {
            var result = template;

            foreach (var token in tokens)
                result = result.Replace(token.Key, token.Value ?? string.Empty);

            return result;
        }
    }
}
