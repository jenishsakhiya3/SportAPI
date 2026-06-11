using Microsoft.EntityFrameworkCore;

namespace SportAPI.Data;

public static class DbConfiguration
{
    public static IServiceCollection AddSportDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SportDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }
}
