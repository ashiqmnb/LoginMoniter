using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoginMonitering.Migrations
{
    /// <inheritdoc />
    public partial class change_username_into_email_in_LoginAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Username",
                table: "LoginAttempts",
                newName: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Email",
                table: "LoginAttempts",
                newName: "Username");
        }
    }
}
