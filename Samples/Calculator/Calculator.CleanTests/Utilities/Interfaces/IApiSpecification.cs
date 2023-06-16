namespace Calculator.CleanTests.Utilities.Interfaces;

public interface IApiSpecification
{
    IApiOperationDescriptor TrianglePerimeter { get; }
    IApiOperationDescriptor RectanglePerimeter { get; }
}