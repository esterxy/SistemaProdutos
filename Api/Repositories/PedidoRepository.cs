using Microsoft.EntityFrameworkCore;
using SistemaProdutos.Context;
using SistemaProdutos.Models;

namespace SistemaProdutos.Repositories
{
    public class PedidoRepository : Repository<Pedido>, IPedidoRepository
    {
        public PedidoRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Busca um pedido específico incluindo seus itens e os dados do produto de cada item.
        /// </summary>
        public Pedido? GetPedidoComItens(int id)
        {
            return _context.Pedidos
                .Include(p => p.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefault(p => p.PedidoId == id);
        }

        /// <summary>
        /// Busca todos os pedidos com eager loading completo.
        /// </summary>
        public IEnumerable<Pedido> GetTodosComItens()
        {
            return _context.Pedidos
                .Include(p => p.Itens)
                    .ThenInclude(i => i.Produto)
                .AsNoTracking()
                .OrderByDescending(p => p.DataPedido)
                .ToList();
        }

        /// <summary>
        /// Busca pedidos de um cliente específico.
        /// </summary>
        public IEnumerable<Pedido> GetPedidosPorCliente(int clienteId)
        {
            return _context.Pedidos
                .Include(p => p.Itens)
                    .ThenInclude(i => i.Produto)
                .AsNoTracking()
                .Where(p => p.ClienteId == clienteId)
                .OrderByDescending(p => p.DataPedido)
                .ToList();
        }

        /// <summary>
        /// Busca pedidos filtrados. clienteId=null → todos. categoriaId filtra itens por categoria.
        /// </summary>
        public IEnumerable<Pedido> GetPedidosFiltrados(int? clienteId, int? categoriaId)
        {
            var query = _context.Pedidos
                .Include(p => p.Itens)
                    .ThenInclude(i => i.Produto)
                .AsNoTracking()
                .AsQueryable();

            if (clienteId.HasValue)
            {
                query = query.Where(p => p.ClienteId == clienteId.Value);
            }

            if (categoriaId.HasValue)
            {
                // Filtra pedidos que contenham ao menos um item da categoria
                query = query.Where(p => p.Itens.Any(i => i.Produto != null && i.Produto.CategoriaId == categoriaId.Value));
            }

            return query
                .OrderByDescending(p => p.DataPedido)
                .ToList();
        }
    }
}
