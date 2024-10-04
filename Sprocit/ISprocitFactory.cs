using System.Data;
using System.Data.SqlClient;

namespace Sprocit
{
    public interface ISprocitFactory
    {
        T GetImplementation<T>(IDbConnection connection);
        T GetImplementation<T>(SqlConnection connection);
    }
}