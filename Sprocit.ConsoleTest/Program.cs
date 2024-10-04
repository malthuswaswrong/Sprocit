using SharedTestingModels;
using Sprocit;
using System.Data.SqlClient;

SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlServerConnectionString"));
IMySprocitTest sprocit = SprocitGenerator.GetImplementation<IMySprocitTest>(connection);

var movies = sprocit.MoviesRatings(8.9f);
foreach (var movie in movies)
{
    Console.WriteLine(movie.Title);
}