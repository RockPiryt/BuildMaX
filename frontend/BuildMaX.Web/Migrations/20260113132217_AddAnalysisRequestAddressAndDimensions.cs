using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildMaX.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalysisRequestAddressAndDimensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AddressKind",
                table: "AnalysisRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CadastralArea",
                table: "AnalysisRequests",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "AnalysisRequests",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Commune",
                table: "AnalysisRequests",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "AnalysisRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ModuleLengthM",
                table: "AnalysisRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ModuleWidthM",
                table: "AnalysisRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PlotLengthM",
                table: "AnalysisRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PlotNumber",
                table: "AnalysisRequests",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PlotWidthM",
                table: "AnalysisRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "AnalysisRequests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "AnalysisRequests",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StreetNumber",
                table: "AnalysisRequests",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressKind",
                table: "AnalysisRequests");

            migrationBuilder.DropColumn(
                name: "CadastralArea",
                table: "AnalysisRequests");

            migrationBuilder.DropColumn(
                name: "City",
                table: "AnalysisRequests");

            migrationBuilder.DropColumn(
                name: "Commune",
                table: "AnalysisRequests");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "AnalysisRequests");

            migrationBuilder.DropColumn(
                name: "ModuleLengthM",
                table: "AnalysisRequests");

            migrationBuilder.DropColumn(
                name: "ModuleWidthM",
                table: "AnalysisRequests");

            migrationBuilder.DropColumn(
                name: "PlotLengthM",
                table: "AnalysisRequests");

            migrationBuilder.DropColumn(
                name: "PlotNumber",
                table: "AnalysisRequests");

            migrationBuilder.DropColumn(
                name: "PlotWidthM",
                table: "AnalysisRequests");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "AnalysisRequests");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "AnalysisRequests");

            migrationBuilder.DropColumn(
                name: "StreetNumber",
                table: "AnalysisRequests");
        }
    }
}
