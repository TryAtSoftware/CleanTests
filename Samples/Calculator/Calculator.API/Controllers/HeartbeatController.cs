namespace Calculator.API.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController, Route("api/heartbeat")]
public class HeartbeatController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => this.Ok();
}