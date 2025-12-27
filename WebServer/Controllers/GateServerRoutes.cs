using EggLink.DanhengServer.WebServer.Handler;
using Microsoft.AspNetCore.Mvc;

namespace EggLink.DanhengServer.WebServer.Controllers;

[ApiController]
[Route("/")]
public class GateServerRoutes
{
    [HttpGet("/query_gateway")]
    public async ValueTask<string> QueryGateway([FromQuery] string version)
    {
        await ValueTask.CompletedTask;
        return new QueryGatewayHandler(version).Data;
    }
}