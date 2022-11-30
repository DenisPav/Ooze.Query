namespace Ooze.Query.Filters;

public interface IOozeQueryFilterProvider<TEntity>
{
    IEnumerable<IQueryFilterDefinition<TEntity>> GetFilters();
}