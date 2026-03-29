using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpOverrides;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using TalentVerse.WebAPI.Configuration;
using TalentVerse.WebAPI.Data;
using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.Hubs;
using TalentVerse.WebAPI.Interfaces;
using TalentVerse.WebAPI.Repositories;
using TalentVerse.WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Data Protection for OAuth state cookies with persistent keys
var keysDirectory = Path.Combine(builder.Environment.ContentRootPath, "keys");
Directory.CreateDirectory(keysDirectory); // Ensure directory exists

builder.Services.AddDataProtection()
    .SetApplicationName("TalentVerse")
    .PersistKeysToFileSystem(new DirectoryInfo(keysDirectory));

// Environment-aware cookie configuration for OAuth
// Development: SameSite=Lax + SameAsRequest (works on HTTP)
// Production: SameSite=None + Always (requires HTTPS)
var isDevelopment = builder.Environment.IsDevelopment();
var cookieSameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.None;
var cookieSecurePolicy = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "TalentVerse API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// Add memory cache for 2FA code storage
builder.Services.AddMemoryCache();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton<DapperContext>();
builder.Services.AddScoped<ISkillRepository, SkillRepository>();
builder.Services.AddScoped<IProposalRepository, ProposalRepository>();
builder.Services.AddScoped<IMarketplaceRepository, MarketplaceRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISkillService, SkillService>(); 
builder.Services.AddScoped<IProposalService, ProposalService>();
builder.Services.AddScoped<IMarketplaceService, MarketplaceService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IEmailService, EmailService>(); 
builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

// Appointment scheduling with Google Calendar
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();

// Credit / Economy / Gamification
builder.Services.AddScoped<ICreditRepository, CreditRepository>();
builder.Services.AddScoped<ICreditService, CreditService>();
builder.Services.AddScoped<IBadgeRepository, BadgeRepository>();
builder.Services.AddScoped<IBadgeService, BadgeService>();

// Identity Verification
builder.Services.AddScoped<IVerificationRepository, VerificationRepository>();
builder.Services.AddScoped<IVerificationService, VerificationService>();

// Admin Panel
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IAdminService, AdminService>();

// Register background email queue service
builder.Services.AddSingleton<IEmailQueueService, BackgroundEmailQueueService>();
builder.Services.AddHostedService<BackgroundEmailQueueService>(provider =>
    provider.GetRequiredService<IEmailQueueService>() as BackgroundEmailQueueService
    ?? throw new InvalidOperationException("IEmailQueueService must be BackgroundEmailQueueService"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // Get frontend URL from configuration
        var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost:3000";
        
        policy.SetIsOriginAllowed(origin =>
              {
                  if (string.IsNullOrWhiteSpace(origin)) return false;
                  if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)) return false;
                  
                  // Allow configured frontend URL
                  if (origin.TrimEnd('/').Equals(frontendUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                      return true;
                  
                  // Allow local development servers
                  return uri.Host is "localhost" or "127.0.0.1";
              })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddSignalR();

// Configure options from appsettings
builder.Services.Configure<AppConfigOptions>(
    builder.Configuration.GetSection("AppConfig"));
builder.Services.Configure<RateLimitingOptions>(
    builder.Configuration.GetSection("RateLimiting"));
builder.Services.Configure<StripeSettings>(
    builder.Configuration.GetSection("Stripe"));

// Initialize Stripe with the secret key (fully-qualified to avoid ambiguity with TokenService)
global::Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Add rate limiting configuration using settings
var rateLimitConfig = builder.Configuration.GetSection("RateLimiting").Get<RateLimitingOptions>() 
    ?? new RateLimitingOptions();

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("fixed", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitConfig.PermitLimit,
                Window = TimeSpan.FromMinutes(rateLimitConfig.WindowMinutes),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
    
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentityCore<AppUser>(options =>
{
    // For now simple Password policy is used, can be enhanced later with better policy
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
    options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
})
    .AddRoles<IdentityRole>()
    .AddSignInManager<SignInManager<AppUser>>() // Add SignInManager for external authentication
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Add authentication with JWT as default, and Identity cookie schemes for OAuth
var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var tokenKey = builder.Configuration["JWT:TokenKey"]
                   ?? throw new Exception("JWT:TokenKey is missing from appsettings.json");

    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = false, //It is set to false for development purpose, can be enabled later
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey))
    };
    // Allow JWT token from query string for SignalR WebSocket connections
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
})
// Add Identity cookie schemes for OAuth external authentication
.AddCookie(IdentityConstants.ApplicationScheme, options =>
{
    options.Cookie.Name = ".AspNetCore.Identity.Application";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = cookieSecurePolicy;
    options.Cookie.SameSite = cookieSameSite;
    options.Cookie.IsEssential = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
})
// Add Identity.External cookie scheme (required by SignInManager.GetExternalLoginInfoAsync)
.AddCookie(IdentityConstants.ExternalScheme, options =>
{
    options.Cookie.Name = ".AspNetCore.Identity.External";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = cookieSecurePolicy;
    options.Cookie.SameSite = cookieSameSite;
    options.Cookie.IsEssential = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(10); // Short-lived for OAuth flow
    options.SlidingExpiration = false;
});

// Configure OAuth providers - only register if credentials exist

// Google OAuth
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.CallbackPath = "/signin-google";
        options.SaveTokens = true;
        options.SignInScheme = IdentityConstants.ExternalScheme;
        
        // Environment-aware cookie configuration for OAuth state
        options.CorrelationCookie.SameSite = cookieSameSite;
        options.CorrelationCookie.SecurePolicy = cookieSecurePolicy;
        options.CorrelationCookie.HttpOnly = true;
        options.CorrelationCookie.IsEssential = true;
        options.CorrelationCookie.Path = "/";
    });
}

// GitHub OAuth
var githubClientId = builder.Configuration["Authentication:GitHub:ClientId"];
var githubClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"];
if (!string.IsNullOrWhiteSpace(githubClientId) && !string.IsNullOrWhiteSpace(githubClientSecret))
{
    authBuilder.AddGitHub(options =>
    {
        options.ClientId = githubClientId;
        options.ClientSecret = githubClientSecret;
        options.CallbackPath = "/signin-github";
        options.SaveTokens = true;
        options.SignInScheme = IdentityConstants.ExternalScheme;
        options.Scope.Add("user:email");
        
        // Environment-aware cookie configuration for OAuth state
        options.CorrelationCookie.SameSite = cookieSameSite;
        options.CorrelationCookie.SecurePolicy = cookieSecurePolicy;
        options.CorrelationCookie.HttpOnly = true;
        options.CorrelationCookie.IsEssential = true;
        options.CorrelationCookie.Path = "/";
    });
}

builder.Services.AddAuthorization();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var logger = services.GetRequiredService<ILogger<Program>>(); 

try
{
    var context = services.GetRequiredService<AppDbContext>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    await context.Database.MigrateAsync();

    await Seed.SeedUsers(userManager, roleManager, logger);
    await Seed.SeedBadges(context, logger);
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred during migration/seeding");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable HTTPS redirection only in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Environment-aware cookie policy for OAuth
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = app.Environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
    Secure = app.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always
});

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseCors("AllowFrontend");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

using (var migrationscope = app.Services.CreateScope())
{
    var db = migrationscope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();

