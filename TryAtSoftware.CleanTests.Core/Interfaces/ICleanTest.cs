namespace TryAtSoftware.CleanTests.Core.Interfaces;

using Microsoft.Extensions.DependencyInjection;

public interface ICleanTest
{
    IServiceCollection LocalDependenciesCollection { get; }
    IServiceCollection GlobalDependenciesCollection { get; }
}