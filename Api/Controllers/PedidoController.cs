using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaProdutos.DTOs;
using SistemaProdutos.Services;
using System.Security.Claims;

namespace SistemaProdutos.Controllers
{
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
        /// Retorna o clienteId do JWT, ou null se for admin.
        /// </summary>
        private int? ObterClienteId()
        {
            var claim = User.FindFirst("clienteId");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : null;
        }

        private bool IsAdmin() => User.IsInRole("Admin");

        /// <summary>
        /// POST /api/Pedido — Cria pedido vinculado ao cliente logado (ou sem vínculo se admin).
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PedidoRespostaDto>> CriarPedido([FromBody] CriarPedidoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var clienteId = ObterClienteId();
                var resultado = await _pedidoService.CriarPedidoAsync(dto, clienteId);
                return CreatedAtAction(nameof(ObterPedido), new { id = resultado.PedidoId }, resultado);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>
        /// GET /api/Pedido/{id}
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PedidoRespostaDto>> ObterPedido(int id)
        {
            var pedido = await _pedidoService.ObterPedidoAsync(id);
            if (pedido == null) return NotFound(new { message = $"Pedido #{id} não encontrado." });
            return Ok(pedido);
        }

        /// <summary>
        /// GET /api/Pedido?categoriaId=2
        /// Admin → todos os pedidos. Cliente → só os seus.
        /// Filtro opcional por categoria.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PedidoRespostaDto>>> ListarPedidos([FromQuery] int? categoriaId)
        {
            int? clienteId = IsAdmin() ? null : ObterClienteId();
            var pedidos = await _pedidoService.ObterPedidosFiltradosAsync(clienteId, categoriaId);
            return Ok(pedidos);
        }

        /// <summary>
        /// DELETE /api/Pedido/{id}
        /// Admin pode cancelar qualquer pedido. Cliente só os seus.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> CancelarPedido(int id)
        {
            int? clienteId = IsAdmin() ? null : ObterClienteId();
            var cancelado = await _pedidoService.CancelarPedidoAsync(id, clienteId);

            if (!cancelado)
                return NotFound(new { message = $"Pedido #{id} não encontrado ou não pertence a você." });

            return Ok(new { message = $"Pedido #{id} cancelado com sucesso. Estoque restaurado." });
        }
    }
}
