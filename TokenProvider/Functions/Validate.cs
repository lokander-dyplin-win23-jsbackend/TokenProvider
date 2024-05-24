using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

public class Validate
{
    private readonly ILogger<Validate> _logger;

    public Validate(ILogger<Validate> logger)
    {
        _logger = logger;
    }

    [Function("Validate")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
        var response = req.CreateResponse();

        // Hämta token från Authorization header
        if (!req.Headers.TryGetValues("Authorization", out var authorizationHeaders))
        {
            response.StatusCode = System.Net.HttpStatusCode.Unauthorized;
            await response.WriteStringAsync("No Authorization header present.");
            return response;
        }

        var token = authorizationHeaders.FirstOrDefault()?.Split(" ").Last();

        if (string.IsNullOrEmpty(token))
        {
            response.StatusCode = System.Net.HttpStatusCode.Unauthorized;
            await response.WriteStringAsync("Bearer token not found.");
            return response;
        }

        // Validera token
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = Environment.GetEnvironmentVariable("TOKEN_ISSUER"),
            ValidateAudience = true,
            ValidAudience = Environment.GetEnvironmentVariable("TOKEN_AUDIENCE"),
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("TOKEN_SECRETKEY")!)),
            ValidateIssuerSigningKey = true
        };

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            response.StatusCode = System.Net.HttpStatusCode.OK;
            await response.WriteStringAsync("Token is valid.");
        }
        catch (SecurityTokenValidationException stve)
        {
            response.StatusCode = System.Net.HttpStatusCode.Unauthorized;
            await response.WriteStringAsync($"Token validation failed: {stve.Message}");
        }
        catch (Exception ex)
        {
            response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
            await response.WriteStringAsync($"An error occurred: {ex.Message}");
        }

        return response;
    }
}