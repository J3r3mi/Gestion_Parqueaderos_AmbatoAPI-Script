using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartParking.Api.BackgroundServices;
using SmartParking.Api.Data;
using SmartParking.Api.Services;
using SmartParking.Api.Services.Routing;

var builder = WebApplication.CreateBuilder(args);

// ---------------- MySQL / Entity Framework Core ----------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString) || connectionString.StartsWith("SET_VIA"))
{
    connectionString = "Server=localhost;Port=3306;Database=smart_parking;User=root;TreatTinyAsBoolean=true;";
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ---------------- Servicios propios ----------------
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IQrService, QrService>();
builder.Services.AddScoped<IReservaService, ReservaService>();
builder.Services.AddScoped<IParqueaderoService, ParqueaderoService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IRoutingService, RoutingService>();
builder.Services.AddHostedService<ExpiracionReservasService>();

// ---------------- Autenticación JWT ----------------
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.StartsWith("SET_VIA"))
{
    jwtKey = "MiClaveSuperSecretaYMuySeguraParaSmartParking2026*";
}

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
        ValidIssuer = jwtSection["Issuer"] ?? "https://api.smartparking-ambato.local",
        ValidAudience = jwtSection["Audience"] ?? "https://app.smartparking-ambato.local",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromMinutes(2)
    };
});

builder.Services.AddAuthorization();

// ---------------- Rate limiting (nativo de .NET 8, sin paquete extra) ----------------
// Protege endpoints sensibles a fuerza bruta / abuso: login y recuperación de contraseña.
// Se limita POR IP, con una ventana fija — pasado el límite, responde 429 Too Many Requests.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.PermitLimit = 5;                    // 5 intentos...
        opt.Window = TimeSpan.FromMinutes(1);    // ...por minuto...
        opt.QueueLimit = 0;                      // ...sin cola: el 6to intento se rechaza de inmediato
    });

    options.AddFixedWindowLimiter("recuperacion-password", opt =>
    {
        opt.PermitLimit = 3;                     // más estricto: pedir tokens repetidamente es más "gratis" de abusar
        opt.Window = TimeSpan.FromMinutes(5);
        opt.QueueLimit = 0;
    });
});

// ---------------- CORS para el cliente Ionic/Angular ----------------
// "ionic serve" corre normalmente en localhost:8100; el build de Capacitor
// usa esquemas propios (capacitor://, http://localhost) según la plataforma.
builder.Services.AddCors(options =>
{
    options.AddPolicy("IonicClient", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ---------------- Controllers + Swagger ----------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Smart Parking Ambato API", Version = "v1" });

    // Botón "Authorize" en Swagger para probar endpoints protegidos con JWT
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingresa: Bearer {tu token}"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("IonicClient");

app.UseAuthentication(); // primero autentica (valida el token)
app.UseAuthorization();  // luego autoriza (revisa el rol/claims)

app.UseRateLimiter();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapControllers();

// Auto-actualizar hashes de contraseñas de usuarios demo si tienen el placeholder
DbInitializer.Initialize(app);

app.Run();
