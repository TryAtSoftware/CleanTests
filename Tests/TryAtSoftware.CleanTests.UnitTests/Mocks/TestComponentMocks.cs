namespace TryAtSoftware.CleanTests.UnitTests.Mocks;

using NSubstitute;
using NSubstitute.Core.DependencyInjection;
using NSubstitute.ReturnsExtensions;
using Xunit.Abstractions;

internal static class TestComponentMocks
{
    internal static ITestAssembly MockTestAssembly(this IReflectionAssemblyInfo assemblyInfo)
    {
        var testAssembly = Substitute.For<ITestAssembly>();
        testAssembly.Assembly.Returns(assemblyInfo);
        return testAssembly;
    }

    internal static ITestCollection MockTestCollection(this ITestAssembly testAssembly)
    {
        var testCollection = Substitute.For<ITestCollection>();
        testCollection.CollectionDefinition.Returns(_ => null!);
        testCollection.TestAssembly.Returns(testAssembly);
        testCollection.DisplayName.Returns("__internal__");
        testCollection.UniqueID.Returns(Guid.NewGuid());
        return testCollection;
    }

    internal static ITestClass MockTestClass(this ITestCollection testCollection, IReflectionTypeInfo typeInfo)
    {
        var testClass = Substitute.For<ITestClass>();
        testClass.TestCollection.Returns(testCollection);
        testClass.Class.Returns(typeInfo);
        return testClass;
    }

    internal static ITestMethod MockTestMethod(this ITestClass testClass, IReflectionMethodInfo methodInfo)
    {
        var testMethod = Substitute.For<ITestMethod>();
        testMethod.TestClass.Returns(testClass);
        testMethod.Method.Returns(methodInfo);
        return testMethod;
    }

    internal static TestComponentMocksSuite MockTestComponentsSuite(this ReflectionMocksSuite reflectionMocks)
    {
        var testAssembly = reflectionMocks.AssemblyInfo.MockTestAssembly();
        var testCollection = testAssembly.MockTestCollection();
        var testClass = testCollection.MockTestClass(reflectionMocks.TypeInfo);

        return new TestComponentMocksSuite
        {
            TestAssembly = testAssembly,
            TestCollection = testCollection,
            TestClass = testClass,
            DiagnosticMessageSink = MockMessageSink(),
            ExecutionMessageSink = MockMessageSink(),
            SourceInformationProvider = MockSourceInformationProvider(),
            TestFrameworkDiscoveryOptions = MockTestFrameworkDiscoveryOptions(),
            TestFrameworkExecutionOptions = MockTestFrameworkExecutionOptions()
        };
    }

    internal static IMessageSink MockMessageSink()
    {
        var messageSink = Substitute.For<IMessageSink>();
        messageSink.OnMessage(Arg.Any<IMessageSinkMessage>()).Returns(true);
        return messageSink;
    }

    internal static ITestFrameworkDiscoveryOptions MockTestFrameworkDiscoveryOptions()
    {
        var discoveryOptions = Substitute.For<ITestFrameworkDiscoveryOptions>();
        discoveryOptions.GetValue<string>(Arg.Any<string>()).ReturnsNull();
        return discoveryOptions;
    }

    internal static ITestFrameworkExecutionOptions MockTestFrameworkExecutionOptions()
    {
        var executionOptions = Substitute.For<ITestFrameworkExecutionOptions>();
        executionOptions.GetValue<string>(Arg.Any<string>()).ReturnsNull();
        return executionOptions;
    }

    internal static ISourceInformationProvider MockSourceInformationProvider() => Substitute.For<ISourceInformationProvider>();
}