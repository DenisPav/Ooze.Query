using Ooze.Query.Filters;

namespace Ooze.Query.Extensions;

public static class OozeQueryServiceCollectionExtensions
{
    private static readonly Type QueryFilterProviderType = typeof(IOozeQueryFilterProvider<>);

    //TODO fix
    // public static IOozeServiceCollectionBuilder AddQueryHandler(this IOozeServiceCollectionBuilder oozeBuilder)
    // {
    //     oozeBuilder.Services.AddScoped(typeof(IOozeQueryHandler<>), typeof(OozeQueryHandler<>));
    //     return oozeBuilder;
    // }
    //
    // public static IOozeServiceCollectionBuilder AddQueryFilter<TQueryFilter>(
    //     this IOozeServiceCollectionBuilder oozeBuilder,
    //     ServiceLifetime providerLifetime = ServiceLifetime.Singleton)
    // {
    //     var queryFilterType = typeof(TQueryFilter);
    //     var implementedInterfaces = queryFilterType.GetInterfaces()
    //         .Where(@type => type.IsGenericType)
    //         .ToList();
    //     var queryFilterProvider = implementedInterfaces.SingleOrDefault(@interface =>
    //         QueryFilterProviderType.IsAssignableFrom(@interface.GetGenericTypeDefinition()));
    //
    //     if (queryFilterProvider is not null)
    //     {
    //         oozeBuilder.Services.Add(new ServiceDescriptor(queryFilterProvider, queryFilterType, providerLifetime));
    //     }
    //
    //     return oozeBuilder;
    // }
}