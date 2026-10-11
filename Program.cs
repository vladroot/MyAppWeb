using MyAppWeb.Configuration;
using MyAppWeb.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);

var app = builder.Build();

await app.InitializeDatabaseAsync();

app.UseApplicationPipeline();
app.MapApplicationEndpoints();

await app.RunAsync();
