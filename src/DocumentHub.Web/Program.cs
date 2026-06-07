using DocumentHub.Core;
using DocumentHub.Infrastructure;
using DocumentHub.Web.Endpoints.Auth;
using DocumentHub.Web.Endpoints.Users;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCoreServices();

var app = builder.Build();

app.InitializeDatabase();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/auth")
   .MapRegisterEndpoint()
   .MapLoginEndpoint();

app.MapGroup("/users")
   .RequireAuthorization()
   .MapUsersEndpoints();

app.Run();

public partial class Program { }
