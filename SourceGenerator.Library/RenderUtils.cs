using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Scriban;
using Scriban.Runtime;

namespace SourceGenerator.Library
{
    public class PreprocessTemplateContext
    {
        public string Language { get; set; }

        public string[] References { get; set; }

        public string OutputContent { get; set; }
        public string ClassName { get; set; }
    }

    public class RenderUtils
    {
        private const string ClassNamespace = "SourceGenerator.Library.Template";
        private const string TemplateExtension = "scriban";

        // public static PreprocessTemplateContext Preprocess(string key)
        // {
        //     var resourceName = string.Join(".", ClassNamespace, key, TemplateExtension);
        //     var template = ReadFile(resourceName);
        //
        //     var className = key;
        //     var generator = new TemplateGenerator();
        //     generator.PreprocessTemplate(resourceName, className, ClassNamespace, template,
        //         out var language, out var references, out var outputContent);
        //
        //     return new PreprocessTemplateContext()
        //     {
        //         ClassName = className,
        //         Language = language,
        //         References = references,
        //         OutputContent = outputContent
        //     };
        // }

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
            var resourceName = string.Join(".", ClassNamespace, key, TemplateExtension);
            var text = ReadFile(resourceName);
            var template = Scriban.Template.Parse(text);

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