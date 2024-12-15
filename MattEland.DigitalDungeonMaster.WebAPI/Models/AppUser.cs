using System.Security.Claims;

namespace MattEland.DigitalDungeonMaster.WebAPI.Models;

public class AppUser : ClaimsPrincipal
{
    public AppUser(IHttpContextAccessor context) : base(context.HttpContext!.User) { }

    public string? Id => FindFirst(ClaimTypes.NameIdentifier)?.Value;
    public string? Email => FindFirst(ClaimTypes.Email)?.Value;
}