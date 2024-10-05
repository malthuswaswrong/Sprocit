using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    [Fact]
    public void GetActivatedClassSqlServer()
    {
        SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlServerConnectionString"));
        var cut = connection.Sprocit<IMySprocitTest>(_logger);
        var result = cut.MoviesRatings(8.9f);
        Assert.NotNull(result);
        Assert.Equal(4, result.Count());
    }
    [Fact]
    public void GetActivatedClassIDbConnection()
    {
        IDbConnection connection = new MySqlConnection(Environment.GetEnvironmentVariable("MySqlConnectionString"));
        var cut = connection.Sprocit<IMySprocitTest>(_logger);
        var result = cut.MoviesRatings(8.9f);
        Assert.NotNull(result);
        Assert.Equal(4, result.Count());
    }
}