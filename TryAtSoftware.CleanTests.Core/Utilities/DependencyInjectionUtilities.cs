namespace TryAtSoftware.CleanTests.Core.Utilities;

using Microsoft.Extensions.DependencyInjection;

internal static class DependencyInjectionUtilities
{
    internal static ServiceProviderOptions ConstructServiceProviderOptions() => new() { ValidateScopes = true, ValidateOnBuild = true };
}