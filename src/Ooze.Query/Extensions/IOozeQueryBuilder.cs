using Microsoft.Extensions.DependencyInjection;

namespace Ooze.Query.Extensions;

public interface IOozeQueryBuilder
{
    IOozeQueryBuilder AddFilterProvider<TProvider>(ServiceLifetime lifetime = ServiceLifetime.Scoped);
}