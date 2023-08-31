var builder = WebApplication.CreateBuilder(args);

builder.Services.AddE();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapGet("/haha", () =>
{
    Console.WriteLine("hello world!");
    var resp = new Response
    {
        Name = "Jacek",
        Age = 24
    };

    return resp;
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/v1/swagger.json", "My hello world app");
});

app.Run();

class Response
{
    public String Name { get; set; }
    public int Age { get; set; }
    
}