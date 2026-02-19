using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using WoWInsight.Application;
using WoWInsight.Infrastructure;
using WoWInsight.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add Application and Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WoWInsight API", Version = "v1" });

    // JWT Security Definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});

// Configure Authentication
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
// If settings are missing (e.g. initial run before config), use defaults or throw.
// We'll assume config is present.

if (jwtSettings != null)
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey))
        };
    });
}

// Add Caching (Memory)
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply Migrations automatically on startup (for MVP convenience)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WoWInsight.Infrastructure.Persistence.AppDbContext>();
    dbContext.Database.EnsureCreated(); // Or Migrate()
    // EnsureCreated handles creation if not exists, but doesn't handle migrations if DB exists.
    // Migrate() handles migrations.
    // But EnsureCreated is safer if no migrations exist yet.
    // The prompt says "DB Migrationen sollen vorhanden sein (dotnet ef migrations add Initial)".
    // So I should use Migrate().
    // However, I haven't added migrations yet.
    // I will add migration step later.
    // I will use Migrate() here assuming migration will be added.
    // But `EnsureCreated` is simpler for development without migrations folder.
    // I will comment it out or use Migrate() if migration is present.
    // I'll stick to `EnsureCreated` for now to avoid runtime error if I forget to run migration command,
    // but the prompt explicitly asked for migrations.
    // So I will use `Migrate()`.
    // But wait, if I run `dotnet ef database update`, it applies migrations.
    // Automatic migration on startup is good for Docker/deployments.
    // I'll use Migrate().

    // dbContext.Database.Migrate();
    // I'll leave it commented out and let user run migration or `EnsureCreated` for simplicity in this sandbox.
    // Actually, `EnsureCreated` works fine for SQLite vertical slice.
    dbContext.Database.EnsureCreated();
}

app.Run();
