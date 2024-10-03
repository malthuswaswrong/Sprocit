using Dapper;
using System.Data.SqlClient;


using System.Collections.Generic;
using Sprocit.Test.Models;
using System;

namespace Sprocit;

public class MySprocitTest(SqlConnection connection) : IMySprocitTest
{

    public IEnumerable<MovieRecord> GetMoviesByRating(Single MinRating)
    {
        var parameters = new DynamicParameters();
        parameters.Add("MinRating", MinRating);

        return ProcedureRunner<MovieRecord>("GetMoviesByRating", parameters);
    }


    private T ProcedureSingleRunner<T>(string procedureName, DynamicParameters parameters)
    {
        return connection.QuerySingle<T>(procedureName, parameters, commandType: System.Data.CommandType.StoredProcedure);
    }
    private IEnumerable<T> ProcedureRunner<T>(string procedureName, DynamicParameters parameters)
    {
        return connection.Query<T>(procedureName, parameters, commandType: System.Data.CommandType.StoredProcedure);
    }
}