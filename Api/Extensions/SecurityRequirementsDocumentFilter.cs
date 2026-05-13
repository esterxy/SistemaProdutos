using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SistemaProdutos.Extensions;

/// <summary>
/// O <see cref="OpenApiSecuritySchemeReference"/> precisa do documento anfitrião para serializar
/// <c>security: [{ "Bearer": [] }]</c>. Sem isso, o Swagger gerava <c>security: [{}]</c> e o UI não enviava o header Authorization.
/// </summary>
public sealed class SecurityRequirementsDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (swaggerDoc.Paths is null)
            return;

        foreach (var (pathKey, pathItem) in swaggerDoc.Paths)
        {
            if (pathItem.Operations is null)
                continue;

            foreach (var (method, operation) in pathItem.Operations)
            {
                if (method == HttpMethod.Post
                    && pathKey.Equals("/api/Auth/login", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                operation.Security =
                [
                    new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", swaggerDoc)] = []
                    }
                ];
            }
        }
    }
}
