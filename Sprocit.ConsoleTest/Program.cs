using SharedTestingModels;
using Sprocit;
using System.Data.SqlClient;

SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlServerConnectionString"));
var sprocit = connection.Sprocit<IMySprocitTest>();

var movies = sprocit.MoviesRatings(8.9f);
foreach (var movie in movies)
{
    Console.WriteLine(movie.Title);
}