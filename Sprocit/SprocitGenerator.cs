using Dapper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace Sprocit;

internal static class SprocitGenerator
{
    internal static T GetImplementation<T>(SqlConnection connection, ILogger? logger = null)
    {
        string interfaceName = typeof(T).Name;
        if (!interfaceName.StartsWith("I"))
        {
            throw new ArgumentException("Interface name must start with 'I'");
        }
        string className = interfaceName.Substring(1);

        // Generate the code
        string code = GetImplementationCode<T, SqlConnection>(className);

        if (logger?.IsEnabled(LogLevel.Trace) ?? false)
        {
            logger.LogTrace($"{Environment.NewLine}####Sprocit Generated Core####{Environment.NewLine}{code}{Environment.NewLine}####End####");
        }

        // Compile the code into a dynamic assembly
        Assembly assembly = CompileCode(code, className);

        // Load the assembly and create an instance of the class
        var instance = CreateInstance<T>(assembly, className, connection);

        return instance;
    }
    internal static T GetImplementation<T>(IDbConnection connection, ILogger? logger)
    {
        string interfaceName = typeof(T).Name;
        if (!interfaceName.StartsWith("I"))
        {
            throw new ArgumentException("Interface name must start with 'I'");
        }
        string className = interfaceName.Substring(1);

        // Generate the code
        string code = GetImplementationCode<T, IDbConnection>(className);

        if (logger?.IsEnabled(LogLevel.Trace) ?? false)
        {
            logger.LogTrace($"{Environment.NewLine}####Sprocit Generated Core####{Environment.NewLine}{code}{Environment.NewLine}####End####");
        }

        // Compile the code into a dynamic assembly
        Assembly assembly = CompileCode(code, className);

        // Load the assembly and create an instance of the class
        var instance = CreateInstance<T>(assembly, className, connection);

        return instance;
    }

    private static T CreateInstance<T>(Assembly assembly, string className, SqlConnection connection)
    {
        if (assembly == null)
        {
            throw new ArgumentException("Invalid assembly object", nameof(assembly));
        }

        var type = assembly.GetType($"Sprocit.{className}");
        if (type == null)
        {
            throw new InvalidOperationException("Type 'DynamicClass' not found in assembly.");
        }

        var instance = Activator.CreateInstance(type, connection);
        if (instance == null)
        {
            throw new InvalidOperationException("Failed to create an instance of 'DynamicClass'.");
        }

        return (T)instance;
    }
    private static T CreateInstance<T>(Assembly assembly, string className, IDbConnection connection)
    {
        if (assembly == null)
        {
            throw new ArgumentException("Invalid assembly object", nameof(assembly));
        }

        var type = assembly.GetType($"Sprocit.{className}");
        if (type == null)
        {
            throw new InvalidOperationException("Type 'DynamicClass' not found in assembly.");
        }

        var instance = Activator.CreateInstance(type, connection);
        if (instance == null)
        {
            throw new InvalidOperationException("Failed to create an instance of 'DynamicClass'.");
        }

        return (T)instance;
    }

    private static Assembly CompileCode(string code, string className)
    {
        var options = new CSharpCompilationOptions(
            outputKind: OutputKind.DynamicallyLinkedLibrary,
            optimizationLevel: OptimizationLevel.Release);

        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToList();
        var systemReferences = new[]
        {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(SqlConnection).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DynamicParameters).Assembly.Location)
        };
        references.AddRange(systemReferences);

        var compilation = CSharpCompilation.Create($"{className}Assembly")
            .WithOptions(options)
            .AddReferences(references)
            .AddSyntaxTrees(syntaxTree);

        using (var ms = new System.IO.MemoryStream())
        {
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                var failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);

                foreach (var failure in failures)
                {
                    Console.WriteLine($"{failure.Id}: {failure.GetMessage()}");
                }

                throw new InvalidOperationException("Compilation failed.");
            }

            ms.Seek(0, SeekOrigin.Begin);
            var assembly = Assembly.Load(ms.ToArray());
            return assembly!;
        }
    }

    private static string GetImplementationCode<TResult, TConnection>(string className)
    {
        var connectionTypeName = GetTypeName(typeof(TConnection));
        string template = string.Format(_template, className, connectionTypeName);

        var reflectedMethods = typeof(TResult).GetMethods(BindingFlags.Public | BindingFlags.Instance);
        string usingNamespaces = string.Join("\n", GetUserDefinedNamespaces<TResult>()
            .Select(ns => $"using {ns};"));

        template = template.Replace("###USING_NAMESPACES###", usingNamespaces);
        //Create methods
        StringBuilder sb = new StringBuilder();
        foreach (var method in reflectedMethods)
        {
            string generatedMethodSignature = GetMethodSignature(method);
            string? dynamicParametersCode = GetDynamicParametersCode(method);
            string returnType = GetInnerTypeName(method.ReturnType);
            string methodName = GetProcedureName(method);
            sb.AppendLine($"{generatedMethodSignature}");
            sb.AppendLine("{");
            sb.AppendLine(dynamicParametersCode);
            sb.AppendLine($"    return ProcedureRunner<{returnType}>(\"{methodName}\", parameters);");
            sb.AppendLine("}");
        }

        template = template.Replace("###METHODS###", sb.ToString());

        return template;
    }

    private static string GetProcedureName(MethodInfo method)
    {
        var sprocitProcName = method.GetCustomAttribute<SprocitProcNameAttribute>();
        if (sprocitProcName != null)
        {
            return sprocitProcName.ProcedureName;
        }
        return method.Name;
    }

    private static string? GetDynamicParametersCode(MethodInfo method)
    {
        //This method will generate the code to create the DynamicParameters object for the Dapper call
        var parameters = method.GetParameters();
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("    var parameters = new DynamicParameters();");
        if (parameters.Length == 0)
        {
            return sb.ToString(); // TODO test how Dapper works with no parameters
        }
        foreach (var parameter in parameters)
        {
            //For each parameter look for the SprocitParamName attribute and use that name if it exists
            var sprocitParamName = parameter.GetCustomAttribute<SprocitParamNameAttribute>();
            if (sprocitParamName != null)
            {
                sb.AppendLine($"    parameters.Add(\"{sprocitParamName.ParamName}\", {parameter.Name});");
                continue;
            }
            sb.AppendLine($"    parameters.Add(\"{parameter.Name}\", {parameter.Name});");
        }
        return sb.ToString();
    }

    private static string GetMethodSignature(MethodInfo method)
    {
        // Get return type (accounting for generic return types)
        string returnType = GetTypeName(method.ReturnType);

        // Get method name
        string methodName = method.Name;

        // Get parameters
        var parameters = method.GetParameters();
        string parameterSignature = string.Join(", ", parameters.Select(p => $"{GetTypeName(p.ParameterType)} {p.Name}"));

        // Account for method generic parameters
        if (method.IsGenericMethod)
        {
            var genericArguments = method.GetGenericArguments();
            string genericParameters = string.Join(", ", genericArguments.Select(t => t.Name));
            return $"public {returnType} {methodName}<{genericParameters}>({parameterSignature})";
        }
        else
        {
            return $"public {returnType} {methodName}({parameterSignature})";
        }
    }

    // Helper method to get type name, handles generic types
    private static string GetTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            string genericTypeName = type.GetGenericTypeDefinition().Name;
            // Remove the ` character that C# uses for generic types
            genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));

            string genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetTypeName));
            return $"{genericTypeName}<{genericArgs}>";
        }
        else
        {
            return type.Name;
        }
    }
    private static string GetInnerTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            string genericTypeName = type.GetGenericTypeDefinition().Name;
            // Remove the ` character that C# uses for generic types
            genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));

            string genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetTypeName));
            return $"{genericArgs}";
        }
        else
        {
            return type.Name;
        }
    }
    private static List<string> GetUserDefinedNamespaces<T>()
    {
        var namespaces = new HashSet<string>(); // Using HashSet to ensure uniqueness
        var reflectedMethods = typeof(T).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        foreach (var method in reflectedMethods)
        {
            // Add return type namespace
            AddNamespace(method.ReturnType, namespaces);

            // Add namespaces of all parameters
            foreach (var parameter in method.GetParameters())
            {
                AddNamespace(parameter.ParameterType, namespaces);
            }
        }

        return namespaces.Where(ns => !string.IsNullOrEmpty(ns)).ToList();
    }

    // Helper method to add the namespace of a type and handle generic types recursively
    private static void AddNamespace(Type type, HashSet<string> namespaces)
    {
        if (type.IsGenericType)
        {
            // Add namespace of the generic type definition
            if (namespaces.Contains(type.Namespace!) == false) namespaces.Add(type.Namespace!);

            // Recursively add namespaces of generic type arguments
            foreach (var arg in type.GetGenericArguments())
            {
                AddNamespace(arg, namespaces);
            }
        }
        else
        {
            // Add namespace of the non-generic type
            if (namespaces.Contains(type.Namespace!) == false) namespaces.Add(type.Namespace!);
        }
    }

    // Helper method to filter out system namespaces like "System"
    private static bool IsSystemNamespace(string ns)
    {
        return ns.StartsWith("System") || ns.StartsWith("Microsoft");
    }


    private const string _template = """
        using Dapper;
        using System.Data;
        using System.Data.SqlClient;
        
        
        ###USING_NAMESPACES###

        namespace Sprocit;

        public class {0}({1} connection) : I{0}
        {{

        ###METHODS###

            private T ProcedureSingleRunner<T>(string procedureName, DynamicParameters parameters)
            {{
                return connection.QuerySingle<T>(procedureName, parameters, commandType: System.Data.CommandType.StoredProcedure);
            }}
            private IEnumerable<T> ProcedureRunner<T>(string procedureName, DynamicParameters parameters)
            {{
                return connection.Query<T>(procedureName, parameters, commandType: System.Data.CommandType.StoredProcedure);
            }}
        }}
        """;
}
