using Microsoft.AspNetCore.Mvc;
using SistemaProdutos.Models;
using SistemaProdutos.Repositories;

namespace SistemaProdutos.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly IProdutoRepository _repository;
        private readonly ILogger _logger;


        public ProdutosController(IProdutoRepository repository, ILogger<ProdutosController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // GET: api/Produtos
        [HttpGet]
        public ActionResult<IQueryable<Produto>> Get()
        {
            _logger.LogInformation($"=========================== Iniciando a execução do método GetProdutos ===========================");

            var produto = _repository.GetProdutos().ToList();
            if (produto is null)
            {
                return NotFound();
            }

            return Ok(produto);
        }

        // GET: api/Produtos/5
        [HttpGet("{id:int:min(1)}")]
        public ActionResult <Produto> GetProduto(int id)
        {
            var produto = _repository.GetProduto;

            if (produto == null)
            {
                _logger.LogInformation($"=========================== Produto com id: {id} não encontrada. =========================== ");
                return NotFound();
            }
            _logger.LogInformation($"=========================== Iniciando a execução do método GetProdutos {id} ===========================");
            return Ok (produto);
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

            bool atualizado  = _repository.Update(produto);
            if (atualizado)
            {
                return Ok(produto);
            }
            else
            {
                return StatusCode(500, $"Falha ao atualizar o produto de Id = {id}");
            }
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
             var novoProduto = _repository.Create(produto);

            return new CreatedAtRouteResult("ObterProduto", new { id = novoProduto.ProdutoId }, novoProduto);
        }

        // DELETE: api/Produtos/5
        [HttpDelete("{id}")]
        public IActionResult DeleteProduto(int id)
        {
            bool deletado = _repository.Delete(id);
            if (deletado)
            {
                return Ok($"Produto de id = {id} foi excluido");
            }
            else
            {
                return StatusCode(500, $" Falha ao excluir produto de id={id}");
            }

            
            
        }

       
    }
}
