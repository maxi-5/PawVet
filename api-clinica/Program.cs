using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using PawVetClinica.Middleware;
using PawVetClinica.Models;
using PawVetClinica.Repositories;
using PawVetClinica.Services;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ================== SECRETOS (variables de entorno) ==================
// La cadena de conexion y la llave JWT NO estan en appsettings.json.
// Se leen de las variables de entorno: ConnectionStrings__ClinicaDB y Jwt__Key (ver .env.example)
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "Falta la variable de entorno Jwt__Key (minimo 32 caracteres). Ver README.md");
}

// ================== BASE DE DATOS (Entity Framework Core) ==================
// Scoped (por defecto): un DbContext por cada peticion HTTP
if (!builder.Environment.IsEnvironment("Testing"))
{
    var conexion = builder.Configuration.GetConnectionString("ClinicaDB");
    if (string.IsNullOrWhiteSpace(conexion))
    {
        throw new InvalidOperationException(
            "Falta la variable de entorno ConnectionStrings__ClinicaDB. Ver README.md");
    }

    builder.Services.AddDbContext<ClinicaDbContext>(options =>
        options.UseSqlServer(conexion));
}

// ================== INYECCION DE DEPENDENCIAS ==================
// Repositorios -> Scoped: usan el DbContext, que vive lo mismo que la peticion
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ITutorRepository, TutorRepository>();
builder.Services.AddScoped<IMascotaRepository, MascotaRepository>();
builder.Services.AddScoped<ICatalogoRepository, CatalogoRepository>();
// Servicio de tokens -> Singleton: no guarda estado y solo lee la configuracion
builder.Services.AddSingleton<ITokenService, TokenService>();

// ================== AUTENTICACION JWT ==================
// Esta API EMITE el token (TokenService). La API Agenda lo valida con la misma llave (ADR-002).
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// ================== CORS (permite que el cliente React llame a la API) ==================
// 5173 = cliente en desarrollo (npm run dev), 3000 = cliente en Docker
var origenes = (builder.Configuration["Cors:Origenes"] ?? "http://localhost:5173,http://localhost:3000")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
builder.Services.AddCors(options =>
{
    options.AddPolicy("ClienteReact", politica =>
        politica.WithOrigins(origenes).AllowAnyHeader().AllowAnyMethod());
});

// ================== ERRORES ESTRUCTURADOS (ProblemDetails) ==================
// hace que los 401, 403 y 404 sin cuerpo tambien respondan con el modelo ProblemDetails
builder.Services.AddProblemDetails();

// ================== VERSIONAMIENTO (por URL: /api/v1/...) ==================
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true; // agrega el header api-supported-versions
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";      // grupo "v1" para swagger
    options.SubstituteApiVersionInUrl = true; // muestra /api/v1/... en vez de /api/v{version}/...
});

builder.Services.AddControllers();

// configuramos los endpoints
builder.Services.AddEndpointsApiExplorer();
// generamos el documento con swagger, con el boton Authorize para probar las apis bloqueadas
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PawVet - API Clinica e Identidad",
        Version = "v1",
        Description = "Usuarios, roles y login (emite el JWT), tutores, mascotas, veterinarios, especies y razas. " +
                      "Roles: Administrador, Recepcionista y Veterinario."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pega aqui SOLO el token obtenido en POST /api/v1/auth/login"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });

    // usa los comentarios /// <summary> de los controladores en la documentacion
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// aplica las migraciones y crea el usuario Administrador inicial si no existe
await DbSeeder.InicializarAsync(app.Services, app.Configuration, app.Logger);

// ================== PIPELINE (orden de los middleware) ==================
app.UseMiddleware<ErrorHandlerMiddleware>(); // 1. captura errores de todo lo que viene despues
app.UseMiddleware<LoggingMiddleware>();      // 2. registra cada peticion
app.UseStatusCodePages();                    // 3. cuerpo ProblemDetails para 401, 403 y 404 vacios

// Swagger siempre disponible en /swagger (entorno academico)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "PawVet Clinica v1");
    options.DocumentTitle = "PawVet - API Clinica e Identidad";
});

app.UseCors("ClienteReact");

// se habilitan los permisos
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// GET: /health -> estado de la API y de su base de datos (publico)
app.MapGet("/health", async (ClinicaDbContext context) =>
    await context.Database.CanConnectAsync()
        ? Results.Ok(new { estado = "Healthy" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable))
    .AllowAnonymous();

app.Run();

// necesario para que el proyecto de pruebas de integracion pueda levantar la API
public partial class Program { }
