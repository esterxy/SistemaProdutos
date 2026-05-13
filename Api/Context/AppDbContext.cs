using Microsoft.EntityFrameworkCore;
using SistemaProdutos.Models;

namespace SistemaProdutos.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<ItemPedido> ItensPedido { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração do Pedido
            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.HasKey(p => p.PedidoId);

                entity.Property(p => p.ValorTotal)
                    .HasColumnType("decimal(10,2)");

                entity.HasMany(p => p.Itens)
                    .WithOne(i => i.Pedido)
                    .HasForeignKey(i => i.PedidoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuração do ItemPedido
            modelBuilder.Entity<ItemPedido>(entity =>
            {
                entity.HasKey(i => i.ItemPedidoId);

                entity.Property(i => i.PrecoUnitario)
                    .HasColumnType("decimal(10,2)");

                entity.Property(i => i.SubTotal)
                    .HasColumnType("decimal(10,2)");

                entity.HasOne(i => i.Produto)
                    .WithMany()
                    .HasForeignKey(i => i.ProdutoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
