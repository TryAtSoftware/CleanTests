namespace TryAtSoftware.CleanTests.Core.XUnit.Wrappers;

using System;
using System.Collections.Generic;
using TryAtSoftware.Extensions.Reflection;
using Xunit.Abstractions;

/// <summary>
/// An implementation of the <see cref="IReflectionTypeInfo"/> interface responsible for beautifying the name of generated generic test classes.
/// </summary>
public class CleanTestReflectionTypeInfoWrapper : IReflectionTypeInfo
{
    private readonly ITypeInfo _wrapped;

    /// <summary>
    /// Initializes a new instance of the <see cref="CleanTestReflectionTypeInfoWrapper"/> class.
    /// </summary>
    /// <param name="wrapped">The wrapped <see cref="ITypeInfo"/> instance.</param>
    public CleanTestReflectionTypeInfoWrapper(ITypeInfo wrapped)
    {
        this._wrapped = wrapped ?? throw new ArgumentNullException(nameof(wrapped));
        this.Type = wrapped.ToRuntimeType();
    }

    /// <inheritdoc/>
    public IAssemblyInfo Assembly => this._wrapped.Assembly;

    /// <inheritdoc/>
    public ITypeInfo BaseType => this._wrapped.BaseType;

    /// <inheritdoc/>
    public IEnumerable<ITypeInfo> Interfaces => this._wrapped.Interfaces;

    /// <inheritdoc/>
    public bool IsAbstract => this._wrapped.IsAbstract;

    /// <inheritdoc/>
    public bool IsGenericParameter => this._wrapped.IsGenericParameter;

    /// <inheritdoc/>
    public bool IsGenericType => this._wrapped.IsGenericType;

    /// <inheritdoc/>
    public bool IsSealed => this._wrapped.IsSealed;

    /// <inheritdoc/>
    public bool IsValueType => this._wrapped.IsValueType;

    /// <inheritdoc/>
    public string Name => $"{this.Type.Namespace}.{this.Type.Name}";

    /// <inheritdoc/>
    public Type Type { get; }

    /// <inheritdoc/>
    public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName) => this._wrapped.GetCustomAttributes(assemblyQualifiedAttributeTypeName);

    /// <inheritdoc/>
    public IEnumerable<ITypeInfo> GetGenericArguments() => this._wrapped.GetGenericArguments();

    /// <inheritdoc/>
    public IMethodInfo GetMethod(string methodName, bool includePrivateMethod) => this._wrapped.GetMethod(methodName, includePrivateMethod);

    /// <inheritdoc/>
    public IEnumerable<IMethodInfo> GetMethods(bool includePrivateMethods) => this._wrapped.GetMethods(includePrivateMethods);

    /// <inheritdoc/>
    public override string ToString() => $"{this.Type.Namespace}.{TypeNames.Get(this.Type)}";
}