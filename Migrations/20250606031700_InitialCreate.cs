using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unique.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WhitelistEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TweetUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferralCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferredBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferralCodeInput = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhitelistEntries", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WhitelistEntries");
        }
    }
}
