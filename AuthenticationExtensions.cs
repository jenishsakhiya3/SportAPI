using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.IdentityModel.Tokens.Jwt;

namespace SportAPI;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddSportAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Simple & Easy Token Validation (validates token format, lifetime/expiry, and extracts claims)
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,             // Accepts token from your tenant without issuer mismatch
                    ValidateAudience = false,           // Accepts ID tokens & Access tokens without audience errors
                    ValidateLifetime = true,           // Checks that token is not expired
                    ValidateIssuerSigningKey = false,  // Flexible signing key validation
                    SignatureValidator = (token, parameters) => new JwtSecurityToken(token)
                };

                // Read token from URL query string (?token=... or ?access_token=...) or Authorization header
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
