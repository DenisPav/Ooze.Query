# Ooze.Query 🔎
This package enables usage of `Readable Queries` on `IQueryable<T>` instances. This is a concept where you can write actual
expression as a string representation for filtering. For example if you have bunch of `Posts` which have `Id` and `Name` columns, you could filter them out via something like `Id >= '3' && Name != 'My Post'`. This in turn will be translated into the correct expression and applied to your `IQueryable` instance.

Supported logical operations for queries are AND (`&&`) and OR (`||`). On the other side of things supported value
operators are next:
 * GreaterThan - `>>`
 * GreaterThanOrEqual -`>=`
 * LessThan - `<<`
 * LessThanOrEqual - `<=`
 * Equal - `==`
 * NotEqual - `!=`
 * StartsWith - `@=`
 * EndsWith - `=@`
 * Contains - `%%`

In order for this to work correctly you'll need to add implementation of `IQueryFilterProvider<T>` which will need
to be registered to `ServiceCollection`. This implementation will contain filter fields which you enable for use in queries. Example implementation can be seen below (based on previous `Blog` example):

```csharp
public class BlogQueryFiltersProvider : IOozeQueryFilterProvider<Blog>
{
    public IEnumerable<IQueryFilterDefinition<Blog>> GetFilters()
    {
        return QueryFilters.CreateFor<Blog>()
            .Add(x => x.Id, name: "Id")
            .Add(x => x.Name, name: "Name")
            .Build();
    }
}

//register Ooze.Query to your service collection and add filter provider for your type
services.AddOozeQuery()
    .AddFilterProvider<BlogQueryFiltersProvider>();
```

After registering required services/dependencies you just need to call `Apply` method on instance of Ooze Handler. Example can be seen below:

```csharp
//lets say we have a simple controller action that will filter posts by hardcoded query
//you can always pass that query from outside as this is just an example
[HttpGet]
public async Task<IActionResult> Get(
    [FromServices]IOozeQueryHandler<Post> handler,
    [FromServices]DatabaseContext db)
{
    IQueryable<Post> queryable = db.Set<Post>();
    var sampleLanguageQuery = "Id >> '3' && Name != 'Test'";
    queryable = handler.Apply(queryable, sampleLanguageQuery);
    //this materialized list now contains posts that meet the filter above
    var materialized = queryable.ToListAsync();

    //map this to some DTO before returning
    return Ok(materialized);
}
```