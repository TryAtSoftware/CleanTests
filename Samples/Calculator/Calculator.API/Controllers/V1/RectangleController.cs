namespace Calculator.API.Controllers.V1;

using Calculator.API.InputModels.V1;
using Calculator.API.OutputModels.V1;
using Microsoft.AspNetCore.Mvc;

[ApiController, ApiVersion("1.0"), Route("api/v{version:apiVersion}/rectangle")]
public class RectangleController : ControllerBase
{
    [HttpPost("perimeter")]
    public IActionResult CalculatePerimeter(RectanglePerimeterInputModel inputModel)
    {
        var result = new ScalarOutputModel { Result = 2 * (inputModel.A + inputModel.B) };
        return this.Ok(result);
    }

    [HttpPost("area")]
    public IActionResult CalculateArea(RectangleAreaInputModel inputModel)
    {
        var result = new ScalarOutputModel { Result = inputModel.A * inputModel.B };
        return this.Ok(result);
    }
}