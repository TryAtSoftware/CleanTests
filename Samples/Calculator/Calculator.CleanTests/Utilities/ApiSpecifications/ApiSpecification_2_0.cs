namespace Calculator.CleanTests.Utilities.ApiSpecifications;

using Calculator.API.InputModels.V1;
using Calculator.API.InputModels.V2;
using Calculator.API.OutputModels.V1;
using Calculator.CleanTests.Constants;
using Calculator.CleanTests.Utilities.Interfaces;
using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(InitializationCategories.ApiSpecification, "v2.0", Characteristics.SupportsRectangleOperations, IsGlobal = true)]
public class ApiSpecification_2_0 : IApiSpecification
{
    public IApiOperationDescriptor TrianglePerimeter { get; } = new ApiOperationDescriptor(_ => "api/v2.0/triangle/perimeter", args => new TrianglePerimeterInputModel { Side1 = (int)args["side1"], Side2 = (int)args["side2"], Side3 = (int)args["side3"] }, typeof(ScalarOutputModel));
    public IApiOperationDescriptor RectanglePerimeter { get; }  = new ApiOperationDescriptor(_ => "api/v2.0/rectangle/perimeter", args => new RectanglePerimeterInputModel { Side1 = (int)args["side1"], Side2 = (int)args["side2"] }, typeof(ScalarOutputModel));
}