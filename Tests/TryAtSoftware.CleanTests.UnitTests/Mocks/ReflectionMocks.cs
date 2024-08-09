namespace TryAtSoftware.CleanTests.UnitTests.Mocks;

using System.Reflection;
using NSubstitute;
using TryAtSoftware.Extensions.Reflection.Interfaces;
using Xunit.Abstractions;
using Xunit.Sdk;

internal static class ReflectionMocks
{
    internal static IReflectionAssemblyInfo MockReflectionAssemblyInfo(this Assembly assembly, Dictionary<string, IReflectionTypeInfo> typesMap)
    {
        var originalAssemblyInfo = Reflector.Wrap(assembly);

        var assemblyInfo = Substitute.For<IReflectionAssemblyInfo>();
        assemblyInfo.Assembly.Returns(assembly);
        assemblyInfo.Name.Returns(originalAssemblyInfo.Name);
        assemblyInfo.AssemblyPath.Returns(originalAssemblyInfo.AssemblyPath);
        assemblyInfo.GetCustomAttributes(Arg.Any<string>()).Returns(x => originalAssemblyInfo.GetCustomAttributes(x.ArgAt<string>(position: 0)));
        assemblyInfo.GetTypes(Arg.Any<bool>()).Returns(_ => typesMap.Values);
        assemblyInfo.GetType(Arg.Any<string>()).Returns(x => typesMap.TryGetValue(x.ArgAt<string>(position: 0), out var type) ? type : null!);

        return assemblyInfo;
    }

    internal static IReflectionTypeInfo MockReflectionTypeInfo(this Type type, IAssemblyInfo assemblyInfo, Dictionary<string, IMethodInfo>? methodsMap = null)
    {
        var originalTypeInfo = Reflector.Wrap(type);

        var typeInfo = Substitute.For<IReflectionTypeInfo>();
        typeInfo.Type.Returns(type);
        typeInfo.Assembly.Returns(assemblyInfo);
        typeInfo.Name.Returns(originalTypeInfo.Name);
        typeInfo.Interfaces.Returns(originalTypeInfo.Interfaces);
        typeInfo.BaseType.Returns(originalTypeInfo.BaseType);
        typeInfo.IsAbstract.Returns(originalTypeInfo.IsAbstract);
        typeInfo.IsSealed.Returns(originalTypeInfo.IsSealed);
        typeInfo.IsGenericParameter.Returns(originalTypeInfo.IsGenericParameter);
        typeInfo.IsGenericType.Returns(originalTypeInfo.IsGenericType);
        typeInfo.IsValueType.Returns(originalTypeInfo.IsValueType);
        typeInfo.GetCustomAttributes(Arg.Any<string>()).Returns(x => originalTypeInfo.GetCustomAttributes(x.ArgAt<string>(position: 0)));
        typeInfo.GetGenericArguments().Returns(_ => originalTypeInfo.GetGenericArguments());

        if (methodsMap is not null)
        {
            typeInfo.GetMethod(Arg.Any<string>(), Arg.Any<bool>()).Returns((x) => methodsMap.TryGetValue(x.ArgAt<string>(position: 0), out var method) ? method : null!);
            typeInfo.GetMethods(Arg.Any<bool>()).Returns(_ => methodsMap.Values);
        }
        else
        {
            typeInfo.GetMethod(Arg.Any<string>(), Arg.Any<bool>()).Returns(x => originalTypeInfo.GetMethod(x.ArgAt<string>(position: 0), x.ArgAt<bool>(position: 1)));
            typeInfo.GetMethods(Arg.Any<bool>()).Returns(x => originalTypeInfo.GetMethods(x.ArgAt<bool>(position: 0)));
        }

        return typeInfo;
    }

    internal static IReflectionMethodInfo MockReflectionMethodInfo(this MethodInfo method, ITypeInfo type, Dictionary<string, IReflectionAttributeInfo>? attributesMap = null)
    {
        var originalMethodInfo = Reflector.Wrap(method);

        var methodInfo = Substitute.For<IReflectionMethodInfo>();
        methodInfo.MethodInfo.Returns(method);
        methodInfo.Type.Returns(type);
        methodInfo.Name.Returns(originalMethodInfo.Name);
        methodInfo.IsGenericMethodDefinition.Returns(originalMethodInfo.IsGenericMethodDefinition);
        methodInfo.IsAbstract.Returns(originalMethodInfo.IsAbstract);
        methodInfo.IsPublic.Returns(originalMethodInfo.IsPublic);
        methodInfo.IsStatic.Returns(originalMethodInfo.IsStatic);
        methodInfo.ReturnType.Returns(originalMethodInfo.ReturnType);
        methodInfo.GetParameters().Returns(_ => originalMethodInfo.GetParameters());
        methodInfo.GetGenericArguments().Returns(_ => originalMethodInfo.GetGenericArguments());
        methodInfo.MakeGenericMethod(Arg.Any<ITypeInfo[]>()).Returns(x => originalMethodInfo.MakeGenericMethod(x.ArgAt<ITypeInfo[]>(position: 0)));
        
        if (attributesMap is not null) methodInfo.GetCustomAttributes(Arg.Any<string>()).Returns(x => attributesMap.TryGetValue(x.ArgAt<string>(position: 0), out var attribute) ? [attribute] : []);
        else methodInfo.GetCustomAttributes(Arg.Any<string>()).Returns(x => originalMethodInfo.GetCustomAttributes(x.ArgAt<string>(position: 0)));

        return methodInfo;
    }

    internal static IReflectionAttributeInfo MockReflectionAttributeInfo(this FactAttribute attribute)
    {
        var attributeTypeInfo = Reflector.Wrap(attribute.GetType());

        var attributeInfo = Substitute.For<IReflectionAttributeInfo>();
        attributeInfo.Attribute.Returns(attribute);
        attributeInfo.GetCustomAttributes(Arg.Any<string>()).Returns(x => attributeTypeInfo.GetCustomAttributes(x.ArgAt<string>(position: 0)));
        attributeInfo.GetConstructorArguments().Returns(_ => []);
        attributeInfo.GetNamedArgument<string>(nameof(FactAttribute.DisplayName)).Returns(_ => attribute.DisplayName);
        attributeInfo.GetNamedArgument<string>(nameof(FactAttribute.Skip)).Returns<string>(_ => attribute.Skip);
        attributeInfo.GetNamedArgument<int>(nameof(FactAttribute.Timeout)).Returns(_ => attribute.Timeout);

        return attributeInfo;
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
        var hierarchyScanner = Substitute.For<IHierarchyScanner>();
        hierarchyScanner.ScanForAttribute<TAttribute>(Arg.Is<MemberInfo>(mi => mi .ReflectedType == type && mi.Name == memberName)).Returns(_ => attributes);
        return hierarchyScanner;
    } 
}