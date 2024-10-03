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

public class GeneratorTests
{
    ILogger<GeneratorTests> _logger;
    public GeneratorTests(ITestOutputHelper output)
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
        _logger = serviceProvider.GetRequiredService<ILogger<GeneratorTests>>();
    }
    [Fact]
    public void GetActivatedClassSqlServer()
    {
        Generator.InitializeLogger(_logger);
        SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlServerConnectionString"));
        var cut = Generator.GetImplementation<IMySprocitTest>(connection);
        var result = cut.GetMoviesByRating(8.9f);
        Assert.NotNull(result);
        Assert.Equal(4, result.Count());
    }
    [Fact]
    public void GetActivatedClassIDbConnection()
    {
        Generator.InitializeLogger(_logger);
        IDbConnection connection = new MySqlConnection(Environment.GetEnvironmentVariable("MySqlConnectionString"));
        var cut = Generator.GetImplementation<IMySprocitTest>(connection);
        var result = cut.GetMoviesByRating(8.9f);
        Assert.NotNull(result);
        Assert.Equal(4, result.Count());
        //Assert.Fail("Remove password from connection string");
    }
}