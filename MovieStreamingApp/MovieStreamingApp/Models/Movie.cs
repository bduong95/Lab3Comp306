using System;

namespace MovieStreamingApp.Models
{
    public class Movie
    {
        public string MovieID { get; set; }       // Primary key for DynamoDB
        public string Title { get; set; }
        public string Genre { get; set; }
        public string Director { get; set; }
        public string ReleaseTime { get; set; }   // Stored as string in DynamoDB
        public int Rating { get; set; }
        public string? FileUrl { get; set; }
        public string Comments { get; set; }
        public int OwnerId { get; set; }          // User ID of the uploader
    }

}
