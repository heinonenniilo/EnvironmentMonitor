using EnvironmentMonitor.Application.Extensions;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Infrastructure.Data;
using EnvironmentMonitor.Infrastructure.Extensions;
using EnvironmentMonitor.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

var isDevelopment = builder.Environment.IsDevelopment();

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
});
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentCorsPolicy");
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleDefiner = services.GetRequiredService<IRoleManager>();
    await roleDefiner.SetRoles(); 
}

app.Run();
