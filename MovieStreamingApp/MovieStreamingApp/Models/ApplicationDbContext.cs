using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace MovieStreamingApp.Models
{
    // This class connects our models to the database
    public class ApplicationDbContext : DbContext
    {
        // Constructor to configure options for DbContext
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet represents the table for Users in the database
        public DbSet<User> Users { get; set; }

    }
}
