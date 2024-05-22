﻿
using Microsoft.AspNetCore.Http;

namespace TokenProvider.Infrastructure.Services;

public class CookieGenerator
{
    public static  CookieOptions GenerateCookie(DateTimeOffset expiryDate)
    {
        var cookieOption = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Expires = expiryDate,
        };

        return cookieOption;
    }
}
