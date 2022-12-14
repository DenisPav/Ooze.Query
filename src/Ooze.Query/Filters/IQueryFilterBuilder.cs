using System.Linq.Expressions;

namespace Ooze.Query.Filters;

public interface IQueryFilterBuilder<TEntity>
{
    IQueryFilterBuilder<TEntity> Add<TProperty>(
        Expression<Func<TEntity, TProperty>> dataExpression,
        string name = null);
    IEnumerable<IQueryFilterDefinition<TEntity>> Build();
}