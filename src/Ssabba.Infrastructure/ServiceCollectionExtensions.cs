using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ssabba.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers the PostgreSQL-backed <see cref="SsabbaDbContext"/>.</summary>
    public static IServiceCollection AddSsabbaInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContextFactory<SsabbaDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }
}
