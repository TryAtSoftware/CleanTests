namespace TryAtSoftware.CleanTests.Core;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TryAtSoftware.CleanTests.Core.Interfaces;
using Xunit;
using Xunit.Abstractions;

public abstract class CleanTest : ICleanTest, IDisposable, IAsyncLifetime
{
    private ServiceProvider? _localDependenciesProvider;
    private IServiceScope? _scope;

    private ServiceProvider? _globalDependenciesProvider;

    protected CleanTest(ITestOutputHelper testOutputHelper)
    {
        this.TestOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    protected ITestOutputHelper TestOutputHelper { get; }

    public IServiceCollection LocalDependenciesCollection { get; } = new ServiceCollection();
    public IServiceCollection GlobalDependenciesCollection { get; } = new ServiceCollection();

    public virtual Task InitializeAsync()
    {
        this.InitializeGlobalDependenciesProvider();
        this.InitializeLocalDependenciesProvider();

        return Task.CompletedTask;
    }

    public virtual Task DisposeAsync() => Task.CompletedTask;

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void InitializeGlobalDependenciesProvider()
    {
        var serviceProviderOptions = ConstructServiceProviderOptions();
        this._globalDependenciesProvider = this.GlobalDependenciesCollection.BuildServiceProvider(serviceProviderOptions);
    }

    protected void InitializeLocalDependenciesProvider()
    {
        var serviceProviderOptions = ConstructServiceProviderOptions();
        this._localDependenciesProvider = this.LocalDependenciesCollection.BuildServiceProvider(serviceProviderOptions);
        this._scope = this._localDependenciesProvider.CreateScope();
    }

    protected TService? GetOptionalGlobalService<TService>()
    {
        Assert.NotNull(this._globalDependenciesProvider);
        return this._globalDependenciesProvider.GetService<TService>();
    }

    protected object? GetOptionalGlobalService(Type serviceType)
    {
        Assert.NotNull(this._globalDependenciesProvider);
        return this._globalDependenciesProvider.GetService(serviceType);
    }

    protected TService GetGlobalService<TService>()
        where TService : notnull
    {
        Assert.NotNull(this._globalDependenciesProvider);
        return this._globalDependenciesProvider.GetRequiredService<TService>();
    }

    protected object GetGlobalService(Type serviceType)
    {
        Assert.NotNull(this._globalDependenciesProvider);
        return this._globalDependenciesProvider.GetRequiredService(serviceType);
    }

    protected IEnumerable<TService> GetServices<TService>()
    {
        Assert.NotNull(this._scope);
        return this._scope.ServiceProvider.GetServices<TService>();
    }

    protected TService GetService<TService>()
        where TService : notnull
    {
        Assert.NotNull(this._scope);
        return this._scope.ServiceProvider.GetRequiredService<TService>();
    }

    protected object GetService(Type serviceType)
    {
        Assert.NotNull(this._scope);
        return this._scope.ServiceProvider.GetRequiredService(serviceType);
    }

    protected TService? GetOptionalService<TService>()
    {
        Assert.NotNull(this._scope);
        return this._scope.ServiceProvider.GetService<TService>();
    }

    protected object? GetOptionalService(Type serviceType)
    {
        Assert.NotNull(this._scope);
        return this._scope.ServiceProvider.GetService(serviceType);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;

        this._scope?.Dispose();
        this._localDependenciesProvider?.Dispose();
        // The global dependencies provider should not be disposed as that would cause the disposal of all dependent utilities and that may be problematic.
        // Its disposal is handled on a different level.
    }
    
    private static ServiceProviderOptions ConstructServiceProviderOptions() => new() { ValidateScopes = true, ValidateOnBuild = true };

}