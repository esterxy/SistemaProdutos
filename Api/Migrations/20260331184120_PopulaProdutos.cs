using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaProdutos.Migrations
{
    /// <inheritdoc />
    public partial class PopulaProdutos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder mb)
        {
            mb.Sql("INSERT INTO Produtos (Nome, Descricao, Preco, ImageUrl, Estoque, DataCadastro,CategoriaId) " +
                "VALUES ('Coca-Cola', 'Refrigerante de cola', 5.00, 'coca-cola.jpg', 100, NOW(),1)");

            mb.Sql("INSERT INTO Produtos (Nome, Descricao, Preco, ImageUrl, Estoque, DataCadastro,CategoriaId) " +
                "VALUES ('Hambúrguer', 'Hambúrguer artesanal com queijo e bacon', 15.00, 'hamburguer.jpg', 50, NOW(), 2)");

            mb.Sql("INSERT INTO Produtos (Nome, Descricao, Preco, ImageUrl, Estoque, DataCadastro,CategoriaId) " +
                "VALUES ('Sorvete', 'Sorvete de chocolate', 7.00, 'sorvete.jpg', 30, NOW(), 3)");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder mb)
        {
            mb.Sql("DELETE FROM Produtos");
        }
    }
}
