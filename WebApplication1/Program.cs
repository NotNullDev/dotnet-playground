var builder = WebApplication.CreateBuilder();

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () =>
{
    var sampleResponse = new SampleResponse();
    sampleResponse.Name = "Jacek";
    sampleResponse.Age = 24;
    
    return sampleResponse;
}).WithName("Get haha");

app.Run();

class SampleResponse
{
    public String Name { get; set; }
    public int Age { get; set; }
}