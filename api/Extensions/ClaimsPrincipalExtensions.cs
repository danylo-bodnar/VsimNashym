using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace api.Extensions
{
    static public class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException("UserId claim is missing.");

            return Guid.Parse(value);
        }

        public static long GetTelegramId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue("telegram_id");
            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException("TelegramId claim is missing.");

            return long.Parse(value);
        }
    }
}