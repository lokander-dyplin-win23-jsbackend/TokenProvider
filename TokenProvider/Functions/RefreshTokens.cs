﻿using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TokenProvider.Infrastructure.Models;
using TokenProvider.Infrastructure.Services;

namespace TokenProvider.Functions;

public class RefreshTokens(ILogger<RefreshTokens> logger, ITokenService tokenService)
{
    private readonly ILogger<RefreshTokens> _logger = logger;
    private readonly ITokenService _tokenService = tokenService;

    [Function("RefreshTokens")]

    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "token/refresh")] HttpRequest req)
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();
        var tokenRequest = JsonConvert.DeserializeObject<TokenRequest>(body);



        if (tokenRequest == null || tokenRequest.UserId == null || tokenRequest.Email == null)
            return new BadRequestObjectResult(new { Error = "Please provide a valid email" });


        try
        {
            RefreshTokenResult refreshTokenResult = null!;
            AccessTokenResult accessTokenResult = null!;
            using var ctsTimeOut = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ctsTimeOut.Token, req.HttpContext.RequestAborted);

            req.HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken);

            if(string.IsNullOrEmpty(refreshToken))
                return new UnauthorizedObjectResult(new { Error = "Refreshtoken was not found" });


                refreshTokenResult = await _tokenService.GetRefreshTokenAsync(refreshToken, cts.Token);

                if (refreshTokenResult.ExpiryDate < DateTime.Now)
                    return new UnauthorizedObjectResult(new { Error = "Refresh has expired" });

                if ( refreshTokenResult.ExpiryDate < DateTime.Now.AddDays(1))
                    refreshTokenResult = await _tokenService.GenerateRefreshTokenAsync(tokenRequest.UserId, cts.Token);

               
                accessTokenResult = _tokenService.GenerateAccessToken(tokenRequest, refreshTokenResult.Token);

                if (refreshTokenResult.Token != null && refreshTokenResult.CookieOptions != null)
                    req.HttpContext.Response.Cookies.Append("refreshToken", refreshTokenResult.Token, refreshTokenResult.CookieOptions);


                if (accessTokenResult != null && accessTokenResult.Token != null && refreshTokenResult.Token != null)
                    return new OkObjectResult(new { AccessToken = accessTokenResult.Token, RefreshToken = refreshTokenResult.Token });

        }
        catch 
        {

        }
        return new ObjectResult(new { Error = "error generating tokens" }) { StatusCode = 500 };

    }
}
