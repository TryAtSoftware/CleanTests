namespace Calculator.CleanTests.Utilities;

public class ResourcesManager<TKey, TOptions>
    where TKey : notnull
{
    private readonly object _lockObj = new ();

    private readonly Dictionary<TKey, Queue<int>> _freeResourceIdentifiersMap = new ();
    private readonly Dictionary<int, TKey> _producersMap = new ();
    private int _lastDatabaseId;

    private readonly Func<TOptions, TKey> _keyGenerator;
    private readonly Func<TOptions, int, CancellationToken, Task> _resourcePreparationFunc;
    private readonly Func<int, CancellationToken, Task> _resourceCleanupFunc;

    public ResourcesManager(Func<TOptions, TKey> keyGenerator, Func<TOptions, int, CancellationToken, Task> resourcePreparationFunc, Func<int, CancellationToken, Task> resourceCleanupFunc)
    {
        this._keyGenerator = keyGenerator ?? throw new ArgumentNullException(nameof(keyGenerator));
        this._resourcePreparationFunc = resourcePreparationFunc ?? throw new ArgumentNullException(nameof(resourcePreparationFunc));
        this._resourceCleanupFunc = resourceCleanupFunc ?? throw new ArgumentNullException(nameof(resourceCleanupFunc));
    }
    
    public async Task<int> GetResourceIdAsync(TOptions options, CancellationToken cancellationToken)
    {
        var key = this._keyGenerator(options);

        var requiresPreparation = false;
        int resourceId;
        lock (this._lockObj)
        {
            if (!this._freeResourceIdentifiersMap.ContainsKey(key)) this._freeResourceIdentifiersMap[key] = new Queue<int>();
            if (!this._freeResourceIdentifiersMap[key].TryDequeue(out resourceId))
            {
                requiresPreparation = true;
                resourceId = ++this._lastDatabaseId;
            }

            this._producersMap[resourceId] = key;
        }

        if (requiresPreparation) await this._resourcePreparationFunc(options, resourceId, cancellationToken);
        return resourceId;
    }

    public async Task ReleaseResourceAsync(int resourceId, CancellationToken cancellationToken)
    {
        await this._resourceCleanupFunc(resourceId, cancellationToken);
        
        lock (this._lockObj)
        {
            var producer = this._producersMap[resourceId];
            this._freeResourceIdentifiersMap[producer].Enqueue(resourceId);
            this._producersMap.Remove(resourceId);
        }
    }
}