using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildMaX.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalysisRequestOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnalysisRequests_Variants_VariantId",
                table: "AnalysisRequests");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "AnalysisRequests",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisRequests_ApplicationUserId",
                table: "AnalysisRequests",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AnalysisRequests_AspNetUsers_ApplicationUserId",
                table: "AnalysisRequests",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AnalysisRequests_Variants_VariantId",
                table: "AnalysisRequests",
                column: "VariantId",
                principalTable: "Variants",
                principalColumn: "VariantId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnalysisRequests_AspNetUsers_ApplicationUserId",
                table: "AnalysisRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_AnalysisRequests_Variants_VariantId",
                table: "AnalysisRequests");

            migrationBuilder.DropIndex(
                name: "IX_AnalysisRequests_ApplicationUserId",
                table: "AnalysisRequests");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "AnalysisRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_AnalysisRequests_Variants_VariantId",
                table: "AnalysisRequests",
                column: "VariantId",
                principalTable: "Variants",
                principalColumn: "VariantId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
