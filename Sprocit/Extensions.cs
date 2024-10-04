using Microsoft.Extensions.DependencyInjection;
using System.Data.SqlClient;

namespace Sprocit;

public static class Extensions
{
    public static IServiceCollection AddSprocit<T>(this IServiceCollection services, Func<SqlConnection> sqlConnectionFactory, ServiceLifetime lifetime = ServiceLifetime.Singleton) where T : class
    {
        SqlConnection connection = sqlConnectionFactory();
        services.Add(new ServiceDescriptor(typeof(T), sp => SprocitGenerator.GetImplementation<T>(connection), lifetime));
        return services;
    }
    public static IServiceCollection AddSprocitFactory(this IServiceCollection services)
    {
        services.AddSingleton<ISprocitFactory, SprocitFactory>();
        return services;
    }
}
