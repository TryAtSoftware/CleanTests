namespace Calculator.CleanTests.Utilities.ApiProviders;

using System.Collections.Concurrent;
using Calculator.CleanTests.Constants;
using Calculator.CleanTests.Utilities.Interfaces;
using Microsoft.AspNetCore.Mvc.Testing;
using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(InitializationCategories.ApiProvider, "default_api_provider", IsGlobal = true)]
public class ApiProvider : IApiProvider, IAsyncDisposable, IDisposable
{
    private readonly ResourcesManager<string, Nothing> _resourcesManager;
    private readonly ConcurrentDictionary<int, (WebApplicationFactory<Program> WebApplicationFactory, IApiAccessor ApiAccessor)> _accessorsMap = new ();
    private bool _isDisposed;

    public ApiProvider()
    {
        this._resourcesManager = new ResourcesManager<string, Nothing>(_ => "test_key", this.InitializeResourceAsync, (_, _) => Task.CompletedTask);
    }

    public Task<int> GetResourceIdAsync(Nothing options, CancellationToken cancellationToken) => this._resourcesManager.GetResourceIdAsync(options, cancellationToken);

    public Task ReleaseResourceAsync(int resourceId, CancellationToken cancellationToken) => this._resourcesManager.ReleaseResourceAsync(resourceId, cancellationToken);

    public IApiAccessor GetApiAccessor(int resourceId) => this._accessorsMap[resourceId].ApiAccessor;

    private Task InitializeResourceAsync(Nothing options, int resourceId, CancellationToken cancellationToken)
    {
        this.InitializeResource(resourceId);
        return Task.CompletedTask;
    }

    private void InitializeResource(int resourceId)
    {
        var webApplicationFactory = new WebApplicationFactory<Program>();

        var accessor = new ApiAccessor<Program>(new Uri("http://localhost:5000"), webApplicationFactory);
        this._accessorsMap[resourceId] = (webApplicationFactory, accessor);
    }

    public async ValueTask DisposeAsync()
    {
        if (!this.IsFirstDisposalCall()) return;

        foreach (var (webApplicationFactory, accessor) in this._accessorsMap.Values)
        {
            await webApplicationFactory.DisposeAsync();
            accessor.HttpClient.Dispose();
        }
        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        if (!this.IsFirstDisposalCall()) return;

        foreach (var (webApplicationFactory, accessor) in this._accessorsMap.Values)
        {
            webApplicationFactory.Dispose();
            accessor.HttpClient.Dispose();
        }
        
        GC.SuppressFinalize(this);
    }

    private bool IsFirstDisposalCall()
    {
        if (this._isDisposed) return false;
        this._isDisposed = true;
        return true;
    }
}