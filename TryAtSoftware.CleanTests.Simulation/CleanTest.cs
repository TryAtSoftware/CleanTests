namespace TryAtSoftware.CleanTests.Simulation;

using Microsoft.Extensions.DependencyInjection;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Core.Interfaces;

[TestSuite]
public class CleanTest : ICleanTest, IDisposable, IAsyncLifetime
{
    private ServiceProvider? _localDependenciesProvider;
    private IServiceScope? _scope;
    
    private ServiceProvider? _globalDependenciesProvider;
    
    public IServiceCollection LocalDependenciesCollection { get; } = new ServiceCollection();
    public IServiceCollection GlobalDependenciesCollection { get; } = new ServiceCollection();

    public virtual Task InitializeAsync()
    {
        var serviceProviderOptions = new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true };
        this._globalDependenciesProvider = this.GlobalDependenciesCollection.BuildServiceProvider(serviceProviderOptions);
        
        this._localDependenciesProvider = this.LocalDependenciesCollection.BuildServiceProvider(serviceProviderOptions);
        this._scope = this._localDependenciesProvider.CreateScope();

        return Task.CompletedTask;
    }
    
    protected TService GetGlobalService<TService>()
        where TService : notnull
    {
        Assert.NotNull(this._globalDependenciesProvider);
        return this._globalDependenciesProvider.GetRequiredService<TService>();
    }

    protected TService GetService<TService>()
        where TService : notnull
    {
        Assert.NotNull(this._scope);
        return this._scope.ServiceProvider.GetRequiredService<TService>();
    }

    public virtual Task DisposeAsync() => Task.CompletedTask;

    protected virtual void Dispose(bool disposing)
    {
        if (disposing == false) return;

        this._scope?.Dispose();
        this._localDependenciesProvider?.Dispose();
        // The global dependencies provider should not be disposed as that would cause the disposal of all dependent utilities and that may be problematic.
        // Its disposal is handled on a different level.
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }
}