using System.Security.Claims;

namespace MattEland.DigitalDungeonMaster.WebAPI.Models;

/// <summary>
/// An injectable user within the application context.
/// Contains their authenticated identity information taken from the JWT.
/// </summary>
public class AppUser : ClaimsPrincipal
{
    /// <summary>
    /// Creates a new instance of the <see cref="AppUser"/> class.
    /// </summary>
    /// <remarks>
    /// This is typically invoked by the dependency injection container.
    /// </remarks>
    /// <param name="context">
    /// The HTTP context to extract the user from.
    /// </param>
    public AppUser(IHttpContextAccessor context) : base(context.HttpContext!.User)
    {
        
    }

    /// <summary>
    /// Gets the name of the user.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the user could not be loaded.</exception>
    public string Name
    {
        get
        {
            Claim? nameClaim = FindFirst(ClaimTypes.NameIdentifier);
            if (nameClaim is null)
            {
                throw new InvalidOperationException("User does not have a name claim");
            }
            return nameClaim.Value;
        }
    }
}