using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;

namespace Sprocit;

public static class Extensions
{
    public static T Sprocit<T>(this SqlConnection connection, ILogger? logger = null) => SprocitGenerator.GetImplementation<T>(connection, logger);
    public static T Sprocit<T>(this IDbConnection connection, ILogger? logger = null) => SprocitGenerator.GetImplementation<T>(connection, logger);
}
