namespace Calculator.API.Controllers.V2;

using Calculator.API.InputModels.V2;
using Calculator.API.OutputModels.V2;
using Microsoft.AspNetCore.Mvc;

[ApiController, ApiVersion("2.0"), Route("api/v{version:apiVersion}/rectangle")]
public class RectangleController : ControllerBase
{
    [HttpPost("perimeter")]
    public IActionResult CalculatePerimeter(RectanglePerimeterInputModel inputModel)
    {
        var result = new ScalarOutputModel { Result = 2 * (inputModel.Side1 + inputModel.Side2) };
        return this.Ok(result);
    }

    [HttpPost("area")]
    public IActionResult CalculateArea(RectangleAreaInputModel inputModel)
    {
        var result = new ScalarOutputModel { Result = inputModel.Side1 * inputModel.Side2 };
        return this.Ok(result);
    }
}