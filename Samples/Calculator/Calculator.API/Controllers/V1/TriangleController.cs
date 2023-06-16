namespace Calculator.API.Controllers.V1;

using Calculator.API.InputModels.V1;
using Calculator.API.OutputModels.V1;
using Microsoft.AspNetCore.Mvc;

[ApiController, ApiVersion("1.0"), Route("api/v{version:apiVersion}/triangle")]
public class TriangleController : ControllerBase
{
    [HttpPost("perimeter")]
    public IActionResult CalculatePerimeter(TrianglePerimeterInputModel inputModel)
    {
        var result = new ScalarOutputModel { Result = inputModel.A + inputModel.B + inputModel.C };
        return this.Ok(result);
    }

    [HttpPost("area")]
    public IActionResult CalculateArea(TriangleAreaInputModel inputModel)
    {
        var result = new ScalarOutputModel { Result = inputModel.A * inputModel.H / 2 };
        return this.Ok(result);
    }
}