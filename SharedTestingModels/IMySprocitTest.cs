using Sprocit;
namespace SharedTestingModels;

public interface IMySprocitTest
{
    [SprocitProcName("GetMoviesByRating")]
    IEnumerable<MovieRecord> MoviesRatings([SprocitParamName("MinRating")] float ratingMin);
}

public record MovieRecord(int Movie_ID, string Title, int Release_Year, string Genre, string Director, int Duration, double Rating);
