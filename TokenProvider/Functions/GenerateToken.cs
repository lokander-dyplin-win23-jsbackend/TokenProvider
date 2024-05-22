using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TokenProvider.Infrastructure.Models;
using TokenProvider.Infrastructure.Services;

namespace TokenProvider.Functions;

public class GenerateToken
{
    private readonly ILogger<GenerateToken> _logger;
    private readonly IRefreshTokenService _refreshTokenService;

    public GenerateToken(ILogger<GenerateToken> logger, IRefreshTokenService refreshTokenService)
    {
        _logger = logger;
        _refreshTokenService = refreshTokenService;
    }

    [Function("GenerateToken")]

    public async IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "token/generate")] HttpRequest req, [FromBody] TokenRequest tokenRequest)
    {

        if (tokenRequest.UserId == null || tokenRequest.Email == null)
            return new BadRequestObjectResult(new { Error = "Please provide a valid email" });


        try
        {
            RefreshTokenResult refreshTokenResult = null!;
            using var ctsTimeOut = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ctsTimeOut.Token, req.HttpContext.RequestAborted);

            req.HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken);
            if (!string.IsNullOrEmpty(refreshToken))
                refreshTokenResult = await _refreshTokenService.GetRefreshTokenAsync(refreshToken, cts.Token);

            if ( refreshTokenResult == null ||refreshTokenResult.ExpiryDate < DateTime.Now.AddDays(1))
                refreshTokenResult = await _tokenGenerator.GenerateRefreshTokenAsync(tokenRequest.UserId, cts.Token);
        }catch 
        {

        }
        

        
        return new OkObjectResult("");
    }
}
