using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildMaX.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnalysisRequests_Variants_VariantId",
                table: "AnalysisRequests");

            migrationBuilder.DropIndex(
                name: "IX_LegalDocuments_VariantId",
                table: "LegalDocuments");

            migrationBuilder.DropIndex(
                name: "IX_AnalysisRequests_ApplicationUserId",
                table: "AnalysisRequests");

            migrationBuilder.DropIndex(
                name: "IX_AnalysisRequests_VariantId",
                table: "AnalysisRequests");

            migrationBuilder.CreateIndex(
                name: "IX_LegalDocuments_VariantId_Category",
                table: "LegalDocuments",
                columns: new[] { "VariantId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisRequests_ApplicationUserId_CreatedAt",
                table: "AnalysisRequests",
                columns: new[] { "ApplicationUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisRequests_BuiltUpPercent",
                table: "AnalysisRequests",
                column: "BuiltUpPercent");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisRequests_VariantId_Status",
                table: "AnalysisRequests",
                columns: new[] { "VariantId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_AnalysisRequests_Variants_VariantId",
                table: "AnalysisRequests",
                column: "VariantId",
                principalTable: "Variants",
                principalColumn: "VariantId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnalysisRequests_Variants_VariantId",
                table: "AnalysisRequests");

            migrationBuilder.DropIndex(
                name: "IX_LegalDocuments_VariantId_Category",
                table: "LegalDocuments");

            migrationBuilder.DropIndex(
                name: "IX_AnalysisRequests_ApplicationUserId_CreatedAt",
                table: "AnalysisRequests");

            migrationBuilder.DropIndex(
                name: "IX_AnalysisRequests_BuiltUpPercent",
                table: "AnalysisRequests");

            migrationBuilder.DropIndex(
                name: "IX_AnalysisRequests_VariantId_Status",
                table: "AnalysisRequests");

            migrationBuilder.CreateIndex(
                name: "IX_LegalDocuments_VariantId",
                table: "LegalDocuments",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisRequests_ApplicationUserId",
                table: "AnalysisRequests",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisRequests_VariantId",
                table: "AnalysisRequests",
                column: "VariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_AnalysisRequests_Variants_VariantId",
                table: "AnalysisRequests",
                column: "VariantId",
                principalTable: "Variants",
                principalColumn: "VariantId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
