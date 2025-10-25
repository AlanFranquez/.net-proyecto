using Espectaculos.Application.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MediatR;
using Espectaculos.Infrastructure;
using Espectaculos.Infrastructure.Persistence;
using Espectaculos.Application.Abstractions.Repositories;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();

// Si AddInfrastructure(string) espera la cadena, pásasela:
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("Default")
);

// MediatR (descubre handlers en el ensamblado que contiene IUnitOfWork)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(IUnitOfWork).Assembly);
});

// Identity
builder.Services
    .AddDefaultIdentity<IdentityUser>(o =>
    {
        o.SignIn.RequireConfirmedAccount = false;
        o.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<EspectaculosDbContext>();

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("RequireAdmin", p => p.RequireRole("Admin"));
});
/*
builder.Services.AddRazorPages(o =>
{
    o.Conventions.AuthorizeAreaFolder("Admin", "/", "RequireAdmin");
});
*/
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.Run();