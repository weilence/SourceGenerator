using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SourceGenerator.Library.Models;
using SourceGenerator.Library.Utils;

namespace SourceGenerator.Library.Generators
{
    [Generator]
    public class AutoArgsGenerator : IIncrementalGenerator
    {
        private const string ArgsAttributeName = "SourceGenerator.Common.ArgsAttribute";
        private const string LoggerAttributeName = "SourceGenerator.Common.LoggerAttribute";
        private const string IgnoreAttributeName = "SourceGenerator.Common.IgnoreAttribute";
        private const string KeyAttributeName = "SourceGenerator.Common.KeyAttribute";
        private const string PostConstructAttributeName = "SourceGenerator.Common.PostConstructAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            RegisterAttributes(context);

            var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
                ArgsAttributeName,
                static (_, _) => true,
                Transform
            );

            context.RegisterSourceOutput(pipeline, Output);
        }

        private static void RegisterAttributes(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(static postInitializationContext =>
            {
                postInitializationContext.AddSource("ArgsAttribute.cs", SourceText.From("""
                    using System;

                    namespace SourceGenerator.Common
                    {
                        [AttributeUsage(AttributeTargets.Class)]
                        public class ArgsAttribute : Attribute
                        {
                        }
                    }
                    """, Encoding.UTF8));
                postInitializationContext.AddSource("IgnoreAttribute.cs", SourceText.From("""
                    using System;

                    namespace SourceGenerator.Common
                    {
                        [AttributeUsage(AttributeTargets.Field)]
                        public class IgnoreAttribute : Attribute
                        {
                        }
                    }
                    """, Encoding.UTF8));
                postInitializationContext.AddSource("LoggerAttribute.cs", SourceText.From("""
                    using System;

                    namespace SourceGenerator.Common
                    {
                        [AttributeUsage(AttributeTargets.Class)]
                        public class LoggerAttribute : Attribute
                        {
                        }
                    }
                    """, Encoding.UTF8));
                postInitializationContext.AddSource("KeyAttribute.cs", SourceText.From("""
                    using System;

                    namespace SourceGenerator.Common
                    {
                        [AttributeUsage(AttributeTargets.Field)]
                        public class KeyAttribute : Attribute
                        {
                            public string Key { get; set; }

                            public KeyAttribute(string key)
                            {
                                Key = key;
                            }
                        }
                    }
                    """, Encoding.UTF8));
                postInitializationContext.AddSource("PostConstructAttribute.cs", SourceText.From("""
                    using System;

                    namespace SourceGenerator.Common
                    {
                        [AttributeUsage(AttributeTargets.Method)]
                        public class PostConstructAttribute : Attribute
                        {
                        }
                    }
                    """, Encoding.UTF8));
            });
        }

        public static GeneratedModel<AutoArgsModel> Transform(GeneratorAttributeSyntaxContext context,
            CancellationToken cancellationToken)
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.TargetNode;
            var model = new GeneratedModel<AutoArgsModel>();

            if (!ReportUtils.CheckPartial(model, classDeclarationSyntax))
            {
                return model;
            }

            var namedTypeSymbol = (INamedTypeSymbol)context.TargetSymbol;
            var usings = SyntaxUtils.GetUsings(context.TargetNode.SyntaxTree);
            var fields = new List<FieldMetadata>();

            // 处理 Logger 注入
            ProcessLoggerInjection(namedTypeSymbol, usings, fields);

            // 处理字段
            ProcessFields(namedTypeSymbol, fields);

            // 处理 PostConstruct 方法
            var (postConstructMethod, postConstructParameters) = ProcessPostConstructMethod(namedTypeSymbol, fields);

            if (fields.Count == 0)
            {
                return model;
            }

            // 添加必要的 using 语句
            AddRequiredUsings(fields, usings);

            model.Data = new AutoArgsModel()
            {
                Usings = usings,
                Namespace = SyntaxUtils.GetNamespaceName(classDeclarationSyntax),
                ClassName = SyntaxUtils.GetName(classDeclarationSyntax),
                Fields = fields,
                PostConstructMethod = postConstructMethod,
                PostConstructParameters = postConstructParameters,
            };
            return model;
        }

        private static void ProcessLoggerInjection(INamedTypeSymbol namedTypeSymbol, List<UsingInfo> usings, List<FieldMetadata> fields)
        {
            var hasLogger = namedTypeSymbol.GetAttributes().Any(m =>
                m.AttributeClass!.ToString() == LoggerAttributeName);

            if (hasLogger)
            {
                usings.Add("Microsoft.Extensions.Logging");
                fields.Add(new FieldMetadata()
                {
                    Type = "ILogger<" + namedTypeSymbol.Name + ">",
                    Name = "_logger",
                });
            }
        }

        private static void ProcessFields(INamedTypeSymbol namedTypeSymbol, List<FieldMetadata> fields)
        {
            var fieldSymbols = GetEligibleFields(namedTypeSymbol);

            foreach (var fieldSymbol in fieldSymbols)
            {
                var field = CreateFieldFromSymbol(fieldSymbol);
                if (field != null)
                {
                    fields.Add(field);
                }
            }
        }

        private static List<IFieldSymbol> GetEligibleFields(INamedTypeSymbol namedTypeSymbol)
        {
            return namedTypeSymbol.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(m => m.DeclaredAccessibility == Accessibility.Private &&
                           m.IsReadOnly &&
                           !m.IsStatic &&
                           !m.IsConst &&
                           !HasIgnoreAttribute(m))
                .ToList();
        }

        private static bool HasIgnoreAttribute(IFieldSymbol fieldSymbol)
        {
            return fieldSymbol.GetAttributes().Any(n =>
                n.AttributeClass!.ToString() == IgnoreAttributeName);
        }

        private static FieldMetadata CreateFieldFromSymbol(IFieldSymbol fieldSymbol)
        {
            var syntaxNodes = fieldSymbol.DeclaringSyntaxReferences
                .Select(m => m.GetSyntax())
                .OfType<VariableDeclaratorSyntax>()
                .ToList();

            // 跳过有初始化器的字段
            if (syntaxNodes.Any(m => m.Initializer != null))
            {
                return null;
            }

            if (syntaxNodes.FirstOrDefault()?.Parent is not VariableDeclarationSyntax variableDeclarationSyntax)
            {
                return null;
            }

            var key = ExtractKeyFromAttribute(fieldSymbol);

            return new FieldMetadata()
            {
                Name = fieldSymbol.Name,
                Type = variableDeclarationSyntax.Type.ToString(),
                ServiceKey = key
            };
        }

        private static string ExtractKeyFromAttribute(IFieldSymbol fieldSymbol)
        {
            var keyAttribute = fieldSymbol.GetAttributes().FirstOrDefault(a =>
                a.AttributeClass!.ToString() == KeyAttributeName);

            if (keyAttribute == null)
            {
                return null;
            }

            // 从构造函数参数获取key值
            if (keyAttribute.ConstructorArguments.Length > 0)
            {
                return keyAttribute.ConstructorArguments[0].Value?.ToString();
            }

            // 如果没有构造函数参数，尝试从命名参数获取
            var namedArg = keyAttribute.NamedArguments.FirstOrDefault(na => na.Key == "Key");
            if (!namedArg.Equals(default))
            {
                return namedArg.Value.Value?.ToString();
            }

            return null;
        }

        private static (string method, List<FieldMetadata> parameters) ProcessPostConstructMethod(INamedTypeSymbol namedTypeSymbol, List<FieldMetadata> fields)
        {
            var postConstructMethodSymbol = FindPostConstructMethod(namedTypeSymbol);
            if (postConstructMethodSymbol == null)
            {
                return (null, new List<FieldMetadata>());
            }

            var postConstructParameters = ProcessPostConstructParameters(postConstructMethodSymbol, fields);
            return (postConstructMethodSymbol.Name, postConstructParameters);
        }

        private static IMethodSymbol FindPostConstructMethod(INamedTypeSymbol namedTypeSymbol)
        {
            return namedTypeSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .FirstOrDefault(m =>
                    m.DeclaredAccessibility == Accessibility.Private &&
                    !m.IsStatic &&
                    m.ReturnsVoid &&
                    m.GetAttributes().Any(a => a.AttributeClass!.ToString() == PostConstructAttributeName));
        }

        private static List<FieldMetadata> ProcessPostConstructParameters(IMethodSymbol postConstructMethodSymbol, List<FieldMetadata> fields)
        {
            var postConstructParameters = new List<FieldMetadata>();

            foreach (var parameter in postConstructMethodSymbol.Parameters)
            {
                var parameterSyntax = parameter.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as ParameterSyntax;
                if (parameterSyntax?.Type == null)
                {
                    continue;
                }

                var paramType = parameterSyntax.Type.ToString();
                var paramName = parameter.Name;

                // 检查参数是否已经在现有字段中存在（按类型匹配）
                var existingField = fields.FirstOrDefault(f => f.Type == paramType && !f.Ignore);
                if (existingField == null)
                {
                    // 如果不存在，添加为新的参数（仅用于构造函数参数，不作为字段）
                    var newField = new FieldMetadata()
                    {
                        Type = paramType,
                        Name = paramName,
                        Ignore = true  // 设置为 Ignore，这样不会生成字段赋值
                    };
                    fields.Add(newField);
                    postConstructParameters.Add(newField);
                }
                else
                {
                    // 使用现有字段
                    postConstructParameters.Add(existingField);
                }
            }

            return postConstructParameters;
        }

        private static void AddRequiredUsings(List<FieldMetadata> fields, List<UsingInfo> usings)
        {
            // 如果有 keyed services，添加相应的 using 语句
            if (fields.Any(f => f.ServiceKey != null))
            {
                usings.Add("Microsoft.Extensions.DependencyInjection");
            }
        }

        public static void Output(SourceProductionContext sourceOutputContext, GeneratedModel<AutoArgsModel> model)
        {
            if (model.HasError)
            {
                foreach (var diagnostic in model.Diagnostics)
                {
                    sourceOutputContext.ReportDiagnostic(diagnostic);
                }
                return;
            }

            if (model.Data == null)
            {
                return;
            }

            var data = model.Data;
            var code = Write(data);
            sourceOutputContext.AddSource($"{data.Namespace}.{data.ClassName}.g.cs", code);
        }

        public record AutoArgsModel : ClassInfo
        {
            public bool HasLogger => Fields.Any(m => m.Name == "_logger");
            public bool HasKeyedServices => Fields.Any(m => m.ServiceKey != null);
            public string PostConstructMethod { get; set; }
            public List<FieldMetadata> PostConstructParameters { get; set; } = new List<FieldMetadata>();
        }

        public static string Write(AutoArgsModel model)
        {
            var sw = new StringWriter();
            var writer = new IndentedTextWriter(sw);

            WriteHeader(writer, model);
            WriteClass(writer, model);

            return sw.ToString();
        }

        private static void WriteHeader(IndentedTextWriter writer, AutoArgsModel model)
        {
            writer.WriteLine("// Auto-generated code");
            foreach (var item in model.Usings)
            {
                writer.WriteLine(string.IsNullOrEmpty(item.Alias)
                    ? $"using {item.Name};"
                    : $"using {item.Alias} {item.Name};");
            }
            writer.WriteLineNoTabs("");
        }

        private static void WriteClass(IndentedTextWriter writer, AutoArgsModel model)
        {
            writer.WriteLine($"namespace {model.Namespace}");
            writer.WriteLine("{");
            writer.Indent++;

            writer.WriteLine($"public partial class {model.ClassName}");
            writer.WriteLine("{");
            writer.Indent++;

            WriteLoggerField(writer, model);
            WriteConstructor(writer, model);

            writer.Indent--;
            writer.WriteLine("}");
            writer.Indent--;
            writer.WriteLine("}");
        }

        private static void WriteLoggerField(IndentedTextWriter writer, AutoArgsModel model)
        {
            if (model.HasLogger)
            {
                writer.WriteLine($"private readonly ILogger<{model.ClassName}> _logger;");
                writer.WriteLineNoTabs("");
            }
        }

        private static void WriteConstructor(IndentedTextWriter writer, AutoArgsModel model)
        {
            writer.WriteLine($"public {model.ClassName}({WriteParameters(model)})");
            writer.WriteLine("{");
            writer.Indent++;

            WriteFieldAssignments(writer, model);
            WritePostConstructCall(writer, model);

            writer.Indent--;
            writer.WriteLine("}");
        }

        private static void WriteFieldAssignments(IndentedTextWriter writer, AutoArgsModel model)
        {
            foreach (var field in model.Fields.Where(f => !f.Ignore))
            {
                writer.WriteLine($"this.{field.Name} = {GetParameterName(field.Name)};");
            }
        }

        private static void WritePostConstructCall(IndentedTextWriter writer, AutoArgsModel model)
        {
            if (string.IsNullOrEmpty(model.PostConstructMethod))
            {
                return;
            }

            writer.WriteLineNoTabs("");
            var postConstructArgs = BuildPostConstructArguments(model);
            writer.WriteLine($"this.{model.PostConstructMethod}({postConstructArgs});");
        }

        private static string BuildPostConstructArguments(AutoArgsModel model)
        {
            var args = new List<string>();

            foreach (var param in model.PostConstructParameters)
            {
                // 找到参数在 Fields 列表中的索引
                var fieldIndex = -1;
                for (var j = 0; j < model.Fields.Count; j++)
                {
                    if (ReferenceEquals(model.Fields[j], param))
                    {
                        fieldIndex = j;
                        break;
                    }
                }

                if (fieldIndex >= 0)
                {
                    args.Add(GetParameterName(model.Fields[fieldIndex].Name));
                }
            }

            return string.Join(", ", args);
        }

        private static string WriteParameters(AutoArgsModel model)
        {
            var parameters = new List<string>();

            foreach (var field in model.Fields)
            {
                var parameter = new StringBuilder();

                // 如果有 Key，添加 [FromKeyedServices] 属性
                if (field.ServiceKey != null)
                {
                    parameter.Append($"[FromKeyedServices(\"{field.ServiceKey}\")] ");
                }

                parameter.Append(field.Type);
                parameter.Append(" ");
                parameter.Append(GetParameterName(field.Name));

                parameters.Add(parameter.ToString());
            }

            return string.Join(", ", parameters);
        }

        private static string GetParameterName(string fieldName)
        {
            // 如果字段名以下划线开头，去掉下划线并转为小驼峰命名
            if (fieldName.StartsWith("_"))
            {
                var name = fieldName.Substring(1);
                return char.ToLowerInvariant(name[0]) + name.Substring(1);
            }

            // 如果不是以下划线开头，直接转为小驼峰命名
            return char.ToLowerInvariant(fieldName[0]) + fieldName.Substring(1);
        }
    }
}