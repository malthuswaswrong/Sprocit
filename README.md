# Sprocit
Sprocit is a library inspired by Refit to make calling stored procedures easier.

You define an interface with methods that represent stored procedures and Sprocit will generate the necessary code to call them. Sprocit will compile the generated code into an assembly and return it as an implemention of the interface.

By default sprocit will directly map the method names and parameters to the names and parameters of the stored procedure. You can override this behavior by using the `SprocitProcName` attribute on the method to name the stored procedure, and the `SprocitParamName` attribute to name the parameters.

Sprocit supports both SqlConnection and IDbConnection.

## Examples

Defining an interface that calls a stored procedure named `GetMoviesByRating` that takes a parameter named `MinRating` and returns an `IEnumerable<MovieRecord>`.

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

SqlConnection connection = new("Your connection string");
var myInstance = SprocitGenerator.GetImplementation<IMySprocitTest>(connection);

foreach (var movie in myInstance.MoviesRatings(8.9f))
{
    Console.WriteLine(movie.Title);
}
```

Adding your interface directly to dependency injection using the AddSprocit extension method
```csharp

builder.Services.AddSprocit<IMySprocitTest>(() => #your sql connection#);

```

Adding the ISprocitFactory to dependency injection and then using it to create your instance in an endpoint
```csharp
using Sprocit;

...

builder.Services.AddSprocitFactory();

...

app.MapGet("/testsprocit2", (float rating, ISprocitFactory sprocitFactory) =>
{
    IMySprocitTest sprocit = sprocitFactory.GetImplementation<IMySprocitTest>(new SqlConnection("Data Source=(localdb)\\ProjectModels;Initial Catalog=Sprocit;Integrated Security=True;Connect Timeout=30;"));
    var movies = sprocit.MoviesRatings(rating);
    return movies;
})

```

You can see the class that Sprocit generates by passing in an ILogger with the log level set to Trace.

```csharp
using Sprocit;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;

SqlConnection connection = new("Your connection string");
ILogger<Program> logger = GetLogger(); //create your logger and set its log level to Trace
IMySprocitTest myInstance = SprocitGenerator.GetImplementation<IMySprocitTest>(connection, logger);
```

## Current Limitations
- Sprocit only supports stored procedures that return a single result set and the result is IEnumerable<T>.
- Sprocit only supports stored procedures that return a result set that can be mapped to a record or class with primitive type properties.
- Sprocit doesn't have a connection factory or builder.  The connection must be instantiated at the time the Interface Instance is generated.  This is a bummer for dependency injection.  But you can inject the ISprocitFactory and then instantiate your interface with that.