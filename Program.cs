using InventarioAPI.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// EF Core PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Inventory service
builder.Services.AddScoped<InventarioAPI.Services.InventoryService>();
// SignalR
builder.Services.AddSignalR();

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

// Map SignalR hubs
app.MapHub<InventarioAPI.Hubs.InventoryHub>("/hubs/inventory");

// Ra�z y health
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/healthz", () => Results.Ok("Healthy"));

app.Run();