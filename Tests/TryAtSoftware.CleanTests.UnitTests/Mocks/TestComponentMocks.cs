namespace TryAtSoftware.CleanTests.UnitTests.Mocks;

using Moq;
using Xunit.Abstractions;

internal static class TestComponentMocks
{
    internal static ITestAssembly MockTestAssembly(this IReflectionAssemblyInfo assemblyInfo)
    {
        var testAssemblyMock = new Mock<ITestAssembly>();
        testAssemblyMock.SetupGet(x => x.Assembly).Returns(assemblyInfo);
        return testAssemblyMock.Object;
    }

    internal static ITestCollection MockTestCollection(this ITestAssembly testAssembly)
    {
        var testCollectionMock = new Mock<ITestCollection>();
        testCollectionMock.SetupGet(x => x.CollectionDefinition).Returns(() => null!);
        testCollectionMock.SetupGet(x => x.TestAssembly).Returns(testAssembly);
        testCollectionMock.SetupGet(x => x.DisplayName).Returns("__internal__");
        testCollectionMock.SetupGet(x => x.UniqueID).Returns(Guid.NewGuid());
        return testCollectionMock.Object;
    }

    internal static ITestClass MockTestClass(this ITestCollection testCollection, IReflectionTypeInfo typeInfo)
    {
        var testClassMock = new Mock<ITestClass>();
        testClassMock.SetupGet(x => x.TestCollection).Returns(testCollection);
        testClassMock.SetupGet(x => x.Class).Returns(typeInfo);
        return testClassMock.Object;
    }

    internal static ITestMethod MockTestMethod(this ITestClass testClass, IReflectionMethodInfo methodInfo)
    {
        var testMethodMock = new Mock<ITestMethod>();
        testMethodMock.SetupGet(x => x.TestClass).Returns(testClass);
        testMethodMock.SetupGet(x => x.Method).Returns(methodInfo);
        return testMethodMock.Object;
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
        var messageSinkMock = new Mock<IMessageSink>();
        messageSinkMock.Setup(x => x.OnMessage(It.IsAny<IMessageSinkMessage>())).Returns<IMessageSinkMessage>(_ => true);
        return messageSinkMock.Object;
    }

    internal static ITestFrameworkDiscoveryOptions MockTestFrameworkDiscoveryOptions()
    {
        var testFrameworkDiscoveryOptionsMock = new Mock<ITestFrameworkDiscoveryOptions>();
        return testFrameworkDiscoveryOptionsMock.Object;
    }

    internal static ITestFrameworkExecutionOptions MockTestFrameworkExecutionOptions()
    {
        var testFrameworkExecutionOptionsMock = new Mock<ITestFrameworkExecutionOptions>();
        return testFrameworkExecutionOptionsMock.Object;
    }

    internal static ISourceInformationProvider MockSourceInformationProvider()
    {
        var sourceInformationProviderMock = new Mock<ISourceInformationProvider>();
        return sourceInformationProviderMock.Object;
    }
}