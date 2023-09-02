using Microsoft.AspNetCore.Identity;

namespace WebApplication1;

public class AppUser: IdentityUser
{
    [PersonalData] 
    public DateOnly DateOfBirth { get; set; }
}