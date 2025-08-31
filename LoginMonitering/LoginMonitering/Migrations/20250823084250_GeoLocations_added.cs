using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoginMonitering.Migrations
{
    /// <inheritdoc />
    public partial class GeoLocations_added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GeoLocationId",
                table: "LoginAttempts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GeoLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timezone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Query = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoLocations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_GeoLocationId",
                table: "LoginAttempts",
                column: "GeoLocationId",
                unique: true,
                filter: "[GeoLocationId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_LoginAttempts_GeoLocations_GeoLocationId",
                table: "LoginAttempts",
                column: "GeoLocationId",
                principalTable: "GeoLocations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoginAttempts_GeoLocations_GeoLocationId",
                table: "LoginAttempts");

            migrationBuilder.DropTable(
                name: "GeoLocations");

            migrationBuilder.DropIndex(
                name: "IX_LoginAttempts_GeoLocationId",
                table: "LoginAttempts");

            migrationBuilder.DropColumn(
                name: "GeoLocationId",
                table: "LoginAttempts");
        }
    }
}
