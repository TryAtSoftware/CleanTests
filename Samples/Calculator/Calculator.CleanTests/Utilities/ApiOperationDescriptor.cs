namespace Calculator.CleanTests.Utilities;

using Calculator.CleanTests.Utilities.Interfaces;

public class ApiOperationDescriptor : IApiOperationDescriptor
{
    private readonly Func<IDictionary<string, object>, string> _urlConstructionFunc;
    private readonly Func<IDictionary<string, object>, object> _inputModelConstructionFunc;

    public ApiOperationDescriptor(Func<IDictionary<string, object>, string> urlConstructionFunc, Func<IDictionary<string, object>, object> inputModelConstructionFunc, Type responseType)
    {
        this._urlConstructionFunc = urlConstructionFunc ?? throw new ArgumentNullException(nameof(urlConstructionFunc));
        this._inputModelConstructionFunc = inputModelConstructionFunc ?? throw new ArgumentNullException(nameof(inputModelConstructionFunc));
        this.ResponseType = responseType;
    }

    public Type ResponseType { get; }
    public string ConstructUrl(IDictionary<string, object> arguments) => this._urlConstructionFunc(arguments);
    public object ConstructInputModel(IDictionary<string, object> arguments) => this._inputModelConstructionFunc(arguments);
}