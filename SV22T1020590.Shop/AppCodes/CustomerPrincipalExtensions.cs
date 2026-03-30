using System.Security.Claims;

namespace SV22T1020590.Shop
{
    public static class CustomerPrincipalExtensions
    {
        public static int? GetCustomerId(this ClaimsPrincipal user)
        {
            var v = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(v, out var id) ? id : null;
        }
    }
}
