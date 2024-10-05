# Sprocit
Sprocit is an interface based stored procedure mapping library inspired by [Refit](https://github.com/reactiveui/refit) that makes calling stored procedures easier.

You define an interface with methods that represent stored procedures and Sprocit will generate the necessary code to call them.  The generated code uses [Dapper](https://github.com/DapperLib/Dapper) to call stored procedures and map the results onto the objects you specified as the return type. Sprocit will compile the generated code into an assembly and return it as an implemention of the interface.

By default sprocit will directly map the method names and parameters to the names and parameters of the stored procedure. You can override this behavior by using the `SprocitProcName` attribute on the method to name the stored procedure, and the `SprocitParamName` attribute to name the parameters.

Instantiate an instance of your interface by using the `Sprocit<T>` extension method on your connection.
```csharp
var mySprocit = connection.Sprocit<IMySprocitTest>();
```
Sprocit supports both SqlConnection and IDbConnection.

## Installation

TODO

## Examples

Defining an interface that maps the `MovieRatings` method to a stored procedure named `GetMoviesByRating` and maps a parameter named `ratingMin` to an SQL parameter named `MinRating`.  The stored procedure returns a single result set that maps to `IEnumerable<MovieRecord>`.

```csharp
public interface IMySprocitTest
{
    [SprocitProcName("GetMoviesByRating")]
    IEnumerable<MovieRecord> MoviesRatings([SprocitParamName("MinRating")] float ratingMin);
}

public record MovieRecord(int Movie_ID, string Title, int Release_Year, string Genre, string Director, int Duration, double Rating);

```

Using Sprocit in a console application
```csharp
using Sprocit;
using System.Data.SqlClient;

SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlServerConnectionString"));
var sprocit = connection.Sprocit<IMySprocitTest>();

var movies = sprocit.MoviesRatings(8.9f);
foreach (var movie in movies)
{
    Console.WriteLine(movie.Title);
}
```

Adding your interface to dependency injection and calling a stored procedure in an endpoint using minimal API

```csharp
using Sprocit;

...

builder.Services.AddScoped( _ => (new SqlConnection(Environment.GetEnvironmentVariable("SqlServerConnectionString")!)).Sprocit<IMySprocitTest>());

...


app.MapGet("/testsprocit", (float rating, IMySprocitTest sprocit) => sprocit.MoviesRatings(rating))
.WithOpenApi();

```

## Debugging

You can see the class that Sprocit generates by passing in an ILogger with the log level set to Trace.

```csharp
connection.Sprocit<IMySprocitTest>(logger: logger);
```

## Current Limitations

- Sprocit only supports stored procedures that return a single result set and the result is `IEnumerable<T>`.
- Sprocit only supports stored procedures that return a result set that can be mapped to a record or class with primitive type properties.  Dapper does the mapping.  So if Dapper can do it with it's default behavior, Sprocit can do it.

