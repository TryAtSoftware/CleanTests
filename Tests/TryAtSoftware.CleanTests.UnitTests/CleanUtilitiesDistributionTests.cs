namespace TryAtSoftware.CleanTests.UnitTests;

using System.Reflection;
using Moq;
using TryAtSoftware.CleanTests.Core;
using TryAtSoftware.CleanTests.Core.Internal;
using TryAtSoftware.CleanTests.Core.XUnit;
using TryAtSoftware.CleanTests.Core.XUnit.Discovery;
using TryAtSoftware.CleanTests.Core.XUnit.Execution;
using TryAtSoftware.CleanTests.UnitTests.Mocks;
using Xunit.Abstractions;
using Xunit.Sdk;

public class CleanUtilitiesDistributionTests
{
    [Fact]
    public async Task GlobalUtilitiesShouldBeDistributedSuccessfully()
    {
        var typeInfo = Reflector.Wrap(typeof(ClassWithTests));
        var originalMethodInfo = typeInfo.GetMethod(nameof(ClassWithTests.Test), includePrivateMethod: false);
        
        var attribute = new FactAttribute();
        var attributeTypeInfo = Reflector.Wrap(attribute.GetType());
        var attributeInfoMock = new Mock<IAttributeInfo>();
        attributeInfoMock.Setup(x => x.GetCustomAttributes(It.IsAny<string>())).Returns<string>(x => attributeTypeInfo.GetCustomAttributes(x));
        attributeInfoMock.Setup(x => x.GetConstructorArguments()).Returns(Enumerable.Empty<object>);
        attributeInfoMock.Setup(x => x.GetNamedArgument<string>(nameof(FactAttribute.DisplayName))).Returns<string>(_ => attribute.DisplayName);
        attributeInfoMock.Setup(x => x.GetNamedArgument<string>(nameof(FactAttribute.Skip))).Returns<string>(_ => attribute.Skip);
        attributeInfoMock.Setup(x => x.GetNamedArgument<int>(nameof(FactAttribute.Timeout))).Returns<string>(_ => attribute.Timeout);
        var attributeInfo = attributeInfoMock.Object;

        var methodInfoMock = new Mock<IReflectionMethodInfo>();
        methodInfoMock.SetupGet(x => x.MethodInfo).Returns(typeof(ClassWithTests).GetMethod(nameof(ClassWithTests.Test))!);
        methodInfoMock.SetupGet(x => x.Name).Returns(originalMethodInfo.Name);
        methodInfoMock.SetupGet(x => x.IsGenericMethodDefinition).Returns(originalMethodInfo.IsGenericMethodDefinition);
        // methodInfoMock.SetupGet(x => x.Type).Returns(originalMethodInfo.Type);
        methodInfoMock.SetupGet(x => x.IsAbstract).Returns(originalMethodInfo.IsAbstract);
        methodInfoMock.SetupGet(x => x.IsPublic).Returns(originalMethodInfo.IsPublic);
        methodInfoMock.SetupGet(x => x.IsStatic).Returns(originalMethodInfo.IsStatic);
        methodInfoMock.SetupGet(x => x.ReturnType).Returns(originalMethodInfo.ReturnType);
        methodInfoMock.Setup(x => x.GetCustomAttributes(It.IsAny<string>())).Returns<string>(x => originalMethodInfo.GetCustomAttributes(x));
        methodInfoMock.Setup(x => x.GetCustomAttributes(typeof(FactAttribute).AssemblyQualifiedName)).Returns<string>(_ => new[] { attributeInfo });
        methodInfoMock.Setup(x => x.GetParameters()).Returns(() => originalMethodInfo.GetParameters());
        methodInfoMock.Setup(x => x.GetGenericArguments()).Returns(() => originalMethodInfo.GetGenericArguments());
        methodInfoMock.Setup(x => x.MakeGenericMethod(It.IsAny<ITypeInfo[]>())).Returns<ITypeInfo[]>(x => originalMethodInfo.MakeGenericMethod(x));
        var methodInfo = methodInfoMock.Object;

        var assemblyInfoMock = new Mock<IAssemblyInfo>();
        assemblyInfoMock.Setup(x => x.GetTypes(It.IsAny<bool>())).Returns<bool>(_ => new[] { typeInfo });
        var assemblyInfo = assemblyInfoMock.Object;
        
        var testAssemblyMock = new Mock<ITestAssembly>();
        testAssemblyMock.SetupGet(x => x.Assembly).Returns(assemblyInfo);
        var testAssembly = testAssemblyMock.Object;

        var testCollectionMock = new Mock<ITestCollection>();
        testCollectionMock.SetupGet(x => x.CollectionDefinition).Returns(() => null!);
        testCollectionMock.SetupGet(x => x.TestAssembly).Returns(testAssembly);
        testCollectionMock.SetupGet(x => x.DisplayName).Returns("__internal__");
        testCollectionMock.SetupGet(x => x.UniqueID).Returns(Guid.NewGuid());
        var testCollection = testCollectionMock.Object;

        var testClassMock = new Mock<ITestClass>();
        testClassMock.SetupGet(x => x.TestCollection).Returns(testCollection);
        testClassMock.SetupGet(x => x.Class).Returns(typeInfo);
        var testClass = testClassMock.Object;

        var testMethodMock = new Mock<ITestMethod>();
        testMethodMock.SetupGet(x => x.TestClass).Returns(testClass);
        testMethodMock.SetupGet(x => x.Method).Returns(methodInfo);
        var testMethod = testMethodMock.Object;

        var diagnosticMessageSinkMock = new Mock<IMessageSink>();
        diagnosticMessageSinkMock.Setup(x => x.OnMessage(It.IsAny<IMessageSinkMessage>())).Returns<IMessageSinkMessage>(_ => true);
        var diagnosticMessageSink = diagnosticMessageSinkMock.Object;
        
        var executionMessageSinkMock = new Mock<IMessageSink>();
        executionMessageSinkMock.Setup(x => x.OnMessage(It.IsAny<IMessageSinkMessage>())).Returns<IMessageSinkMessage>(_ => true);
        var executionMessageSink = executionMessageSinkMock.Object;

        var cleanUtilityDescriptor = new CleanUtilityDescriptor("_", typeof(InconclusiveUtility), "Inconclusive utility", isGlobal: true, characteristics: null, requirements: null);
        var assemblyData = new CleanTestAssemblyData(new[] { cleanUtilityDescriptor })
        {
            MaxDegreeOfParallelism = CleanTestConstants.MaxDegreeOfParallelism,
            UtilitiesPresentations = CleanTestConstants.UtilitiesPresentation,
            GenericTypeMappingPresentations = CleanTestConstants.GenericTypeMappingPresentation
        };

        var testCaseDiscoverer = new TestableTestCaseDiscoverer(diagnosticMessageSink, new TestCaseDiscoveryOptions(), assemblyData.CleanUtilities, assemblyData);
        testCaseDiscoverer.TestMethodArguments = new[] { Array.Empty<object>() };

        var testFrameworkDiscoveryOptionsMock = new Mock<ITestFrameworkDiscoveryOptions>();
        var testFrameworkDiscoveryOptions = testFrameworkDiscoveryOptionsMock.Object;

        var testFrameworkExecutionOptionsMock = new Mock<ITestFrameworkExecutionOptions>();
        var testFrameworkExecutionOptions = testFrameworkExecutionOptionsMock.Object;

        // The fact attribute is not currently used so we can pass null.
        var testCases = testCaseDiscoverer.Discover(testFrameworkDiscoveryOptions, testMethod, factAttribute: null!);
        var cleanTestAssemblyRunner = new CleanTestAssemblyRunner(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, testFrameworkExecutionOptions, assemblyData);

        var executionResult = await cleanTestAssemblyRunner.RunAsync();
        Assert.NotNull(executionResult);
        Assert.Equal(1, executionResult.Failed);
        Assert.Equal(1, executionResult.Total);
    }
    
    public class ClassWithTests : CleanTest
    {
        public ClassWithTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }
        
        public void Test() => this.GetGlobalService<InconclusiveUtility>();
    }

    public class InconclusiveUtility
    {
        public InconclusiveUtility(string unresolvableParameter)
        {
            this.UnresolvableParameter = unresolvableParameter;
        }
        
        public string UnresolvableParameter { get; }
    }
}