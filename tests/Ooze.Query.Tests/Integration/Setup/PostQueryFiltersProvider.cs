using Ooze.Query.Filters;

namespace Ooze.Query.Tests.Integration.Setup;

public class PostQueryFiltersProvider : IOozeQueryFilterProvider<Post>
{
    public IEnumerable<IQueryFilterDefinition<Post>> GetFilters()
    {
        return QueryFilters.CreateFor<Post>()
            .Add(x => x.Id)
            .Add(x => x.Name, name: "DifferentName")
            .Add(x => x.Date)
            .Build();
    }
}