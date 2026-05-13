using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SistemaProdutos.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SistemaProdutos.Controllers
{
    /// <summary>
    /// Controller de autenticação — gera tokens JWT para acesso à API.
    /// 
    /// Em produção, deve ser integrado com ASP.NET Identity ou um IDP externo.
    /// Aqui usamos credenciais mockadas para fins de desenvolvimento.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint de login — valida credenciais e retorna token JWT.
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
            var token = GerarToken(loginDto.Usuario);

            return Ok(token);
        }

        /// <summary>
        /// Gera um token JWT com claims do usuário.
        /// 
        /// O token inclui:
        /// - sub (subject): nome do usuário
        /// - jti (JWT ID): identificador único do token
        /// - role: papel do usuário (Admin mockado)
        /// - exp: expiração configurável via appsettings
        /// </summary>
        private LoginRespostaDto GerarToken(string usuario)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, usuario),
                new Claim(ClaimTypes.Role, "Admin")
            };

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
