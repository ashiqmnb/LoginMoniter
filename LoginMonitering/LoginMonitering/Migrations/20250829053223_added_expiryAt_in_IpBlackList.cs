using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoginMonitering.Migrations
{
    /// <inheritdoc />
    public partial class added_expiryAt_in_IpBlackList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryAt",
                table: "IpBlacklists",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiryAt",
                table: "IpBlacklists");
        }
    }
}
