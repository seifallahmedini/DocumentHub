using DocumentHub.Core;
using DocumentHub.Infrastructure;
using DocumentHub.Web.Endpoints.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationHandlers();

var app = builder.Build();

app.InitializeDatabase();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseAuthentication();
app.UseAuthorization();

RegisterEndpoint.Map(app);

app.Run();

public partial class Program { }
