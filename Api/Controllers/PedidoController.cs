using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaProdutos.DTOs;
using SistemaProdutos.Services;

namespace SistemaProdutos.Controllers
{
    /// <summary>
    /// Controller de pedidos — endpoints protegidos por JWT.
    /// 
    /// Delega toda a lógica de negócio para o IPedidoService,
    /// mantendo o controller fino (thin controller).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PedidoController : ControllerBase
    {
        private readonly IPedidoService _pedidoService;
        private readonly ILogger<PedidoController> _logger;

        public PedidoController(IPedidoService pedidoService, ILogger<PedidoController> logger)
        {
            _pedidoService = pedidoService;
            _logger = logger;
        }

        /// <summary>
        /// Cria um novo pedido.
        /// 
        /// POST /api/Pedido
        /// Body: { "nomeCliente": "João", "itens": [{ "produtoId": 1, "quantidade": 2 }] }
        /// 
        /// Retorna 201 Created com o pedido completo incluindo valor total calculado.
        /// Retorna 400 se os dados forem inválidos.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PedidoRespostaDto>> CriarPedido([FromBody] CriarPedidoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var resultado = await _pedidoService.CriarPedidoAsync(dto);
                return CreatedAtAction(nameof(ObterPedido), new { id = resultado.PedidoId }, resultado);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Erro de validação ao criar pedido: {Mensagem}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Produto não encontrado ao criar pedido: {Mensagem}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Operação inválida ao criar pedido: {Mensagem}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Busca um pedido por ID com todos os seus itens detalhados.
        /// 
        /// GET /api/Pedido/{id}
        /// 
        /// Retorna 200 com o pedido ou 404 se não existir.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PedidoRespostaDto>> ObterPedido(int id)
        {
            var pedido = await _pedidoService.ObterPedidoAsync(id);

            if (pedido == null)
            {
                return NotFound(new { message = $"Pedido #{id} não encontrado." });
            }

            return Ok(pedido);
        }

        /// <summary>
        /// Lista todos os pedidos ordenados por data decrescente.
        /// 
        /// GET /api/Pedido
        /// 
        /// Retorna 200 com a lista de pedidos.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PedidoRespostaDto>>> ListarPedidos()
        {
            var pedidos = await _pedidoService.ObterTodosPedidosAsync();
            return Ok(pedidos);
        }
    }
}
