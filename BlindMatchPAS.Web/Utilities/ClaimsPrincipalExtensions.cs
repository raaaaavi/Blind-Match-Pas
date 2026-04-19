using System.Security.Claims;

namespace BlindMatchPAS.Web.Utilities;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    public static string GetDisplayName(this ClaimsPrincipal principal) =>
        principal.FindFirstValue("DisplayName") ?? principal.Identity?.Name ?? "User";
}
