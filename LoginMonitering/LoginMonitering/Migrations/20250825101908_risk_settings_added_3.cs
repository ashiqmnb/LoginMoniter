using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoginMonitering.Migrations
{
    /// <inheritdoc />
    public partial class risk_settings_added_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RiskSettings",
                table: "RiskSettings");

            migrationBuilder.RenameTable(
                name: "RiskSettings",
                newName: "RiskSettingses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RiskSettingses",
                table: "RiskSettingses",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RiskSettingses",
                table: "RiskSettingses");

            migrationBuilder.RenameTable(
                name: "RiskSettingses",
                newName: "RiskSettings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RiskSettings",
                table: "RiskSettings",
                column: "Id");
        }
    }
}
