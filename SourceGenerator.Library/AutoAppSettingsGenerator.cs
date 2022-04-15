using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace SourceGenerator.Library
{
    [Generator]
    public class AutoAppSettingsGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.projectdir",
                    out var projectDir))
            {
                return;
            }

            var path = Path.Combine(projectDir, "appsettings.json");

            if (!File.Exists(path))
            {
                return;
            }

            var appSettingsFile = File.ReadAllText(path);
            var rootElement = JObject.Parse(appSettingsFile);
            var columns = new List<Column>();
            foreach (var jsonProperty in rootElement)
            {
                columns.Add(new Column()
                {
                    Name = jsonProperty.Key,
                    Value = jsonProperty.Value?.ToString(),
                    Type = jsonProperty.Value?.Type,
                });
            }

            var appSettings = new AppSettings(new AppSettingsModel()
            {
                Namespace = context.Compilation.AssemblyName,
                Columns = columns,
            });
            context.AddSource("AppSettings.g.cs", appSettings.TransformText());
        }
    }
}