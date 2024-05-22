
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using TokenProvider.Infrastructure.Models;

namespace TokenProvider.Infrastructure.Services;

public class TokenGenerator(IRefreshTokenService refreshTokenService)
{
    private readonly IRefreshTokenService _refreshTokenService = refreshTokenService;


    public async Task<RefreshTokenResult> GenerateRefreshTokenAsync( string userId, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return new RefreshTokenResult { StatusCode = (int)HttpStatusCode.BadRequest, Error = "Invalid body request. No userid was found" };

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var token = GenerateJwtToken(new ClaimsIdentity(claims), DateTime.Now.AddMinutes(5));
            if (token == null)
                return new RefreshTokenResult { StatusCode = (int)HttpStatusCode.InternalServerError, Error = "An error lol" };


        }catch (Exception ex) 
        {
            return new RefreshTokenResult { StatusCode = (int)HttpStatusCode.InternalServerError, Error = ex.Message };
        }
    }


    public static string GenerateJwtToken (ClaimsIdentity claims, DateTime expires)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claims,
            Expires = expires,
            Issuer = Environment.GetEnvironmentVariable("TOKEN_ISSUER"),
            Audience = Environment.GetEnvironmentVariable("TOKEN_AUDIENCE"),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("TOKEN_SECRETKEY")!)), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
