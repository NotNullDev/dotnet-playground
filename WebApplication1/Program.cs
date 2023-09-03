using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

const string loggedInGuard = "must-be-authenticated";
const string adminRoleGuard = "role-admin";
const string myCorsPolicy = "my-policy";

var builder = WebApplication.CreateBuilder();

InitServices(builder);

var app = builder.Build();

InitMiddleware(app);

app.Run();

void InitServices(WebApplicationBuilder webApplicationBuilder)
{
    webApplicationBuilder.Services.AddDbContext<AppDb>(opt => { opt.UseSqlite("Data Source=db.sqlite"); });

    webApplicationBuilder.Services.AddCors(options =>
    {
        options.AddPolicy(myCorsPolicy,
            policy =>
            {
                policy.WithOrigins("http://localhost:5173", "http://localhost").AllowAnyHeader().AllowAnyMethod()
                    .AllowCredentials().Build();
            }
        );
    });

    webApplicationBuilder.Services.AddAuthentication();
    webApplicationBuilder.Services.AddAuthorization((options) =>
    {
        options.AddPolicy(loggedInGuard, b => { b.RequireAuthenticatedUser(); });
        options.AddPolicy(adminRoleGuard, b => b.RequireClaim("role", "admin"));
    });

    webApplicationBuilder.Services.AddIdentity<AppUser, IdentityRole>((a) => { a.Stores.ProtectPersonalData = false; })
        .AddEntityFrameworkStores<AppDb>()
        .AddDefaultTokenProviders();

    webApplicationBuilder.Services.Configure<IdentityOptions>(opt =>
    {
        opt.Password.RequireNonAlphanumeric = false;
        opt.Password.RequireDigit = false;
        opt.Password.RequireLowercase = false;
        opt.Password.RequireUppercase = false;
        opt.Password.RequiredUniqueChars = 0;
        opt.Password.RequiredLength = 4;

        opt.SignIn.RequireConfirmedAccount = false;
        opt.SignIn.RequireConfirmedPhoneNumber = false;

        opt.User.RequireUniqueEmail = true;
    });

    webApplicationBuilder.Services.ConfigureApplicationCookie((opt) =>
    {
        opt.LoginPath = "/login";
        opt.Events.OnRedirectToLogin = (context) =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
    });

    webApplicationBuilder.Services.AddSignalR();

    webApplicationBuilder.Services.AddSwaggerGen();
    webApplicationBuilder.Services.AddEndpointsApiExplorer();

    webApplicationBuilder.Services.AddRazorPages();
}

void InitMiddleware(WebApplication webApplication)
{
    webApplication.UseCors(myCorsPolicy);
    webApplication.UseAuthentication();
    webApplication.UseAuthorization();

    using (var scope = webApplication.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDb>();
        if (dbContext.Database.EnsureCreated())
        {
            dbContext.SaveChanges();
        }
    }

    webApplication.UseSwagger();
    webApplication.UseSwaggerUI();

    webApplication.UseStaticFiles();

    InitEndpoints(webApplication);
    InitHubs(webApplication, loggedInGuard);

    webApplication.MapRazorPages();
}

void InitEndpoints(WebApplication webApplication1)
{
    webApplication1.MapGet("/me", async (ClaimsPrincipal user, UserManager<AppUser> userManager) =>
        {
            var authUser = await userManager.GetUserAsync(user);

            return AppUserDto.From(authUser);
        })
        .RequireAuthorization(loggedInGuard)
        .Produces(200, typeof(AppUserDto))
        .Produces(403, typeof(string));

    webApplication1.MapPost("/login",
        async (SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, [FromBody] LoginRequest req) =>
        {
            var error = new LoginError();
            var user = new AppUser()
            {
                Email = req.Email,
            };

            var foundUser = await signInManager.UserManager.FindByEmailAsync(user.Email);
            if (foundUser != null)
            {
                await signInManager.SignInAsync(foundUser, true);
                return Results.Ok(AppUserDto.From(foundUser));
            }

            error.Error = "Invalid username or password.";
            return Results.BadRequest(error);
        }).Produces(200, typeof(AppUserDto)).Produces(400, typeof(LoginError));


    webApplication1.MapPost("/register",
            async (UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, AppDb db,
                RegisterRequest req) =>
            {
                var error = new RegisterError();
                var validationResults = new List<ValidationResult>();
                var valid = Validator.TryValidateObject(req, new ValidationContext(req), validationResults, true);
                if (!valid)
                {
                    error.ValidationErrors = validationResults.Select(e => e.ErrorMessage).Where(e => e != null).ToList();
                    return Results.BadRequest(error);
                }

                var appuser = new AppUser()
                {
                    UserName = req.Email.Split("@")[0].Replace(".", ""),
                    Email = req.Email
                };

                var result = await userManager.CreateAsync(appuser, req.Password);
                if (!result.Succeeded)
                {
                    error.CreationErrors = result.Errors.Select(e => e.Description).ToList();
                    return Results.BadRequest(error);
                }

                await signInManager.SignInAsync(appuser, true);

                return Results.Ok(AppUserDto.From(appuser));
            })
        .Produces(400, typeof(RegisterError))
        .Produces(200, typeof(AppUserDto))
        .Accepts(typeof(RegisterRequest), "application/json");

    webApplication1.MapGet("/logout", async (SignInManager<AppUser> singInManager) =>
    {
        await singInManager.SignOutAsync();
        return Results.Ok();
    });

    var notesGroup = webApplication1.MapGroup("/notes").RequireAuthorization(loggedInGuard);
    notesGroup.MapGet("/", async (AppDb db, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager) =>
    {
        var user = await userManager.GetUserAsync(claimsPrincipal);
        return await db.Notes.Where(n => user != null && n.Owner.Id == user.Id).ToListAsync();
    });

    notesGroup.MapPost("/",
            async ([FromBody] CreateNoteRequest req, AppDb db, ClaimsPrincipal claimsPrincipal,
                UserManager<AppUser> userManager) =>
            {
                var user = await userManager.GetUserAsync(claimsPrincipal);
                var validationResults = new List<ValidationResult>();

                var valid = Validator.TryValidateObject(req, new ValidationContext(req), validationResults, true);
                if (!valid)
                {
                    return Results.BadRequest(validationResults);
                }

                var note = new Note()
                {
                    Content = req.Content,
                    Title = req.Title,
                    Done = false,
                    Owner = user,
                };

                var result = await db.AddAsync(note);
                await db.SaveChangesAsync();

                return Results.Ok(result.Entity);
            })
        .Produces(200, typeof(Note))
        .Produces(400, typeof(List<ValidationResult>))
        .WithName("create note");

    notesGroup.MapDelete("/{id}", async (int id, AppDb db) =>
    {
        var entity = new Note() { Id = id };
        db.Notes.Attach(entity);
        db.Notes.Remove(entity);
        db.SaveChanges();

        return Results.NoContent();
    });
}

void InitHubs(WebApplication webApplication1, string s)
{
    webApplication1.MapHub<ChatHub>("/chat").RequireAuthorization(s);
}

public class RegisterRequest
{
    [Required] [EmailAddress] public String Email { get; set; }

    [Required] [MinLength(6)] public String Password { get; set; }
}

public class LoginRequest
{
    [Required] [EmailAddress] public String Email { get; set; }

    [Required] [MinLength(6)] public String Password { get; set; }
}

public class AppUser : IdentityUser
{
    [PersonalData] public DateOnly DateOfBirth { get; set; }
}

public class RegisterError
{
    public List<string> ValidationErrors { get; set; }
    public List<string> CreationErrors { get; set; }
}

public class LoginError
{
    public List<string> ValidationError { get; set; }
    public string Error { get; set; }
}

public class AppUserDto
{
    public String Id { get; set; }
    public String? Email { get; set; }
    public String? UserName { get; set; }

    public static AppUserDto From(AppUser? from)
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

public class Note
{
    public int Id { get; set; }
    [Required] [MinLength(1)] public String Content { get; set; }
    [Required] [MinLength(3)] public String Title { get; set; }
    public bool Done { get; set; }
    public AppUser Owner { get; set; }
}


public class CreateNoteRequest
{
    [Required] [MinLength(3)] public string Title { get; set; }
    [Required] [MinLength(1)] public string Content { get; set; }
}

public class ChatHub : Hub
{
    private AppDb _appDb;

    public ChatHub(AppDb appDb)
    {
        _appDb = appDb;
    }

    public async Task NewMessage(string userId, string content)
    {
        var foundUser = await _appDb.Users.Where(u => u.Id == userId).FirstAsync();
        await Clients.All.SendAsync("chatMessageReceived", Guid.NewGuid().ToString(), foundUser.Email, content);
    }
}

public class AppDb : IdentityDbContext<AppUser>
{
    public AppDb(DbContextOptions<AppDb> options) : base(options)
    {
    }

    public DbSet<Note> Notes => Set<Note>();
}