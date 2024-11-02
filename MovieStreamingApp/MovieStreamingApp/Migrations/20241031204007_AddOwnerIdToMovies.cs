using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieStreamingApp.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerIdToMovies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Movies",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Movies");
        }
    }
}
