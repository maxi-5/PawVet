using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using PawVetAgenda.Middleware;
using PawVetAgenda.Models;
using PawVetAgenda.Repositories;
using PawVetAgenda.Services;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ================== SECRETOS (variables de entorno) ==================
// La cadena de conexion y la llave JWT NO estan en appsettings.json.
// Se leen de las variables de entorno: ConnectionStrings__AgendaDB y Jwt__Key (ver .env.example)
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
    var conexion = builder.Configuration.GetConnectionString("AgendaDB");
    if (string.IsNullOrWhiteSpace(conexion))
    {
        throw new InvalidOperationException(
            "Falta la variable de entorno ConnectionStrings__AgendaDB. Ver README.md");
    }

    builder.Services.AddDbContext<AgendaDbContext>(options =>
        options.UseSqlServer(conexion));
}

// ================== INYECCION DE DEPENDENCIAS ==================
// Repositorios -> Scoped: usan el DbContext, que vive lo mismo que la peticion
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICitaRepository, CitaRepository>();
builder.Services.AddScoped<IAtencionRepository, AtencionRepository>();
builder.Services.AddScoped<IVacunacionRepository, VacunacionRepository>();

// ================== INTEGRACION ENTRE SERVICIOS (ADR-003) ==================
// Clientes HTTP tipados creados por IHttpClientFactory.
// API Clinica: si no responde en 5 segundos, la llamada se cancela y la API responde 503.
builder.Services.AddHttpClient<IClinicaClient, ClinicaClient>(cliente =>
{
    cliente.BaseAddress = new Uri(builder.Configuration["Servicios:ApiClinica"] ?? "http://localhost:5001/");
    cliente.Timeout = TimeSpan.FromSeconds(5);
});
// API publica de feriados (Nager.Date): 3 segundos; si falla, se continua sin validar.
builder.Services.AddMemoryCache(); // cache de 24 h para los feriados
builder.Services.AddHttpClient<IFeriadosClient, FeriadosClient>(cliente =>
{
    cliente.BaseAddress = new Uri(builder.Configuration["Servicios:ApiFeriados"] ?? "https://date.nager.at/");
    cliente.Timeout = TimeSpan.FromSeconds(3);
});

// ================== AUTENTICACION JWT ==================
// Esta API NO emite tokens: VALIDA el JWT que emite la API Clinica,
// con la misma llave (Jwt__Key), el mismo Issuer y el mismo Audience (ADR-002).
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
        Title = "PawVet - API Agenda y Atencion",
        Version = "v1",
        Description = "Citas, atenciones clinicas y vacunaciones. Valida el JWT emitido por la API Clinica (puerto 5001) " +
                      "y consume la API Clinica y la API publica de feriados de Chile."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pega aqui SOLO el token obtenido en la API Clinica (POST http://localhost:5001/api/v1/auth/login)"
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

// aplica las migraciones pendientes
await DbSeeder.InicializarAsync(app.Services, app.Logger);

// ================== PIPELINE (orden de los middleware) ==================
app.UseMiddleware<ErrorHandlerMiddleware>(); // 1. captura errores de todo lo que viene despues
app.UseMiddleware<LoggingMiddleware>();      // 2. registra cada peticion
app.UseStatusCodePages();                    // 3. cuerpo ProblemDetails para 401, 403 y 404 vacios

// Swagger siempre disponible en /swagger (entorno academico)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "PawVet Agenda v1");
    options.DocumentTitle = "PawVet - API Agenda y Atencion";
});

app.UseCors("ClienteReact");

// se habilitan los permisos
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// GET: /health -> estado de la API y de su base de datos (publico)
app.MapGet("/health", async (AgendaDbContext context) =>
    await context.Database.CanConnectAsync()
        ? Results.Ok(new { estado = "Healthy" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable))
    .AllowAnonymous();

app.Run();

// necesario para que el proyecto de pruebas de integracion pueda levantar la API
public partial class Program { }
