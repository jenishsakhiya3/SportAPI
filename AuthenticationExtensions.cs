using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace SportAPI;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddSportAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var azureAdSection = configuration.GetSection("AzureAd");
        var instance = azureAdSection["Instance"] ?? "https://login.microsoftonline.com/";
        var tenantId = azureAdSection["TenantId"] ?? "common";
        var clientId = azureAdSection["ClientId"] ?? string.Empty;
        var audience = azureAdSection["Audience"] ?? clientId;

        var authority = $"{instance.TrimEnd('/')}/{tenantId}/v2.0";

        var validAudiences = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(clientId))
        {
            validAudiences.Add(clientId);
            validAudiences.Add($"api://{clientId}");
        }
        if (!string.IsNullOrWhiteSpace(audience))
        {
            validAudiences.Add(audience);
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = authority;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = !string.Equals(tenantId, "common", StringComparison.OrdinalIgnoreCase) &&
                                 !string.Equals(tenantId, "organizations", StringComparison.OrdinalIgnoreCase),
                ValidIssuer = !string.Equals(tenantId, "common", StringComparison.OrdinalIgnoreCase) &&
                              !string.Equals(tenantId, "organizations", StringComparison.OrdinalIgnoreCase)
                              ? authority : null,
                ValidateAudience = validAudiences.Count > 0,
                ValidAudiences = validAudiences.ToList(),
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            };

            // Custom event to extract MSAL token from URL query string
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Check URL query parameters (e.g. ?token=... or ?access_token=...)
                    if (context.Request.Query.TryGetValue("token", out var tokenValue) ||
                        context.Request.Query.TryGetValue("access_token", out tokenValue))
                    {
                        var token = tokenValue.ToString();
                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            context.Token = token;
                        }
                    }

                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("Authentication");
                    logger.LogWarning("MSAL Token validation failed: {Message}", context.Exception.Message);
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddSwaggerWithJwtAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SportAPI",
                Version = "v1",
                Description = "Sport API secured with MSAL Token Validation (Supports Authorization Header or '?token=' query parameter)"
            });

            // Define Bearer Auth scheme for Swagger UI
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "MSAL Bearer token. Enter 'Bearer {token}' or provide the token.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer"),
                    new List<string>()
                }
            });
        });

        return services;
    }
}
