using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Serilog;
using SharedTestingModels;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using Xunit.Abstractions;

namespace Sprocit.Test;

public class SprocitGeneratorTests
{
    ILogger<SprocitGeneratorTests> _logger;
    public SprocitGeneratorTests(ITestOutputHelper output)
    {
        var serilogLogger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.TestOutput(output)
            .CreateLogger();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(serilogLogger);
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        _logger = serviceProvider.GetRequiredService<ILogger<SprocitGeneratorTests>>();
    }
    [Fact(Skip = "Integration test requires database connection.")]
    public void GetActivatedClassSqlServer()
    {
        SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlServerConnectionString"));
        var cut = connection.Sprocit<IMySprocitTest>(_logger);
        var result = cut.MoviesRatings(8.9f);
        Assert.NotNull(result);
        Assert.Equal(4, result.Count());
    }
    [Fact(Skip = "Integration test requires database connection.")]
    public void GetActivatedClassIDbConnection()
    {
        IDbConnection connection = new MySqlConnection(Environment.GetEnvironmentVariable("MySqlConnectionString"));
        var cut = connection.Sprocit<IMySprocitTest>(_logger);
        var result = cut.MoviesRatings(8.9f);
        Assert.NotNull(result);
        Assert.Equal(4, result.Count());
    }

    [Fact]
    public void GetUserDefinedNamespaces_FiltersSystemNamespaces()
    {
        // Use reflection to invoke private GetUserDefinedNamespaces<T>() method
        var generatorType = typeof(Extensions).Assembly.GetType("Sprocit.SprocitGenerator", true)!;
        var methodInfo = generatorType.GetMethod("GetUserDefinedNamespaces", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(methodInfo);

        var genericMethod = methodInfo!.MakeGenericMethod(typeof(IMySprocitTest));
        var result = (IEnumerable<string>)genericMethod.Invoke(null, null)!;

        Assert.Contains("SharedTestingModels", result);
        Assert.DoesNotContain("System", result);
        Assert.DoesNotContain("Microsoft", result);
    }
}