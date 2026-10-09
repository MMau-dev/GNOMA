using GNOMA.Application.Services.Interfaces;
using GNOMA.Application.Services.Supabase;
using GNOMA.Infrastructure.Supabase.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services
    .AddOptions<SupabaseOptions>()
    .BindConfiguration(SupabaseOptions.SectionName)
    .Validate(
        options =>
            Uri.TryCreate(options.Url, UriKind.Absolute, out var uri) &&
            uri.Scheme == Uri.UriSchemeHttps &&
            !string.IsNullOrWhiteSpace(options.PublishableKey) &&
            !string.IsNullOrWhiteSpace(options.StorageBucket),
        "La configuración de Supabase es inválida.")
    .ValidateOnStart();

var supabaseUrl = builder.Configuration[
    $"{SupabaseOptions.SectionName}:Url"]
    ?? throw new InvalidOperationException(
        "Falta la configuración Supabase:Url.");

var supabaseBaseUrl = supabaseUrl.TrimEnd('/');

builder.Services.AddHttpClient<IAuthService, SupabaseAuthService>(
    (serviceProvider, client) =>
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<SupabaseOptions>>().Value;

        client.BaseAddress = new Uri(
            $"{supabaseBaseUrl}/auth/v1/");

        client.DefaultRequestHeaders.Add(
            "apikey",
            options.PublishableKey);

        client.Timeout = TimeSpan.FromSeconds(30);
    });

builder.Services.AddHttpClient<
    IDocumentService,
    SupabaseDocumentService>(
    (serviceProvider, client) =>
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<SupabaseOptions>>().Value;

        client.BaseAddress = new Uri(
            $"{supabaseBaseUrl}/rest/v1/");

        client.DefaultRequestHeaders.Add(
            "apikey",
            options.PublishableKey);

        client.Timeout = TimeSpan.FromSeconds(30);
    });

builder.Services.AddHttpClient<
    IFileStorageService,
    SupabaseFileStorageService>(
    (serviceProvider, client) =>
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<SupabaseOptions>>().Value;

        client.BaseAddress = new Uri(
            $"{supabaseBaseUrl}/storage/v1/object/");

        client.DefaultRequestHeaders.Add(
            "apikey",
            options.PublishableKey);

        client.Timeout = TimeSpan.FromSeconds(60);
    });

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "__Host-MiProyecto.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Path = "/";

        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";

        options.ExpireTimeSpan = TimeSpan.FromMinutes(50);
        options.SlidingExpiration = false;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("AuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();