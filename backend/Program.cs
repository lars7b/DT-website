using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Registers MVC controllers so [ApiController] classes become HTTP endpoints.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Read connection string from configuration (appsettings or environment variables).
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

// Registers product service in dependency injection per request.
builder.Services.AddScoped<IProductService>(provider => 
    new ProductService(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Maps all controller routes, for example /api/products.
app.MapControllers();

app.Run();
