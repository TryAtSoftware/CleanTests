namespace Calculator.CleanTests.Utilities.ApiSpecifications;

using Calculator.API.InputModels.V1;
using Calculator.API.OutputModels.V1;
using Calculator.CleanTests.Constants;
using Calculator.CleanTests.Utilities.Interfaces;
using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(InitializationCategories.ApiSpecification, "v1.0", IsGlobal = true)]
public class ApiSpecification_1_0 : IApiSpecification
{
    public IApiOperationDescriptor TrianglePerimeter { get; } = new ApiOperationDescriptor(_ => "api/v1.0/triangle/perimeter", args => new TrianglePerimeterInputModel { Side1 = (int)args["side1"], Side2 = (int)args["side2"], Side3 = (int)args["side3"] }, typeof(ScalarOutputModel));
    public IApiOperationDescriptor RectanglePerimeter => throw new InvalidOperationException("This API endpoint is not supported for the current version.");
}