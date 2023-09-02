using System.Runtime.InteropServices.JavaScript;

namespace WebApplication1;

public class AppUserDto
{
    public String Id { get; set; }
    public String? Email { get; set; }
    public String? UserName { get; set; }
    
    public static AppUserDto from(AppUser? from)
    {
        if (from == null)  
        {
            return new AppUserDto();
        }
        
        var appUserDto = new AppUserDto()
        {
            Id = from.Id,
            Email = from.Email,
            UserName = from.UserName
        };

        return appUserDto;
    }
}