using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaProdutos.Context;
using SistemaProdutos.DTOs;
using SistemaProdutos.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SistemaProdutos.Controllers
{
    /// <summary>
    /// Controller de autenticação — gera tokens JWT para acesso à API.
    /// 
    /// Suporta:
    /// - Login admin mockado (POST /api/Auth/login)
    /// - Cadastro de novos clientes (POST /api/Auth/cadastro)
    /// - Login de clientes cadastrados via email (POST /api/Auth/login-email)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly AppDbContext _context;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger, AppDbContext context)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Endpoint de login admin — valida credenciais mockadas e retorna token JWT.
        /// 
        /// POST /api/Auth/login
        /// Body: { "usuario": "admin", "senha": "admin123" }
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public ActionResult<LoginRespostaDto> Login([FromBody] LoginDto loginDto)
        {
            // Validação do ModelState (Data Annotations)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Credenciais mockadas — em produção, usar Identity/banco
            if (loginDto.Usuario != "admin" || loginDto.Senha != "admin123")
            {
                _logger.LogWarning("Tentativa de login falhou para o usuário: {Usuario}", loginDto.Usuario);
                return Unauthorized(new { message = "Usuário ou senha inválidos." });
            }

            _logger.LogInformation("Login realizado com sucesso: {Usuario}", loginDto.Usuario);

            // Geração do token JWT
            var token = GerarToken(loginDto.Usuario, "Admin");

            return Ok(token);
        }

        /// <summary>
        /// Endpoint de login para clientes cadastrados — valida email e senha via banco de dados.
        /// 
        /// POST /api/Auth/login-email
        /// Body: { "email": "cliente@email.com", "senha": "123456" }
        /// </summary>
        [HttpPost("login-email")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginRespostaDto>> LoginEmail([FromBody] LoginEmailDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Email == loginDto.Email);

            if (cliente == null)
            {
                _logger.LogWarning("Login falhou — email não encontrado: {Email}", loginDto.Email);
                return Unauthorized(new { message = "Email ou senha inválidos." });
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Senha, cliente.SenhaHash))
            {
                _logger.LogWarning("Login falhou — senha incorreta para: {Email}", loginDto.Email);
                return Unauthorized(new { message = "Email ou senha inválidos." });
            }

            _logger.LogInformation("Login via email realizado: {Email}", loginDto.Email);

            var token = GerarToken(cliente.Nome, "Cliente", cliente.ClienteId);

            return Ok(token);
        }

        /// <summary>
        /// Endpoint de cadastro de novos clientes.
        /// 
        /// POST /api/Auth/cadastro
        /// Body: { "nome": "João", "email": "joao@email.com", "senha": "123456" }
        /// </summary>
        [HttpPost("cadastro")]
        [AllowAnonymous]
        public async Task<ActionResult<CadastroRespostaDto>> Cadastrar([FromBody] CadastroDto cadastroDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verifica se o email já está cadastrado
            var emailExiste = await _context.Clientes
                .AnyAsync(c => c.Email == cadastroDto.Email);

            if (emailExiste)
            {
                _logger.LogWarning("Tentativa de cadastro com email duplicado: {Email}", cadastroDto.Email);
                return Conflict(new { message = "Este email já está cadastrado." });
            }

            // Cria o cliente com senha hashada
            var cliente = new Cliente
            {
                Nome = cadastroDto.Nome,
                Email = cadastroDto.Email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(cadastroDto.Senha),
                DataCadastro = DateTime.Now
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Novo cliente cadastrado: {Nome} ({Email})", cliente.Nome, cliente.Email);

            // Gera token automaticamente após cadastro (auto-login)
            var token = GerarToken(cliente.Nome, "Cliente", cliente.ClienteId);

            return Created(string.Empty, new CadastroRespostaDto
            {
                ClienteId = cliente.ClienteId,
                Nome = cliente.Nome,
                Email = cliente.Email,
                Token = token.Token,
                Expiracao = token.Expiracao
            });
        }

        /// <summary>
        /// Gera um token JWT com claims do usuário.
        /// 
        /// O token inclui:
        /// - sub (subject): nome do usuário
        /// - jti (JWT ID): identificador único do token
        /// - role: papel do usuário (Admin ou Cliente)
        /// - exp: expiração configurável via appsettings
        /// </summary>
        private LoginRespostaDto GerarToken(string usuario, string role, int? clienteId = null)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, usuario),
                new Claim(ClaimTypes.Role, role)
            };

            if (clienteId.HasValue)
            {
                claims.Add(new Claim("clienteId", clienteId.Value.ToString()));
            }

            var expireMinutes = int.Parse(jwtSettings["ExpireMinutes"] ?? "60");
            var expiration = DateTime.UtcNow.AddMinutes(expireMinutes);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
            );

            return new LoginRespostaDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiracao = expiration,
                Usuario = usuario
            };
        }
    }
}
