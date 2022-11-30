namespace Ooze.Query.Filters;

public static class QueryFilters
{
    public static IQueryFilterBuilder<TEntity> CreateFor<TEntity>()
        => new QueryFilterBuilder<TEntity>();
}