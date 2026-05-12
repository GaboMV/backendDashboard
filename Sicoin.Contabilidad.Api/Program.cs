using Microsoft.EntityFrameworkCore;
using Sicoin.Contabilidad.Application.Interfaces;
using Sicoin.Contabilidad.Application.Services;
using Sicoin.Contabilidad.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Provisional CORS for local frontend testing
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register DbContext
builder.Services.AddDbContext<ContabilidadDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ContaDb")));

// Fix DI: Register IContabilidadDbContext
builder.Services.AddScoped<IContabilidadDbContext>(provider => provider.GetRequiredService<ContabilidadDbContext>());

// Register Services
builder.Services.AddScoped<IPlanCuentaService, PlanCuentaService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IPeriodoService, PeriodoService>();
builder.Services.AddScoped<IParametrizacionContableService, ParametrizacionContableService>();
builder.Services.AddScoped<ICentroCostoService, CentroCostoService>();
builder.Services.AddScoped<ILibrosContablesService, LibrosContablesService>();
builder.Services.AddScoped<IComprobanteService, ComprobanteService>();
builder.Services.AddScoped<PuctSeederService>();


var app = builder.Build();

// --- AUTO-PARCHE DE BASE DE DATOS PARA MONEDABASE ---
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ContabilidadDbContext>();
    try {
        // Ejecutamos SQL puro para asegurar que las columnas existen en PostgreSQL
        await context.Database.ExecuteSqlRawAsync("ALTER TABLE \"Gestiones\" ADD COLUMN IF NOT EXISTS \"MonedaBase\" TEXT DEFAULT 'BOB';");
        await context.Database.ExecuteSqlRawAsync("ALTER TABLE \"Gestiones\" ADD COLUMN IF NOT EXISTS \"EstadoId\" TEXT DEFAULT 'AC';");
        await context.Database.ExecuteSqlRawAsync("ALTER TABLE \"Periodos\" ADD COLUMN IF NOT EXISTS \"EstadoId\" TEXT DEFAULT 'AC';");
    } catch { /* Ignoramos si falla por permisos o si ya existe */ }
}
// -----------------------------------------------------

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
