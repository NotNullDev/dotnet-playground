using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1;

var builder = WebApplication.CreateBuilder();

builder.Services.AddDbContext<AppDb>(opt => { opt.UseSqlite("Data Source=db.sqlite"); });

builder.Services.AddIdentity<AppUser, IdentityRole>((a) => { a.Stores.ProtectPersonalData = false; })
    .AddEntityFrameworkStores<AppDb>()
    .AddDefaultTokenProviders();

const string authScheme = "cookie";
const string loggedIn = "must-be-authenticated";
const string adminRole = "role-admin";

builder.Services.AddAuthentication(authScheme).AddCookie(authScheme, (options) =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";

    options.ReturnUrlParameter = "returnUrl";
});
builder.Services.AddAuthorization((options) =>
{
    options.AddPolicy(loggedIn, b => b.RequireAuthenticatedUser());
    options.AddPolicy(adminRole, b => b.RequireClaim("role", "admin"));
});

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddRazorPages();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().Build()
    );
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDb>();
    if (dbContext.Database.EnsureCreated())
    {
        dbContext.SaveChanges();
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

var adminGroup = app.MapGroup("/admin");
adminGroup.RequireAuthorization(policyNames: new[] { loggedIn, adminRole });

adminGroup.MapGet("/", () => { return "you must be an admin!"; });

app.MapGet("/secured", () => { return Results.Ok("You are logged in!"); }).RequireAuthorization(loggedIn);

app.MapGet("/me", async (ClaimsPrincipal user, UserManager<AppUser> userManager) =>
    {
        var authUser = await userManager.GetUserAsync(user);

        return AppUserDto.from(authUser);
    })
    .RequireAuthorization(loggedIn)
    .Produces(200, typeof(AppUserDto))
    .Produces(403, typeof(string));

app.MapGet("/login", async (SignInManager<AppUser> signInManager, UserManager<AppUser> userManager) =>
{
    var user = new AppUser()
    {
        Email = "aa@gmail.com",
        UserName = "JohnDoeHaha"
    };

    var foundUser = await signInManager.UserManager.FindByEmailAsync(user.Email);
    if (foundUser != null)
    {
        await signInManager.SignInAsync(foundUser, true);
        return Results.Redirect("/");
    }

    var createdUsr = await signInManager.UserManager.CreateAsync(user, "Adsa31232W#@!!@#sad21#!@");
    if (createdUsr.Succeeded)
    {
        await signInManager.SignInAsync(user, true);
    }
    else
    {
        Console.WriteLine(createdUsr.Errors);
    }

    return Results.Redirect("/");
}).Produces(303, typeof(string)).Produces(400, typeof(List<IdentityError>));

app.MapPost("/register",
    async (UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, AppDb db, RegisterRequest req) =>
    {
        var validationResults = new List<ValidationResult>();
        var valid = Validator.TryValidateObject(req, new ValidationContext(req), validationResults, true);
        if (!valid)
        {
            return Results.BadRequest(validationResults);
        }

        var appuser = new AppUser();

        var result = await userManager.CreateAsync(appuser);
        if (!result.Succeeded)
        {
            return Results.BadRequest(result.Errors);
        }

        await signInManager.SignInAsync(appuser, true);

        return Results.Redirect("/");
    })
    .Produces(400, typeof(List<ValidationResult>))
    .Produces(400, typeof(List<IdentityError>))
    .Produces(303, typeof(string));

app.MapGet("/logout", async (SignInManager<AppUser> singInManager) =>
{
    await singInManager.SignOutAsync();
    return Results.Redirect("/");
});

app.MapGet("/", async (HttpContext ctx, UserManager<AppUser> userManager, ClaimsPrincipal user) =>
{
    if (ctx.User.Identity?.IsAuthenticated == false)
    {
        return Results.Unauthorized();
    }

    var foundUser = await userManager.GetUserAsync(user);

    return Results.Ok(AppUserDto.from(foundUser));
}).WithName("Get haha");

var notesGroup = app.MapGroup("/notes").RequireAuthorization(loggedIn);
notesGroup.MapGet("/", async (AppDb db, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager) =>
{
    var user = await userManager.GetUserAsync(claimsPrincipal);
    return await db.Notes.Where(n => n.Owner.Id == user.Id).ToListAsync();
});

notesGroup.MapGet("/create", async (AppDb db) =>
{
    var newNote = new Note();
    newNote.Content = "hehhe";
    newNote.Title = "Note af dsaf";
    newNote.Done = false;

    var entityEntry = db.Notes.Add(newNote);
    await db.SaveChangesAsync();

    return entityEntry.Entity;
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

app.MapRazorPages();

app.Run();

public class RegisterRequest
{
    [Required] [EmailAddress] public String Email { get; set; }

    [Required] [MinLength(6)] public String Password { get; set; }
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

public class AppDb : IdentityDbContext<AppUser>
{
    public AppDb(DbContextOptions<AppDb> options) : base(options)
    {
    }

    public DbSet<Note> Notes => Set<Note>();
}

class SampleResponse
{
    public String? Name { get; set; }
    public int Age { get; set; }
}