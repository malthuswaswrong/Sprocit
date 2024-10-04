using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Data.SqlClient;

namespace Sprocit;

public class SprocitFactory(IOptions<ILogger<SprocitFactory>>? loggerOptions) : ISprocitFactory
{
    public T GetImplementation<T>(IDbConnection connection)
    {
        return SprocitGenerator.GetImplementation<T>(connection, loggerOptions?.Value);
    }
    public T GetImplementation<T>(SqlConnection connection)
    {
        return SprocitGenerator.GetImplementation<T>(connection, loggerOptions?.Value);
    }
}