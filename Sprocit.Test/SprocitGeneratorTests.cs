using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using MySql.Data.MySqlClient;
using Serilog;
using SharedTestingModels;
using System.Data;
using System.Data.SqlClient;
using Xunit.Abstractions;

namespace Sprocit.Test;

public class SprocitGeneratorTests
{
    ILogger<SprocitGeneratorTests> _logger;
    public SprocitGeneratorTests(ITestOutputHelper output)
    {
        var serilogLogger = new LoggerConfiguration()
            .MinimumLevel.Verbose() // Set minimum log level
            .WriteTo.TestOutput(output) // Write logs to xUnit output
            .CreateLogger();

        // Step 2: Set up .NET's ILogger<T> using Serilog
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders(); // Remove default providers
            loggingBuilder.AddSerilog(serilogLogger); // Use Serilog as the logging provider
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        _logger = serviceProvider.GetRequiredService<ILogger<SprocitGeneratorTests>>();
        SprocitGenerator.InitializeLogger(_logger);
    }
    [Fact]
    public void GetActivatedClassSqlServer()
    {
        SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlServerConnectionString"));
        var cut = SprocitGenerator.GetImplementation<IMySprocitTest>(connection);
        var result = cut.MoviesRatings(8.9f);
        Assert.NotNull(result);
        Assert.Equal(4, result.Count());
    }
    [Fact]
    public void GetActivatedClassIDbConnection()
    {
        IDbConnection connection = new MySqlConnection(Environment.GetEnvironmentVariable("MySqlConnectionString"));
        var cut = SprocitGenerator.GetImplementation<IMySprocitTest>(connection);
        var result = cut.MoviesRatings(8.9f);
        Assert.NotNull(result);
        Assert.Equal(4, result.Count());
        //Assert.Fail("Remove password from connection string");
    }
}