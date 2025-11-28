using FBMS_CumulusFlights.Data;
using FBMS_CumulusFlights.Models;
using FBMS_CumulusFlights.Managers; // Add this line
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FBMS_CumulusFlights.Models.Config;
using FBMS_CumulusFlights.Services;
using FBMS_CumulusFlights.Models.Config;
using System.Text.Json.Serialization;
using PayPalCheckoutSdk.Core;
using Microsoft.Extensions.DependencyInjection;



var builder = WebApplication.CreateBuilder(args);

// ========== DATABASE CONFIGURATION ==========
// Ensure the connection string is correct
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)); // Ensure your ApplicationDbContext is properly set up

////////////////////////////////////////////////////
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Use PascalCase
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
//////////////////////////////////////////

// ========== IDENTITY & AUTHENTICATION ==========
// Add Identity services with custom password and sign-in policies
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false; // Disable email confirmation
    options.User.RequireUniqueEmail = true;         // Enforce unique emails

    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Register the custom SignInManager if defined
builder.Services.AddScoped<SignInManager<ApplicationUser>, CustomSignInManager>();

// ========== COOKIE CONFIGURATION ==========
// Configure authentication cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// ========== ADDITIONAL SERVICES ==========
// Add Razor Pages and Controllers for the app
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IataDataService>();

// Configure TravelPayoutsAPI options (Ensure the API section is set up in your appsettings.json)
builder.Services.Configure<TravelPayoutsAPIOptions>(builder.Configuration.GetSection("TravelPayoutsAPI"));

// Register other services (Ensure FlightDataService and BackgroundService are implemented correctly)
// Use Scoped for FlightDataService
builder.Services.AddHttpClient<FlightDataService>(client =>
{
    client.BaseAddress = new Uri("https://api.travelpayouts.com/v2/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHostedService<FlightFetcherBackgroundService>(); // Register background service for flight fetching


// Add these lines under the existing TravelPayoutsAPI configuration
builder.Services.Configure<RapidAPIOptions>(builder.Configuration.GetSection("RapidAPIOptions"));
builder.Services.AddHttpClient<FlightDataService>(client =>
{
    client.BaseAddress = new Uri("https://iata-airports.p.rapidapi.com/airports/");
    client.DefaultRequestHeaders.Add("x-rapidapi-key", "528b395711msh2f3616df23d4ad7p12c282jsn16c947382b41"); // Replace with your actual API key
    client.DefaultRequestHeaders.Add("x-rapidapi-host", "iata-airports.p.rapidapi.com");
});

builder.Services.AddScoped<PayPalService>();
// Configure PayPal SDK
builder.Services.AddSingleton<PayPalHttpClient>(serviceProvider =>
{
var config = serviceProvider.GetRequiredService<IConfiguration>();
var clientId = config["PayPal:ClientId"];
var clientSecret = config["PayPal:ClientSecret"];

PayPalEnvironment environment;
if (config["PayPal:Environment"] == "live")
{
environment = new LiveEnvironment(clientId, clientSecret);
}
else
{
environment = new SandboxEnvironment(clientId, clientSecret);
}

// Create the PayPalHttpClient with the environment
return new PayPalHttpClient(environment);
});



builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdminOnly", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "UserType" && c.Value == "SuperAdmin")));
});





try
{
    var app = builder.Build(); // This is where the exception is likely occurring

    // ========== MIDDLEWARE CONFIGURATION ==========    
    // Development-specific middleware
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint(); // For migrations UI in development
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts(); // Enforce HTTP Strict Transport Security
    }


    // Use necessary middleware
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();


    app.UseAuthentication(); // Ensure authentication is enabled
    app.UseAuthorization();  // Ensure authorization is enabled

    // Map the default controller route
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    app.MapRazorPages();

    app.Run(); // Start the app
}
catch (AggregateException ex)
{
    Console.WriteLine("An error occurred during app startup:");
    foreach (var innerEx in ex.InnerExceptions)
    {
        Console.WriteLine($"Inner Exception: {innerEx.Message}");
        Console.WriteLine($"Stack Trace: {innerEx.StackTrace}");
    }
    throw;
}
