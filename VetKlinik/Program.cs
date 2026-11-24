using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using ModelContextProtocol.AspNetCore;
using VetKlinik.Auth;
using VetKlinik.Data;
using VetKlinik.Mcp;
using VetKlinik.Models;
using VetKlinik.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<UygulamaDbContext>(secenek =>
    secenek.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<UygulamaKullanicisi, IdentityRole>(secenek =>
    {
        // Ödev admin şifresi "sau" olduğu için parola kuralları gevşetildi.
        secenek.Password.RequireDigit = false;
        secenek.Password.RequireLowercase = false;
        secenek.Password.RequireUppercase = false;
        secenek.Password.RequireNonAlphanumeric = false;
        secenek.Password.RequiredLength = 3;
        secenek.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<UygulamaDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(secenek =>
{
    secenek.LoginPath = "/Hesap/Giris";
    secenek.AccessDeniedPath = "/Hesap/ErisimYok";
});

builder.Services.AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationHandler.SemaAdi, _ => { })
    .AddPolicyScheme("Akilli", "Cookie veya API anahtarı", o =>
    {
        o.ForwardDefaultSelector = ctx =>
        {
            var header = ctx.Request.Headers.Authorization.ToString();
            if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return ApiKeyAuthenticationHandler.SemaAdi;
            return IdentityConstants.ApplicationScheme;
        };
    });

builder.Services.Configure<AuthenticationOptions>(o =>
{
    o.DefaultAuthenticateScheme = "Akilli";
    o.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<RandevuServisi>();
builder.Services.AddScoped<HayvanServisi>();
builder.Services.AddScoped<AsiServisi>();
builder.Services.AddScoped<KlinikServisi>();
builder.Services.AddScoped<HizmetServisi>();
builder.Services.AddScoped<VeterinerServisi>();
builder.Services.AddScoped<ApiAnahtarServisi>();
builder.Services.AddScoped<RaporServisi>();
builder.Services.AddScoped<FotoServisi>();

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(secenek =>
{
    secenek.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Pati Ev Veteriner API",
        Version = "v1"
    });
    secenek.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Profilim sayfasında üretilen API anahtarı. Örnek: Bearer abc123",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    secenek.AddSecurityRequirement(dokuman => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", dokuman)] = []
    });
});

builder.Services.AddMcpServer(o =>
    {
        o.ServerInfo = new() { Name = "pati-ev-klinik", Version = "1.0.0" };
    })
    .WithHttpTransport(o => o.SessionMode = HttpServerSessionMode.Stateless)
    .WithTools<KlinikMcpAraclari>();

var app = builder.Build();

using (var kapsam = app.Services.CreateScope())
{
    var db = kapsam.ServiceProvider.GetRequiredService<UygulamaDbContext>();
    db.Database.Migrate();
    await VeriTohumu.CalistirAsync(kapsam.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Hata/Sunucu");
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStatusCodePagesWithReExecute("/Hata/Kod", "?kod={0}");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Klinikler}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapMcp("/mcp").RequireAuthorization();

app.Run();
