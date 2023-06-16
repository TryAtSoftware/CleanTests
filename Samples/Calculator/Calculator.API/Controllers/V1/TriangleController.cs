namespace Calculator.API.Controllers.V1;

using Calculator.API.InputModels.V1;
using Calculator.API.OutputModels;
using Calculator.API.OutputModels.V1;
using Microsoft.AspNetCore.Mvc;

[ApiController, ApiVersion("1.0"), ApiVersion("2.0"), Route("api/v{version:apiVersion}/triangle")]
public class TriangleController : ControllerBase
{
    [HttpPost("perimeter")]
    public IActionResult CalculatePerimeter(TrianglePerimeterInputModel inputModel)
    {
        var result = new ScalarOutputModel { Result = inputModel.Side1 + inputModel.Side2 + inputModel.Side3 };
        return this.Ok(result);
    }
}