using System.Collections.Concurrent;

namespace Scase.Application.DataAccess;

public class GenericRepository<TIdentifier, T> where T : class
{
    private readonly ConcurrentDictionary<TIdentifier, T> _list;

    public GenericRepository()
    {
        _list = new ConcurrentDictionary<TIdentifier, T>();
    }


    public async Task<T?> Get(TIdentifier id, CancellationToken cancellationToken)
    {
        _list.TryGetValue(id, out var result);
        return await Task.FromResult(result);
    }

    public async Task<T> Set(TIdentifier id, T item, CancellationToken cancellationToken)
    {
        _list.AddOrUpdate(id, item, (key, oldValue) => item);
        await Task.CompletedTask;
        return item;
    }
}
