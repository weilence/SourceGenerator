using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using SourceGenerator.Library.Template;

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

            var rootElement = JsonDocument.Parse(appSettingsFile).RootElement;
            var columns = new List<Column>();
            foreach (var jsonProperty in rootElement.EnumerateObject())
            {
                var value = jsonProperty.Value.GetString();
                columns.Add(new Column()
                {
                    Name = jsonProperty.Name,
                    Value = value,
                    Type = JsonUtils.GetType(jsonProperty.Value.ValueKind, value)
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