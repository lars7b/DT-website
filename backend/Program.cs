using Backend.Repositories;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// session config
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".StoreApp.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});

// dependency injection
// every user gets their own database connection, destroyed after use.
builder.Services.AddScoped<CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

var app = builder.Build();

app.UseCors("AllowReactApp"); 
app.UseSession(); // Enables middleware
app.MapControllers(); // Maps api routes

app.Run();

