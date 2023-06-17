namespace Calculator.CleanTests.Utilities.Interfaces;

public interface IApiOperationDescriptor
{
    Type ResponseType { get; }
    string ConstructUrl(IDictionary<string, object> arguments);
    object ConstructInputModel(IDictionary<string, object> arguments);
}