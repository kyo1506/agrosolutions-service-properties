using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroSolutions.Properties.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyProdutorEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Produtores_Cpf",
                table: "Produtores");

            migrationBuilder.DropColumn(
                name: "Cep",
                table: "Produtores");

            migrationBuilder.DropColumn(
                name: "Cidade",
                table: "Produtores");

            migrationBuilder.DropColumn(
                name: "Cpf",
                table: "Produtores");

            migrationBuilder.DropColumn(
                name: "Endereco",
                table: "Produtores");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Produtores");

            migrationBuilder.DropColumn(
                name: "Telefone",
                table: "Produtores");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cep",
                table: "Produtores",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cidade",
                table: "Produtores",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cpf",
                table: "Produtores",
                type: "character varying(11)",
                maxLength: 11,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Endereco",
                table: "Produtores",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Produtores",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefone",
                table: "Produtores",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Produtores_Cpf",
                table: "Produtores",
                column: "Cpf",
                unique: true);
        }
    }
}
