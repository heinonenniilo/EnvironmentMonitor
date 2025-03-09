using EnvironmentMonitor.Application.Extensions;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Infrastructure.Data;
using EnvironmentMonitor.Infrastructure.Extensions;
using EnvironmentMonitor.Infrastructure.Identity;
using EnvironmentMonitor.WebApi.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
var isDevelopment = builder.Environment.IsDevelopment();

var googleClientId = builder.Configuration["Google:ClientId"];
var googleClientSecret = builder.Configuration["Google:ClientSecret"];
if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret)) {
    builder.Services.AddAuthentication().AddGoogle(googleOptions =>
    {
        googleOptions.SaveTokens = true;
        googleOptions.ClientId = googleClientId;
        googleOptions.ClientSecret = googleClientSecret;
    });
}

builder.Services.AddAuthorization();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Specify React dev server origin
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for cookies/credentials
    });
});

builder.Services.ConfigureApplicationCookie(conf =>
{
    if (builder.Environment.IsDevelopment())
    {
        conf.Cookie.SameSite = SameSiteMode.None;
    }
    conf.Cookie.Name = "env-monitor";
    conf.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

// This fixes the issue with additional claims being lost, apparently.
// https://github.com/dotnet/aspnetcore/issues/49610
// Could look into other options?
builder.Services.Configure<SecurityStampValidatorOptions>((options) =>
{
    options.OnRefreshingPrincipal = refreshingPrincipal =>
    {
        ClaimsIdentity? newIdentity = refreshingPrincipal.NewPrincipal?.Identities.First();
        ClaimsIdentity? currentIdentity = refreshingPrincipal.CurrentPrincipal?.Identities.First();
        if (currentIdentity is not null && newIdentity is not null)
        {
            var currentClaimsNotInNewIdentity = currentIdentity.Claims.Where(c => !newIdentity.HasClaim(c.Type, c.Value));
            foreach (Claim claim in currentClaimsNotInNewIdentity)
            {
                newIdentity.AddClaim(claim);
            }
        }
        return Task.CompletedTask;
    };
});

builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
var app = builder.Build();
app.UseExceptionHandler(o => { });
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentCorsPolicy");
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<ApiKeyMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleDefiner = services.GetRequiredService<IRoleManager>();
    await roleDefiner.SetRoles(); 
}

app.UseSpa(spa =>
{
    spa.Options.SourcePath = "wwwroot";
});

app.Run();

public partial class Program { }