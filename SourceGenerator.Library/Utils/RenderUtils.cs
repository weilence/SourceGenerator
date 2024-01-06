using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Scriban;
using Scriban.Runtime;

namespace SourceGenerator.Library.Utils
{
    public class RenderUtils
    {
        private static readonly Dictionary<string, Template> Dictionary = new Dictionary<string, Template>();

        private const string ClassNamespace = "SourceGenerator.Library.Templates";
        private const string TemplateExtension = "scriban";

        private static string ReadFile(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                var content = reader.ReadToEnd();
                return content;
            }
        }

        public static string Render(string key, object model)
        {
            if (!Dictionary.TryGetValue(key, out var template))
            {
                var resourceName = string.Join(".", ClassNamespace, key, TemplateExtension);
                var text = ReadFile(resourceName);
                template = Template.Parse(text);
                Dictionary[key] = template;
            }

            var scriptObject = new ScriptObject();
            scriptObject.Import(typeof(StringUtils));
            scriptObject["Model"] = model;

            var context = new TemplateContext();
            context.PushGlobal(scriptObject);
            context.MemberRenamer = member => member.Name;

            var result = template.Render(context);
            return result;
        }
    }
}