using Microsoft.Identity.Web;
using Microsoft.EntityFrameworkCore;
using Sigasi.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────
// Autenticación con Microsoft Entra ID
// ─────────────────────────────────────────
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);

// ─────────────────────────────────────────
// Base de Datos - PostgreSQL
// ─────────────────────────────────────────
builder.Services.AddDbContext<SigasiDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─────────────────────────────────────────
// Controladores y OpenAPI
// ─────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ─────────────────────────────────────────
// CORS - permite llamadas desde el frontend
// ─────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",   // frontend local
                "http://frontend:3000"     // frontend en Docker
              )
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ─────────────────────────────────────────
// Pipeline
// ─────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health check básico para verificar que la API está viva
app.MapGet("/health", () => Results.Ok(new { status = "SIGASI API corriendo", timestamp = DateTime.UtcNow }));

app.Run();