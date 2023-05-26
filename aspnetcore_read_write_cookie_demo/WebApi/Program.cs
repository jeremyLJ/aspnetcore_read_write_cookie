using WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.UseMiddleware<UpsertCookieMiddleware>();

app.MapControllers();
app.MapGet("/", () => "Hello World!");

app.Run();