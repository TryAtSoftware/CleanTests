namespace TryAtSoftware.CleanTests.UnitTests.Mocks;

using System.Reflection;
using Moq;
using TryAtSoftware.Extensions.Reflection.Interfaces;
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

    internal static IReflectionTypeInfo MockReflectionTypeInfo(this Type type, IAssemblyInfo assemblyInfo, Dictionary<string, IMethodInfo>? methodsMap = null)
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

        if (methodsMap is not null)
        {
            typeInfoMock.Setup(x => x.GetMethod(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>((x, _) => methodsMap.TryGetValue(x, out var method) ? method : null!);
            typeInfoMock.Setup(x => x.GetMethods(It.IsAny<bool>())).Returns<bool>(_ => methodsMap.Values);
        }
        else
        {
            typeInfoMock.Setup(x => x.GetMethod(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>((x, y) => originalTypeInfo.GetMethod(x, y));
            typeInfoMock.Setup(x => x.GetMethods(It.IsAny<bool>())).Returns<bool>(x => originalTypeInfo.GetMethods(x));
        }

        return typeInfoMock.Object;
    }

    internal static IReflectionMethodInfo MockReflectionMethodInfo(this MethodInfo method, ITypeInfo type, Dictionary<string, IReflectionAttributeInfo>? attributesMap = null)
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
        
        if (attributesMap is not null) methodInfoMock.Setup(x => x.GetCustomAttributes(It.IsAny<string>())).Returns<string>(x => attributesMap.TryGetValue(x, out var attribute) ? new[] { attribute } : Enumerable.Empty<IAttributeInfo>());
        else  methodInfoMock.Setup(x => x.GetCustomAttributes(It.IsAny<string>())).Returns<string>(x => originalMethodInfo.GetCustomAttributes(x));

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

    internal static ReflectionMocksSuite MockReflectionSuite(Assembly assembly, Type type)
    {
        Assert.NotNull(type.AssemblyQualifiedName);
        
        var typesMap = new Dictionary<string, IReflectionTypeInfo>();
        var assemblyInfo = assembly.MockReflectionAssemblyInfo(typesMap);

        var typeInfo = type.MockReflectionTypeInfo(assemblyInfo);
        typesMap[type.AssemblyQualifiedName] = typeInfo;

        return new ReflectionMocksSuite(assemblyInfo, typeInfo);
    }

    internal static IHierarchyScanner MockHierarchyScanner<TAttribute>(Type type, string memberName, TAttribute[] attributes)
        where TAttribute : Attribute
    {
        var mockedHierarchyScanner = new Mock<IHierarchyScanner>();
        mockedHierarchyScanner.Setup(x => x.ScanForAttribute<TAttribute>(It.Is<MemberInfo>(mi => mi .ReflectedType == type && mi.Name == memberName))).Returns<MemberInfo>(_ => attributes);
        return mockedHierarchyScanner.Object;
    } 
}