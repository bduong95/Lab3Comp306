using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using MovieStreamingApp.Models;

namespace MovieStreamingApp.Services
{
    public class DynamoDbService
    {
        private readonly Table _moviesTable;

        public DynamoDbService(IAmazonDynamoDB dynamoDbClient)
        {
            _moviesTable = Table.LoadTable(dynamoDbClient, "Movies");
        }

        public async Task SaveMovieAsync(string movieId, string title, string genre, string director, string releaseTime, int rating, string fileUrl, string comments, int ownerId)
        {
            var movie = new Document
            {
                ["MovieID"] = movieId,
                ["Title"] = title,
                ["Genre"] = genre,
                ["Director"] = director,
                ["ReleaseTime"] = releaseTime,
                ["Rating"] = rating,
                ["FileUrl"] = fileUrl,
                ["Comments"] = comments,
                ["OwnerId"] = ownerId
            };

            await _moviesTable.PutItemAsync(movie);
        }

        public async Task<List<Document>> GetAllMoviesAsync()
        {
            var scanFilter = new ScanFilter();
            var search = _moviesTable.Scan(scanFilter);

            var movies = new List<Document>();
            do
            {
                var documents = await search.GetNextSetAsync();
                movies.AddRange(documents);
            } while (!search.IsDone);

            return movies;
        }

        public async Task<Document> GetMovieByIdAsync(string movieId)
        {
            return await _moviesTable.GetItemAsync(movieId);
        }

        public async Task AddCommentAsync(string movieId, string newComment)
        {
            var movie = await _moviesTable.GetItemAsync(movieId);
            if (movie != null)
            {
                string existingComments = movie.ContainsKey("Comments") ? movie["Comments"].AsString() : "";
                string updatedComments = string.IsNullOrEmpty(existingComments)
                    ? newComment
                    : $"{existingComments}\n{newComment}";

                movie["Comments"] = updatedComments;
                await _moviesTable.PutItemAsync(movie);
            }
        }

        public async Task UpdateMovieAsync(Movie updatedMovie)
        {
            var movieDocument = new Document
            {
                ["MovieID"] = updatedMovie.MovieID,
                ["Title"] = updatedMovie.Title,
                ["Genre"] = updatedMovie.Genre,
                ["Director"] = updatedMovie.Director,
                ["ReleaseTime"] = updatedMovie.ReleaseTime,
                ["Rating"] = updatedMovie.Rating,
                ["FileUrl"] = updatedMovie.FileUrl,
                ["Comments"] = updatedMovie.Comments,
                ["OwnerId"] = updatedMovie.OwnerId
            };

            await _moviesTable.PutItemAsync(movieDocument);
        }

        public async Task DeleteMovieAsync(string movieId)
        {
            await _moviesTable.DeleteItemAsync(movieId);
        }

        public async Task<List<Document>> GetMoviesByRatingAsync(int minRating)
        {
            var filter = new ScanFilter();
            filter.AddCondition("Rating", ScanOperator.GreaterThanOrEqual, minRating);

            var config = new ScanOperationConfig
            {
                Filter = filter,
                IndexName = "RatingIndex" // Use the secondary index explicitly
            };

            var search = _moviesTable.Scan(config);
            var movies = new List<Document>();

            do
            {
                var documents = await search.GetNextSetAsync();
                movies.AddRange(documents);
            } while (!search.IsDone);

            return movies;
        }

        public async Task<List<Document>> GetMoviesByGenreAsync(string genre)
        {
            var filter = new QueryFilter("Genre", QueryOperator.Equal, genre);
            var config = new QueryOperationConfig
            {
                IndexName = "GenreIndex",
                Filter = filter,
                ConsistentRead = false
            };

            var search = _moviesTable.Query(config);
            var movies = new List<Document>();
            do
            {
                var documents = await search.GetNextSetAsync();
                movies.AddRange(documents);
            } while (!search.IsDone);

            return movies;
        }
    }
}
