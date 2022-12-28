namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;
using System.Collections.Generic;
using TryAtSoftware.Extensions.Collections;

/// <summary>
/// An attribute that should be used to decorate our clean tests in order to specify a concrete demand for the initialization utilities of a specific category.
/// </summary>
/// <remarks>
/// Each initialization utility can define its own characteristics.
/// Then, we can use these characteristics to filter out on some basis the initialization utilities that we want to use.
/// Demands (that is an interchangeable term for an initialization utility's characteristics) do often correspond to essential segments of the requested component's behavior.
/// We use demands to make sure that the capabilities our test needs are present on the resolved initialization utilities used to execute the test.
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class DemandsAttribute : Attribute
{
    public string Category { get; }
    public IReadOnlyCollection<string> Demands { get; }

    public DemandsAttribute(string category, params string[] demands)
    {
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentNullException(nameof(category));
        this.Category = category;
        this.Demands = demands.OrEmptyIfNull().IgnoreNullOrWhitespaceValues().AsReadOnlyCollection();
    }
}