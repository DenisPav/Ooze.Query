namespace Ooze.Query;

internal interface IOozeQueryHandler<TEntity>
{
    IQueryable<TEntity> Apply(
        IQueryable<TEntity> query,
        string queryDefinition);
}