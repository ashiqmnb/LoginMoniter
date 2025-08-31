using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoginMonitering.Migrations
{
    /// <inheritdoc />
    public partial class removed_Used_from_EmailOtp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Used",
                table: "EmailOtps");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Used",
                table: "EmailOtps",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
