using Microsoft.EntityFrameworkCore.Migrations;
using MinecraftSkins.Infrastructure.Data;

#nullable disable

namespace MinecraftSkins.Infrastructure.Migrations;

public partial class UserScopedIdempotencyKeys : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Purchases_IdempotencyKey",
            table: "Purchases");

        migrationBuilder.CreateIndex(
            name: "IX_Purchases_BuyerId_IdempotencyKey",
            table: "Purchases",
            columns: new[] { "BuyerId", "IdempotencyKey" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Purchases_BuyerId_IdempotencyKey",
            table: "Purchases");

        migrationBuilder.CreateIndex(
            name: "IX_Purchases_IdempotencyKey",
            table: "Purchases",
            column: "IdempotencyKey",
            unique: true);
    }
}
