using System.Text;
using ClinicScheduling.Api.Common.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace ClinicScheduling.Api.Common.InjectionDependency;

public static class JwtAuthInjection
{
    public static void AddJWTAuth(this IServiceCollection services, IConfiguration configuration)
    {
        string jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
                        ?? configuration["Jwt:Key"]
                        ?? string.Empty;

        string jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                           ?? configuration["Jwt:Issuer"]
                           ?? string.Empty;

        string jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                             ?? configuration["Jwt:Audience"]
                             ?? string.Empty;

        int expireMinutes = int.TryParse(
            Environment.GetEnvironmentVariable("JWT_EXPIRE_MINUTES")
            ?? configuration["Jwt:ExpireMinutes"],
            out int result)
            ? result
            : 120;

        JwtSettings jwtSettings = new()
        {
            Key = jwtKey,
            Issuer = jwtIssuer,
            Audience = jwtAudience,
            ExpireMinutes = expireMinutes
        };

        services.AddSingleton(jwtSettings);

        byte[] keyBytes = Encoding.UTF8.GetBytes(jwtKey);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,

                    ValidateAudience = true,
                    ValidAudience = jwtAudience,

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();
    }
}