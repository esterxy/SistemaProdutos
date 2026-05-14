using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaProdutos.DTOs;
using SistemaProdutos.Models;
using SistemaProdutos.Repositories;

namespace SistemaProdutos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProdutosController : ControllerBase
    {
        private readonly IUnitOfWork _uof;
        private readonly ILogger _logger;


        public ProdutosController(IUnitOfWork uof, ILogger<ProdutosController> logger)
        {
            _uof = uof;
            _logger = logger;
        }

        [HttpGet("produtos/{id}")]
        public ActionResult<IEnumerable<Produto>> GetProdutosCategoria(int id)
        {
            var produto = _uof.ProdutoRepository.GetProdutosPorCategoria(id);

            if (produto is null)
            {
                return NotFound();
            }
            return Ok(produto);
        }

        // GET: api/Produtos
        [HttpGet]
        public ActionResult<IQueryable<Produto>> Get([FromQuery] string? nome, [FromQuery] int? categoriaId)
        {
            _logger.LogInformation($"=========================== Iniciando a execução do método GetProdutos ===========================");

            var produtos = _uof.ProdutoRepository.GetAll();
            if (produtos is null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(nome))
            {
                produtos = produtos.Where(p => p.Nome.Contains(nome, StringComparison.OrdinalIgnoreCase)).AsQueryable();
            }

            if (categoriaId.HasValue)
            {
                produtos = produtos.Where(p => p.CategoriaId == categoriaId.Value).AsQueryable();
            }

            return Ok(produtos);
        }

        // GET: api/Produtos/5
        [HttpGet("{id:int:min(1)}", Name = "ObterProduto")]
        public ActionResult<Produto> GetProduto(int id)
        {
            var produto = _uof.ProdutoRepository.Get(c => c.ProdutoId == id);

            if (produto == null)
            {
                _logger.LogInformation($"=========================== Produto com id: {id} não encontrada. =========================== ");
                return NotFound();
            }
            _logger.LogInformation($"=========================== Iniciando a execução do método GetProdutos {id} ===========================");
            return Ok(produto);
        }

        // PUT: api/Produtos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public IActionResult PutProduto(int id, Produto produto)
        {
            if (id != produto.ProdutoId)
            {
                return BadRequest();
            }

            var produtoAtualizado = _uof.ProdutoRepository.Update(produto);
            _uof.Commit();

            return Ok(produtoAtualizado);

        }

        // POST: api/Produtos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public ActionResult<Produto> PostProduto(Produto produto)
        {
            if (produto is null)
            {
                return BadRequest();
            }
            produto.DataCadastro = DateTime.Now;
            var novoProduto = _uof.ProdutoRepository.Create(produto);
            _uof.Commit();

            return new CreatedAtRouteResult("ObterProduto", new { id = novoProduto.ProdutoId }, novoProduto);
        }

        // DELETE: api/Produtos/5
        [HttpDelete("{id}")]
        public IActionResult DeleteProduto(int id)
        {
            var produto = _uof.ProdutoRepository.Get(c => c.ProdutoId == id);
            if (produto is null)
            {
                return NotFound();
            }
            var produtoDeletado = _uof.ProdutoRepository.Delete(produto);
            _uof.Commit();
            return Ok(produtoDeletado);




        }

        /// <summary>
        /// PATCH /api/Produtos/{id}/estoque — Atualiza apenas o estoque (Admin only).
        /// Body: { "estoque": 50 }
        /// </summary>
        [HttpPatch("{id:int}/estoque")]
        [Authorize(Roles = "Admin")]
        public IActionResult AtualizarEstoque(int id, [FromBody] AtualizarEstoqueDto dto)
        {
            var produto = _uof.ProdutoRepository.Get(p => p.ProdutoId == id);
            if (produto == null) return NotFound(new { message = "Produto não encontrado." });

            produto.Estoque = dto.Estoque;
            _uof.ProdutoRepository.Update(produto);
            _uof.Commit();

            _logger.LogInformation("Estoque do produto '{Nome}' atualizado para {Estoque}", produto.Nome, dto.Estoque);
            return Ok(new { message = $"Estoque de '{produto.Nome}' atualizado para {dto.Estoque} unidades.", produto });
        }

    }
}
