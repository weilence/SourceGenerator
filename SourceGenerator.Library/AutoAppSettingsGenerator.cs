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
            var classInfo = JsonUtils.ParseJson(rootElement);

            classInfo.Name = "AppSettings";
            var appSettings = new AppSettingsModel()
            {
                Namespace = context.Compilation.AssemblyName + ".Configuration",
                Class = classInfo,
            };
            context.AddSource("AppSettings.g.cs", RenderUtils.Render("AppSettings", appSettings));
        }
    }
}