using System.Reflection;

namespace Ooze.Query.Filters;

internal class QueryFilterDefinition<TEntity> : IQueryFilterDefinition<TEntity>
{
    public string Name { get; init; }
    public PropertyInfo TargetProperty { get; init; }
}