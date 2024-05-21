using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace TokenProvider.Functions;

public class GenerateToken
{
    private readonly ILogger<GenerateToken> _logger;

    public GenerateToken (ILogger<GenerateToken> logger)
    {
        _logger = logger;
    }

    [Function("GenerateToken")]

    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        return new OkObjectResult("");
    }
}
