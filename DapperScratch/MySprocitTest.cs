// See https://aka.ms/new-console-template for more information
using Dapper;
using System.Data.SqlClient;
using System;
using System.Collections.Generic;


namespace Sprocit.Extensions.YourClassNameGoesHere;
public class MySprocitTest(SqlConnection connection) : IMySprocitTest
{
    public string? RunHelloProcedure(string? name)
    {
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add("@param1", name);
        var result = ProcedureSingleRunner<string?>("SayHello", parameters);

        return result;
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

public interface IMySprocitTest
{
    string? RunHelloProcedure(string? name);
}