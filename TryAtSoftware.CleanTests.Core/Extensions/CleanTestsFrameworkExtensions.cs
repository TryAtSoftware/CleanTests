namespace TryAtSoftware.CleanTests.Core.Extensions;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using TryAtSoftware.Extensions.Collections;
using Xunit.v3;

internal static class CleanTestsFrameworkExtensions
{
    public static void CopyTo<T>(this ICleanTestInitializationCollection<T> source, ICleanTestInitializationCollection<T> target)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (target is null) throw new ArgumentNullException(nameof(target));

        foreach (var (category, values) in source)
        {
            target.Register(category);
            foreach (var value in values) target.Register(category, value);
        }
    }

    public static ICleanUtilityDescriptor[] Get(this ICleanTestInitializationCollection<ICleanUtilityDescriptor> utilitiesCollection, string category, IEnumerable<string> demands, Func<ICleanUtilityDescriptor, bool>? filter = null)
    {
        if (utilitiesCollection is null) throw new ArgumentNullException(nameof(utilitiesCollection));
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentNullException(nameof(category));
        if (demands is null) throw new ArgumentNullException(nameof(demands));

        return utilitiesCollection.Get(category).OrEmptyIfNull().IgnoreNullValues().SafeWhere(filter).Where(iu => iu.FulfillsAllDemands(demands)).ToArray();
    }

    internal static bool FulfillsAllDemands(this ICleanUtilityDescriptor utilityDescriptor, IEnumerable<string> demands)
    {
        if (utilityDescriptor is null) throw new ArgumentNullException(nameof(utilityDescriptor));
        if (demands is null) throw new ArgumentNullException(nameof(demands));

        return demands.All(utilityDescriptor.ContainsCharacteristic);
    }

    internal static ICleanTestInitializationCollection<string> ExtractDemands<TAttribute>(this MemberInfo member)
        where TAttribute : BaseDemandsAttribute
    {
        var demands = new CleanTestInitializationCollection<string>();
        foreach (var attribute in member.GetCustomAttributes<TAttribute>())
        {
            var demandsArgument = attribute.Demands;
            var categoryArgument = attribute.Category;

            demands.Register(categoryArgument);
            foreach (var demand in demandsArgument) demands.Register(categoryArgument, demand);
        }

        return demands;
    }

    public static HashSet<string> ExtractRequirements(this MemberInfo member)
    {
        var requirements = new HashSet<string>();
        foreach (var attribute in member.GetCustomAttributes<WithRequirementsAttribute>())
            foreach (var category in attribute.Categories) requirements.Add(category);

        return requirements;
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