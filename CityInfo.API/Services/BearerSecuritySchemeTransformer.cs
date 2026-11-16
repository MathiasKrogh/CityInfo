using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CityInfo.API.Services;

public class BearerSecuritySchemeTransformer(
    IAuthenticationSchemeProvider authenticationSchemeProvider)
    : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var authenticationSchemes =
            await authenticationSchemeProvider.GetAllSchemesAsync();

        if (authenticationSchemes.Any(
            scheme => scheme.Name == "Bearer"))
        {
            // Add the security definition
            document.Components ??= new OpenApiComponents();

            document.Components.SecuritySchemes =
                new Dictionary<string, IOpenApiSecurityScheme>
                {
                    ["CityInfoApiBearerAuth"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        In = ParameterLocation.Header,
                        BearerFormat = "JWT",
                        Description = "Input a valid JWT Bearer token"
                    }
                };

            // Apply the security requirement to all operations
            foreach (var operation in document.Paths.Values
                .SelectMany(path => path.Operations))
            {
                operation.Value.Security ??= [];

                operation.Value.Security.Add(
                    new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference(
                            "CityInfoApiBearerAuth", document)] = []
                    });
            }
        }
    }
}