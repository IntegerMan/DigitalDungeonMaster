using System.Security.Claims;

namespace MattEland.DigitalDungeonMaster.WebAPI.Models;

public class AppUser : ClaimsPrincipal
{
    public AppUser(IHttpContextAccessor context) : base(context.HttpContext!.User) { }

    public string Name => FindFirst(ClaimTypes.NameIdentifier)!.Value;
}