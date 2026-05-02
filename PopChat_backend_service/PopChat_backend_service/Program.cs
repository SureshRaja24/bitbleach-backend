using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PopChat.Backend.Services.Hubs;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. SERVICES
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<JwtService>();

// 2. JWT AUTHENTICATION SETUP
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        // Ensure this string matches your JwtService exactly!
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("PopChat_Super_Secure_Secret_Key_2026_Suresh")),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        RequireExpirationTime = true,
        ClockSkew = TimeSpan.FromMinutes(2) // 2-minute grace period for time sync
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/popchat"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"[AUTH ERROR]: {context.Exception.Message}");
            return Task.CompletedTask;
        }
    };
});

// 3. CORS POLICY
builder.Services.AddCors(options => {
    options.AddPolicy("BitBleachPolicy", policy => {
        policy.WithOrigins("https://bitbleach.web.app", "https://bitbleach.firebaseapp.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // CRITICAL for SignalR
    });
});

var app = builder.Build();

// 4. MIDDLEWARE PIPELINE
if (app.Environment.IsDevelopment()) { app.MapOpenApi(); }

app.UseRouting();
app.UseCors(); // Must be before Auth
app.UseAuthentication();
app.UseAuthorization();

// 5. HUB MAPPING
app.MapHub<PopHub>("/hubs/popchat");
app.MapControllers();

app.Run();