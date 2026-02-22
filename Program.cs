using InventarioAPI.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// EF Core InMemory (replace with real provider/connection when ready)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("InventarioDb"));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===== Render: escuchar el puerto que Render expone en la env PORT =====
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://*:{port}");
}

// ===== CORS =====
// Pol�tica por defecto (sin nombre) para que app.UseCors() aplique a todo.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Swagger 
app.UseSwagger();
app.UseSwaggerUI();

// CORS
app.UseCors();

app.UseAuthorization();

app.MapControllers();

// Ra�z y health
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/healthz", () => Results.Ok("Healthy"));

app.Run();