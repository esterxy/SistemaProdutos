namespace SistemaProdutos.Repositories
{
    public interface IUnitOfWork
    {
        IProdutoRepository ProdutoRepository { get; }
        ICategoriaRepository CategoriaRepository { get; }
        IPedidoRepository PedidoRepository { get; }
        void Commit();
        Task CommitAsync();
    }
}

