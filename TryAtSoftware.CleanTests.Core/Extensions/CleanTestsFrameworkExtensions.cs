namespace TryAtSoftware.CleanTests.Core.Extensions;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using TryAtSoftware.Extensions.Collections;
using Xunit.Sdk;

public static class CleanTestsFrameworkExtensions
{
    public static void CopyTo<T>(this ICleanTestInitializationCollection<T> source, ICleanTestInitializationCollection<T> target)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (target is null) throw new ArgumentNullException(nameof(target));

        foreach (var (category, values) in source)
        {
            foreach (var value in values) target.Register(category, value);
        }
    }

    public static ICleanUtilityDescriptor[] Get(this ICleanTestInitializationCollection<ICleanUtilityDescriptor> utilitiesCollection, string category, IEnumerable<string> demands)
    {
        if (utilitiesCollection is null) throw new ArgumentNullException(nameof(utilitiesCollection));
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentNullException(nameof(category));
        if (demands is null) throw new ArgumentNullException(nameof(demands));

        return utilitiesCollection.Get(category).OrEmptyIfNull().IgnoreNullValues().Where(iu => demands.All(iu.ContainsCharacteristic)).ToArray();
    }

    internal static (List<ICleanTestCase> CleanTestCases, List<IXunitTestCase> OtherTestCases) ExtractCleanTestCases(this IEnumerable<IXunitTestCase>? testCases)
    {
        var cleanTestCases = new List<ICleanTestCase>();
        var otherTestCases = new List<IXunitTestCase>();
        foreach (var testCase in testCases.OrEmptyIfNull().IgnoreNullValues())
        {
            if (testCase is ICleanTestCase ctc) cleanTestCases.Add(ctc);
            else otherTestCases.Add(testCase);
        }

        return (cleanTestCases, otherTestCases);
    }

    internal static Assembly? LoadAssemblySafely(string assemblyName)
    {
        try
        {
            return Assembly.Load(assemblyName);
        }
        catch
        {
            return null;
        }
    }

    internal static void ValidateInstantiated<TValue>([NotNull] this TValue? value, string valueName)
    {
        if (value is null) throw new InvalidOperationException($"The '{valueName}' cannot be accessed (it should be instantiated explicitly in advance).");
    }
}