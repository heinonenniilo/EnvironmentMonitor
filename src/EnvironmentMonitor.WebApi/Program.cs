using AspNet.Security.OAuth.GitHub; // GitHub OAuth
using EnvironmentMonitor.Application.Extensions;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Infrastructure.Data;
using EnvironmentMonitor.Infrastructure.Extensions;
using EnvironmentMonitor.Infrastructure.Identity;
using EnvironmentMonitor.WebApi.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
var isDevelopment = builder.Environment.IsDevelopment();

var googleClientId = builder.Configuration["Google:ClientId"];
var googleClientSecret = builder.Configuration["Google:ClientSecret"];

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;
});

if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret)) {
    builder.Services.AddAuthentication(x =>
    {
    }).AddGoogle(googleOptions =>
    {
        googleOptions.SaveTokens = true;
        googleOptions.ClientId = googleClientId;
        googleOptions.ClientSecret = googleClientSecret;
        googleOptions.Events.OnRedirectToAuthorizationEndpoint = context =>
        {
            var redirect = QueryHelpers.AddQueryString(context.RedirectUri, "prompt", "select_account");
            context.Response.Redirect(redirect);
            return Task.CompletedTask;
        };
    });
}

var microsoftClientId = builder.Configuration["Microsoft:ClientId"];
var microsoftClientSecret = builder.Configuration["Microsoft:ClientSecret"];
var microsoftTenantId = builder.Configuration["Microsoft:TenantId"]; // Add optional tenant ID
if (!string.IsNullOrEmpty(microsoftClientId) && !string.IsNullOrEmpty(microsoftClientSecret)) {
    builder.Services.AddAuthentication().AddMicrosoftAccount(options =>
    {
        options.ClientId = microsoftClientId;
        options.ClientSecret = microsoftClientSecret;
        options.SaveTokens = false;        
        // Use tenant-specific endpoint if TenantId is provided, otherwise use common (multi-tenant)
        if (!string.IsNullOrEmpty(microsoftTenantId))
        {
            options.AuthorizationEndpoint = $"https://login.microsoftonline.com/{microsoftTenantId}/oauth2/v2.0/authorize";
            options.TokenEndpoint = $"https://login.microsoftonline.com/{microsoftTenantId}/oauth2/v2.0/token";
        }
        
        options.Events.OnRedirectToAuthorizationEndpoint = context =>
        {
            var redirect = QueryHelpers.AddQueryString(context.RedirectUri, "prompt", "select_account");
            context.Response.Redirect(redirect);
            return Task.CompletedTask;
        };

        // Fetch UPN from AccesssToken and set it as claim.
        options.Events.OnCreatingTicket = context =>
        {
            if (!string.IsNullOrWhiteSpace(context.AccessToken))
            {
                try
                {
                    var accessToken = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
                        .ReadJwtToken(context.AccessToken);

                    var upn = accessToken.Claims.FirstOrDefault(c => c.Type == "upn")?.Value
                           ?? accessToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

                    if (!string.IsNullOrWhiteSpace(upn))
                    {
                        var id = (ClaimsIdentity)context.Principal!.Identity!;
                        id.AddClaim(new Claim(ClaimTypes.Upn, upn));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing access token: {ex.Message}");
                }
            }
            return Task.CompletedTask;
        };
    });
}

// GitHub OAuth configuration
var githubClientId = builder.Configuration["GitHub:ClientId"];
var githubClientSecret = builder.Configuration["GitHub:ClientSecret"];
if (!string.IsNullOrEmpty(githubClientId) && !string.IsNullOrEmpty(githubClientSecret))
{
    builder.Services
        .AddAuthentication()
        .AddGitHub(options =>
        {
            options.ClientId = githubClientId;
            options.ClientSecret = githubClientSecret;
            options.SaveTokens = true;
            // Request email scope to get primary email
            options.Scope.Add("user:email");
            options.Events.OnCreatingTicket = context =>
            {
                // Try to add email claim if available
                var email = context.Identity?.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email) && context.User.TryGetProperty("email", out var emailProp))
                {
                    var emailValue = emailProp.GetString();
                    if (!string.IsNullOrEmpty(emailValue))
                    {
                        context.Identity!.AddClaim(new Claim(ClaimTypes.Email, emailValue));
                    }
                }
                return Task.CompletedTask;
            };
        });
}

builder.Services.AddAuthorization();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure ApplicationSettings with IsProduction flag
var applicationSettings = new ApplicationSettings();
builder.Configuration.GetSection("ApplicationSettings").Bind(applicationSettings);
applicationSettings.IsProduction = builder.Environment.IsProduction();

builder.Services.AddInfrastructureServices(builder.Configuration, applicationSettings: applicationSettings);
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

    // TODO TO BE REIMPLEMENTED
    conf.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };

    conf.Events.OnRedirectToLogin = context =>
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
app.UseForwardedHeaders();

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