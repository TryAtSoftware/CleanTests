namespace TryAtSoftware.CleanTests.Core.XUnit.Discovery;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Core.Construction;
using TryAtSoftware.CleanTests.Core.Extensions;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit.Extensions;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using TryAtSoftware.Extensions.Collections;
using TryAtSoftware.Extensions.Reflection;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

internal class CleanTestFrameworkDiscoverer : XunitTestFrameworkDiscoverer
{
    private readonly FallbackTestFrameworkDiscoverer _fallbackTestFrameworkDiscoverer;
    private readonly CleanTestAssemblyData _cleanTestAssemblyData;
    private readonly IConstructionManager _constructionManager;

    public CleanTestFrameworkDiscoverer(CleanTestAssemblyData assemblyData, IXunitTestAssembly testAssembly)
        : base(testAssembly)
    {
        this._cleanTestAssemblyData = assemblyData ?? throw new ArgumentNullException(nameof(assemblyData));
        this._fallbackTestFrameworkDiscoverer = new FallbackTestFrameworkDiscoverer(assemblyInfo, sourceProvider, diagnosticMessageSink);
        this._constructionManager = new ConstructionManager(this._cleanTestAssemblyData);
        this.DisposalTracker.Add(this._fallbackTestFrameworkDiscoverer);
    }

    /// <inheritdoc />
    protected override ValueTask<IXunitTestClass> CreateTestClass(Type @class)
    {
        if (@class.IsCleanTest() && @class.IsGenericType)
        {
            try
            {
                var genericTypesMap = ExtractGenericTypesMap(@class);
                var genericTypesSetup = @class.ExtractGenericParametersSetup(genericTypesMap);
                @class = @class.MakeGenericType(genericTypesSetup);
            }
            catch (Exception e)
            {
                TestContext.Current.SendDiagnosticMessage("Exception occurred while trying to build the generic type {Type}: {Message}", TypeNames.Get(@class), e.Message);
                return ValueTask.FromResult<IXunitTestClass>(null!);
            }
        }

        return base.CreateTestClass(@class);
    }

    /// <inheritdoc />
    protected override ValueTask<bool> FindTestsForType(IXunitTestClass? testClass, ITestFrameworkDiscoveryOptions discoveryOptions, Func<ITestCase, ValueTask<bool>> discoveryCallback)
    {
        if (testClass is null) return ValueTask.FromResult(true);
        return this.FindTests(testClass, discoveryOptions, discoveryCallback);
    }

    private ValueTask<bool> FindTests(IXunitTestClass testClass, ITestFrameworkDiscoveryOptions discoveryOptions, Func<ITestCase, ValueTask<bool>> discoveryCallback)
    {
        var genericTypesMap = ExtractGenericTypesMap(testClass.Class);
        var testCaseDiscoveryOptions = new TestCaseDiscoveryOptions(genericTypesMap);

        var isCleanTestClass = testClass.Class.IsCleanTest();
        var globalRequirements = testClass.Class.ExtractRequirements();

        var options = (includeSourceInformation, messageBus, discoveryOptions, testCaseDiscoveryOptions, isCleanTestClass, globalRequirements);
        foreach (var methodInfo in testClass.Class.GetMethods(includePrivateMethods: false).OrEmptyIfNull().IgnoreNullValues())
        {
            var testMethod = new TestMethod(testClass, methodInfo);
            this.DiscoverTestCasesForMethod(testMethod, options);
        }
    }

    private void DiscoverTestCasesForMethod(IXunitTestMethod testMethod, (bool IncludeSourceInformation, IMessageBus messageBus, ITestFrameworkDiscoveryOptions TestFrameworkDiscoveryOptions, TestCaseDiscoveryOptions TestCaseDiscoveryOptions, bool IsCleanTestClass, HashSet<string> GlobalRequirements) options)
    {
        var methodAttributeContainer = new DecoratedMethod(testMethod.Method);
        if (!methodAttributeContainer.TryGetSingleAttribute(typeof(FactAttribute), out var factAttribute)) return;

        var factAttributeAttributeContainer = new DecoratedAttribute(factAttribute);
        if (!options.IsCleanTestClass || !factAttributeAttributeContainer.TryGetSingleAttribute(typeof(CleanTestCaseDiscovererAttribute), out var cleanTestCaseDiscovererAttribute))
        {
            this._fallbackTestFrameworkDiscoverer.DiscoverFallbackTests(testMethod, options.IncludeSourceInformation, options.messageBus, options.TestFrameworkDiscoveryOptions);
            return;
        }

        var testCaseDiscovererType = cleanTestCaseDiscovererAttribute.GetNamedArgument<Type>(nameof(CleanTestCaseDiscovererAttribute.DiscovererType));
        if (testCaseDiscovererType is null) return;

        var customInitializationUtilitiesCollection = this.GetInitializationUtilities(methodAttributeContainer, options.GlobalRequirements);
        var testCaseDiscoverer = Activator.CreateInstance(testCaseDiscovererType, options.TestCaseDiscoveryOptions, customInitializationUtilitiesCollection, this._constructionManager, this._cleanTestAssemblyData) as IXunitTestCaseDiscoverer;

        if (testCaseDiscoverer is null) return;

        var testCases = testCaseDiscoverer.Discover(options.TestFrameworkDiscoveryOptions, testMethod, factAttribute);
        foreach (var testCase in testCases.OrEmptyIfNull().IgnoreNullValues())
            this.ReportDiscoveredTestCase(testCase, options.IncludeSourceInformation, options.messageBus);
    }

    private CleanTestInitializationCollection<ICleanUtilityDescriptor> GetInitializationUtilities(MethodInfo? method, HashSet<string> globalRequirements)
    {
        var customInitializationUtilitiesCollection = new CleanTestInitializationCollection<ICleanUtilityDescriptor>();
        if (method is null) return customInitializationUtilitiesCollection;

        var demands = method.ExtractDemands<TestDemandsAttribute>();
        var initializationRequirements = method.ExtractRequirements();

        var allRequirementSources = new[] { initializationRequirements, globalRequirements };
        foreach (var category in allRequirementSources.Union())
        {
            customInitializationUtilitiesCollection.Register(category);

            var categoryDemands = demands.Get(category);
            foreach (var initializationUtility in this._cleanTestAssemblyData.CleanUtilities.Get(category, categoryDemands)) customInitializationUtilitiesCollection.Register(category, initializationUtility);
        }

        return customInitializationUtilitiesCollection;
    }

    private static IDictionary<Type, Type> ExtractGenericTypesMap(Type @class)
    {
        var genericTypeMappingAttributes = @class.GetCustomAttributes<TestSuiteGenericTypeMappingAttribute>();
        return genericTypeMappingAttributes.MapSafely(a => a.AttributeType, a => a.ParameterType);
    }
}