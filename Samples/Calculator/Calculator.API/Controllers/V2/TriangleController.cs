namespace Calculator.API.Controllers.V2;

using Calculator.API.InputModels.V2;
using Calculator.API.OutputModels.V2;
using Microsoft.AspNetCore.Mvc;

[ApiController, ApiVersion("1.0"), Route("api/v{version:apiVersion}/triangle")]
public class TriangleController : ControllerBase
{
    [HttpPost("perimeter")]
    public IActionResult CalculatePerimeter(TrianglePerimeterInputModel inputModel)
    {
        var result = new ScalarOutputModel { Result = inputModel.Side1 + inputModel.Side2 + inputModel.Side3 };
        return this.Ok(result);
    }

    [HttpPost("area")]
    public IActionResult CalculateArea(TriangleAreaInputModel inputModel)
    {
        var result = new ScalarOutputModel { Result = inputModel.Side * inputModel.Height / 2 };
        return this.Ok(result);
    }
}