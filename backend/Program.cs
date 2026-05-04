using Backend.Services;
using Backend.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Registers MVC controllers so [ApiController] classes become HTTP endpoints.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register repository and service in dependency injection per request.
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

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
