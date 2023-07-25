using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using restaurantReservation.Models;

namespace restaurantReservation.Factory
{
    public class AppUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User, IdentityRole>
    {
        public AppUserClaimsPrincipalFactory(UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
        {
        }
    }
}
