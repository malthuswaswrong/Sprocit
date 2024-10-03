namespace SharedTestingModels;

public interface IMySprocitTest
{
    IEnumerable<MovieRecord> GetMoviesByRating(float MinRating);
}

public record MovieRecord(int Movie_ID, string Title, int Release_Year, string Genre, string Director, int Duration, double Rating);
