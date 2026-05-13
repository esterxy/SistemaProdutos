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
        /// Usa Include + ThenInclude para eager loading em dois níveis.
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
        /// Usa AsNoTracking pois é apenas leitura.
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
    }
}
