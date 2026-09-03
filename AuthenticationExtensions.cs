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

        var validAudiences = new List<string>();
        if (!string.IsNullOrWhiteSpace(clientId))
        {
            validAudiences.Add(clientId);
            validAudiences.Add($"api://{clientId}");
        }
        if (!string.IsNullOrWhiteSpace(audience) && !validAudiences.Contains(audience, StringComparer.OrdinalIgnoreCase))
        {
            validAudiences.Add(audience);
        }

        var shouldValidateAudience = validAudiences.Count > 0;

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
                ValidateIssuer = !string.IsNullOrWhiteSpace(tenantId) &&
                                 !string.Equals(tenantId, "common", StringComparison.OrdinalIgnoreCase) &&
                                 !string.Equals(tenantId, "organizations", StringComparison.OrdinalIgnoreCase),
                ValidIssuer = !string.IsNullOrWhiteSpace(tenantId) &&
                              !string.Equals(tenantId, "common", StringComparison.OrdinalIgnoreCase) &&
                              !string.Equals(tenantId, "organizations", StringComparison.OrdinalIgnoreCase)
                              ? authority : null,
                ValidateAudience = shouldValidateAudience,
                ValidAudiences = shouldValidateAudience ? validAudiences : null,
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
                        var token = tokenValue.ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                            {
                                token = token["Bearer ".Length..].Trim();
                            }
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
