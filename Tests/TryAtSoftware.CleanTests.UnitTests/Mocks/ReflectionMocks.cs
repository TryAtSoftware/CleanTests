namespace TryAtSoftware.CleanTests.UnitTests.Mocks;

using System.Reflection;
using Moq;
using Xunit.Abstractions;
using Xunit.Sdk;

internal static class ReflectionMocks
{
    internal static IReflectionAssemblyInfo MockReflectionAssemblyInfo(this Assembly assembly, Dictionary<string, IReflectionTypeInfo> typesMap)
    {
        var originalAssemblyInfo = Reflector.Wrap(assembly);

        var assemblyInfoMock = new Mock<IReflectionAssemblyInfo>();
        assemblyInfoMock.SetupGet(x => x.Assembly).Returns(assembly);
        assemblyInfoMock.SetupGet(x => x.Name).Returns(originalAssemblyInfo.Name);
        assemblyInfoMock.SetupGet(x => x.AssemblyPath).Returns(originalAssemblyInfo.AssemblyPath);
        assemblyInfoMock.Setup(x => x.GetCustomAttributes(It.IsAny<string>())).Returns<string>(x => originalAssemblyInfo.GetCustomAttributes(x));
        assemblyInfoMock.Setup(x => x.GetTypes(It.IsAny<bool>())).Returns<bool>(_ => typesMap.Values);
        assemblyInfoMock.Setup(x => x.GetType(It.IsAny<string>())).Returns<string>(name => typesMap.TryGetValue(name, out var type) ? type : null!);

        return assemblyInfoMock.Object;
    }

    internal static IReflectionTypeInfo MockReflectionTypeInfo(this Type type, IAssemblyInfo assemblyInfo, Dictionary<string, IMethodInfo> methodsMap)
    {
        var originalTypeInfo = Reflector.Wrap(type);

        var typeInfoMock = new Mock<IReflectionTypeInfo>();
        typeInfoMock.SetupGet(x => x.Type).Returns(type);
        typeInfoMock.SetupGet(x => x.Assembly).Returns(assemblyInfo);
        typeInfoMock.SetupGet(x => x.Name).Returns(originalTypeInfo.Name);
        typeInfoMock.SetupGet(x => x.Interfaces).Returns(originalTypeInfo.Interfaces);
        typeInfoMock.SetupGet(x => x.BaseType).Returns(originalTypeInfo.BaseType);
        typeInfoMock.SetupGet(x => x.IsAbstract).Returns(originalTypeInfo.IsAbstract);
        typeInfoMock.SetupGet(x => x.IsSealed).Returns(originalTypeInfo.IsSealed);
        typeInfoMock.SetupGet(x => x.IsGenericParameter).Returns(originalTypeInfo.IsGenericParameter);
        typeInfoMock.SetupGet(x => x.IsGenericType).Returns(originalTypeInfo.IsGenericType);
        typeInfoMock.SetupGet(x => x.IsValueType).Returns(originalTypeInfo.IsValueType);
        typeInfoMock.Setup(x => x.GetCustomAttributes(It.IsAny<string>())).Returns<string>(x => originalTypeInfo.GetCustomAttributes(x));
        typeInfoMock.Setup(x => x.GetGenericArguments()).Returns(() => originalTypeInfo.GetGenericArguments());
        typeInfoMock.Setup(x => x.GetMethod(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>((x, y) => methodsMap.TryGetValue(x, out var method) ? method : null!);
        typeInfoMock.Setup(x => x.GetMethods(It.IsAny<bool>())).Returns<bool>(_ => methodsMap.Values);

        return typeInfoMock.Object;
    }

    internal static IReflectionMethodInfo MockReflectionMethodInfo(this MethodInfo method, ITypeInfo type, Dictionary<string, IReflectionAttributeInfo> attributesMap)
    {
        var originalMethodInfo = Reflector.Wrap(method);

        var methodInfoMock = new Mock<IReflectionMethodInfo>();
        methodInfoMock.SetupGet(x => x.MethodInfo).Returns(method);
        methodInfoMock.SetupGet(x => x.Type).Returns(type);
        methodInfoMock.SetupGet(x => x.Name).Returns(originalMethodInfo.Name);
        methodInfoMock.SetupGet(x => x.IsGenericMethodDefinition).Returns(originalMethodInfo.IsGenericMethodDefinition);
        methodInfoMock.SetupGet(x => x.IsAbstract).Returns(originalMethodInfo.IsAbstract);
        methodInfoMock.SetupGet(x => x.IsPublic).Returns(originalMethodInfo.IsPublic);
        methodInfoMock.SetupGet(x => x.IsStatic).Returns(originalMethodInfo.IsStatic);
        methodInfoMock.SetupGet(x => x.ReturnType).Returns(originalMethodInfo.ReturnType);
        methodInfoMock.Setup(x => x.GetParameters()).Returns(() => originalMethodInfo.GetParameters());
        methodInfoMock.Setup(x => x.GetGenericArguments()).Returns(() => originalMethodInfo.GetGenericArguments());
        methodInfoMock.Setup(x => x.MakeGenericMethod(It.IsAny<ITypeInfo[]>())).Returns<ITypeInfo[]>(x => originalMethodInfo.MakeGenericMethod(x));
        methodInfoMock.Setup(x => x.GetCustomAttributes(It.IsAny<string>())).Returns<string>(name => attributesMap.TryGetValue(name, out var attribute) ? new[] { attribute } : Enumerable.Empty<IAttributeInfo>());

        return methodInfoMock.Object;
    }

    internal static IReflectionAttributeInfo MockReflectionAttributeInfo(this FactAttribute attribute)
    {
        var attributeTypeInfo = Reflector.Wrap(attribute.GetType());

        var attributeInfoMock = new Mock<IReflectionAttributeInfo>();
        attributeInfoMock.SetupGet(x => x.Attribute).Returns(attribute);
        attributeInfoMock.Setup(x => x.GetCustomAttributes(It.IsAny<string>())).Returns<string>(x => attributeTypeInfo.GetCustomAttributes(x));
        attributeInfoMock.Setup(x => x.GetConstructorArguments()).Returns(Enumerable.Empty<object>);
        attributeInfoMock.Setup(x => x.GetNamedArgument<string>(nameof(FactAttribute.DisplayName))).Returns<string>(_ => attribute.DisplayName);
        attributeInfoMock.Setup(x => x.GetNamedArgument<string>(nameof(FactAttribute.Skip))).Returns<string>(_ => attribute.Skip);
        attributeInfoMock.Setup(x => x.GetNamedArgument<int>(nameof(FactAttribute.Timeout))).Returns<string>(_ => attribute.Timeout);

        return attributeInfoMock.Object;
    }

    internal static ReflectionMocksSuite MockReflectionSuite(Assembly assembly, Type type, string methodName, FactAttribute attribute)
    {
        Assert.NotNull(type.AssemblyQualifiedName);
        
        var method = type.GetMethod(methodName);
        Assert.NotNull(method);

        var factAttributeAssemblyQualifiedName = typeof(FactAttribute).AssemblyQualifiedName;
        Assert.NotNull(factAttributeAssemblyQualifiedName);
        
        var typesMap = new Dictionary<string, IReflectionTypeInfo>();
        var assemblyInfo = assembly.MockReflectionAssemblyInfo(typesMap);

        var methodsMap = new Dictionary<string, IMethodInfo>();
        var typeInfo = type.MockReflectionTypeInfo(assemblyInfo, methodsMap);
        typesMap[type.AssemblyQualifiedName] = typeInfo;

        var attributesMap = new Dictionary<string, IReflectionAttributeInfo>();
        var methodInfo = method.MockReflectionMethodInfo(typeInfo, attributesMap);
        methodsMap[methodInfo.Name] = methodInfo;
        
        var attributeInfo = attribute.MockReflectionAttributeInfo();
        attributesMap[factAttributeAssemblyQualifiedName] = attributeInfo;

        return new ReflectionMocksSuite(assemblyInfo, typeInfo, methodInfo, attributeInfo);
    }
}