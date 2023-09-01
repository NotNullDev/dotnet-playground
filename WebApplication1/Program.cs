using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder();

builder.Services.AddDbContext<AppDb>(opt => opt.UseSqlite("Data Source=db.sqlite"));

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

app.MapGet("/", () =>
{
    var sampleResponse = new SampleResponse
    {
        Name = "Jacek",
        Age = 24
    };

    return sampleResponse;
}).WithName("Get haha");

var products = app.MapGroup("/notes");
products.MapGet("/", async (AppDb db) => await db.Notes.ToListAsync());

products.MapGet("/create", async (AppDb db) =>
{
    var newNote = new Note();
    newNote.Content = "hehhe";
    newNote.Title = "Note af dsaf";
    newNote.Done = false;

    var entityEntry = db.Notes.Add(newNote);
    await db.SaveChangesAsync();

    return entityEntry.Entity;
});

products.MapPost("/",  async (CreateNoteRequest req, AppDb db) =>
{
    var note = new Note()
    {
        Content = req.Content,
        Title = req.Title,
        Done = false
    };

    var result = await db.AddAsync(note);
    await db.SaveChangesAsync();

    return result.Entity;
}).WithName("create note");

products.MapDelete("/{id}", async (int id, AppDb db) =>
{
    var entity = new Note(){Id = id};
    db.Notes.Attach(entity);
    db.Notes.Remove(entity);
    db.SaveChanges();
    
    
    return Results.NoContent();
});

app.MapRazorPages();

app.Run();

public class Note
{
    public int Id { get; set; }
    public String Content { get; set; }
    public String Title { get; set; }
    public bool Done { get; set; }
}


public class CreateNoteRequest
{
    public string Title { get; set; }
    public string Content { get; set; }
}

public class AppDb : DbContext
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