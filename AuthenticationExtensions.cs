using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.IdentityModel.Tokens.Jwt;

namespace SportAPI;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddSportAuthentication(this IServiceCollection services, IConfiguration configuration){
    // 1. Read values dynamically from Azure App Service Environment Variables
    var tenantId = configuration["TenantId"];
    var backendClientId = configuration["BackendClientId"];

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            // 2. Set the Authority using the TenantId environment variable
            options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = backendClientId,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            };

            // Read token from URL query string or Authorization header
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
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
                Description = "Sport API secured with Easy MSAL Token Validation"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Enter MSAL token (or pass via ?token=... in URL)",
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
