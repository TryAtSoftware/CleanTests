namespace TryAtSoftware.CleanTests.Core.XUnit.Wrappers;

using System;
using TryAtSoftware.CleanTests.Core.Extensions;using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

internal class CleanTestClassWrapper : LongLivedMarshalByRefObject, ITestClass
{
    private ITestCollection? _testCollection;
    private ITypeInfo? _class;
    private string? _fullyQualifiedTypeName;
    
    public CleanTestClassWrapper()
    {
    }
    
    public CleanTestClassWrapper(ITestCollection testCollection, ITypeInfo type, string fullyQualifiedTypeName)
    {
        this.TestCollection = testCollection ?? throw new ArgumentNullException(nameof(testCollection));
        this.Class = type ?? throw new ArgumentNullException(nameof(type));

        if (string.IsNullOrWhiteSpace(fullyQualifiedTypeName)) throw new ArgumentNullException(nameof(fullyQualifiedTypeName));
        this.FullyQualifiedTypeName = fullyQualifiedTypeName;
    }

    public ITestCollection TestCollection
    {
        get
        {
            this._testCollection.ValidateInstantiated("test collection");
            return this._testCollection;
        }
        private set => this._testCollection = value;
    }

    public ITypeInfo Class
    {
        get
        {
            this._class.ValidateInstantiated("class");
            return this._class;
        }
        private set => this._class = value;
    }

    private string FullyQualifiedTypeName
    {
        get
        {
            this._fullyQualifiedTypeName.ValidateInstantiated("fully qualified type name");
            return this._fullyQualifiedTypeName;
        }
        set => this._fullyQualifiedTypeName = value;
    }

    public void Serialize(IXunitSerializationInfo info)
    {
        info.AddValue("an", this.Class.Assembly.Name);
        info.AddValue("cn", this.FullyQualifiedTypeName);
        info.AddValue("c", this.TestCollection);
    }
    
    public void Deserialize(IXunitSerializationInfo info)
    {
        this.TestCollection = info.GetValue<ITestCollection>("c");

        this.FullyQualifiedTypeName = info.GetValue<string>("cn");
        var assemblyName = info.GetValue<string>("an");
        
        var assembly = CleanTestsFrameworkExtensions.LoadAssemblySafely(assemblyName);
        if (assembly is null) throw new InvalidOperationException($"Assembly '{assembly}' was not loaded successfully");

        var type = assembly.GetType(this.FullyQualifiedTypeName);
        if (type is null) throw new InvalidOperationException($"Type '{this.FullyQualifiedTypeName}' was not found in assembly '{assemblyName}'.");

        var xUnitTypeInfo = Reflector.Wrap(type);
        this.Class = new CleanTestReflectionTypeInfoWrapper(xUnitTypeInfo);
    }
}