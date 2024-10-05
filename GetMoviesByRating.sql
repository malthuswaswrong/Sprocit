USE [Sprocit]
GO

/****** Object: SqlProcedure [dbo].[GetMoviesByRating] Script Date: 10/5/2024 12:59:47 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE GetMoviesByRating
    @MinRating FLOAT
AS
BEGIN
    -- Create a temporary table to store movie data
    CREATE TABLE #Movies (
        Movie_ID INT,
        Title NVARCHAR(255),
        Release_Year INT,
        Genre NVARCHAR(50),
        Director NVARCHAR(255),
        Duration INT,
        Rating FLOAT
    );

    -- Insert test data into the temporary table
    INSERT INTO #Movies (Movie_ID, Title, Release_Year, Genre, Director, Duration, Rating)
    VALUES
        (1, 'The Great Escape', 1963, 'Adventure', 'John Sturges', 172, 8.2),
        (2, 'Inception', 2010, 'Sci-Fi', 'Christopher Nolan', 148, 8.8),
        (3, 'Casablanca', 1942, 'Drama', 'Michael Curtiz', 102, 8.5),
        (4, 'The Godfather', 1972, 'Crime', 'Francis Ford Coppola', 175, 9.2),
        (5, 'Pulp Fiction', 1994, 'Crime', 'Quentin Tarantino', 154, 8.9),
        (6, 'The Dark Knight', 2008, 'Action', 'Christopher Nolan', 152, 9.0),
        (7, 'Parasite', 2019, 'Thriller', 'Bong Joon Ho', 132, 8.6),
        (8, 'Spirited Away', 2001, 'Animation', 'Hayao Miyazaki', 125, 8.6),
        (9, 'Schindler''s List', 1993, 'Biography', 'Steven Spielberg', 195, 9.0),
        (10, 'La La Land', 2016, 'Musical', 'Damien Chazelle', 128, 8.0);

    -- Query the data based on the provided rating filter
	-- Rating explicitly cast to double to make unit testing between SQL Server and Generic IDbConnection equivalent
    SELECT Movie_ID, Title, Release_Year, Genre, Director, Duration, Rating
    FROM #Movies
    WHERE Rating >= @MinRating;

    -- Drop the temporary table to clean up
    DROP TABLE #Movies;
END;
