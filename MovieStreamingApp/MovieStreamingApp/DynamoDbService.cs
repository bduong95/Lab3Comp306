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
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly Table _moviesTable;

        public DynamoDbService(IAmazonDynamoDB dynamoDbClient)
        {
            // Initialize DynamoDB client (injected via IAmazonDynamoDB)
            _dynamoDbClient = dynamoDbClient;

            // Load the DynamoDB table
            _moviesTable = Table.LoadTable(dynamoDbClient, "Movies");
        }

        // Method to save movie metadata to DynamoDB
        public async Task SaveMovieAsync(string movieId, string title, string genre, string director, string releaseTime, int rating, string fileUrl, string comments, int ownerId)
        {
            var movie = new Document
            {
                ["MovieID"] = movieId,                      // Primary Key
                ["Title"] = title,
                ["Genre"] = genre,
                ["Director"] = director,
                ["ReleaseTime"] = releaseTime,              // Now stored as a string directly
                ["Rating"] = rating,
                ["FileUrl"] = fileUrl,
                ["Comments"] = comments,                    // Initialize comments as an empty string by default
                ["OwnerId"] = ownerId                       // Store OwnerId for ownership
            };

            await _moviesTable.PutItemAsync(movie);
        }

        // Method to retrieve all movies from DynamoDB
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

        // Method to get a specific movie by MovieID
        public async Task<Document> GetMovieByIdAsync(string movieId)
        {
            return await _moviesTable.GetItemAsync(movieId);
        }

        // Method to add a comment to a specific movie (appending to the Comments string)
        public async Task AddCommentAsync(string movieId, string newComment)
        {
            var movie = await _moviesTable.GetItemAsync(movieId);
            if (movie != null)
            {
                // Retrieve existing comments and append the new comment
                string existingComments = movie.ContainsKey("Comments") ? movie["Comments"].AsString() : "";
                string updatedComments = string.IsNullOrEmpty(existingComments)
                    ? newComment
                    : $"{existingComments}\n{newComment}";

                movie["Comments"] = updatedComments;

                await _moviesTable.PutItemAsync(movie);
            }
        }

        // Method to update movie metadata in DynamoDB
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

        // Method to delete a movie from DynamoDB
        public async Task DeleteMovieAsync(string movieId)
        {
            await _moviesTable.DeleteItemAsync(movieId);
        }
    }
}
