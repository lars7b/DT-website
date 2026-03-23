using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

/*
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(op =>
    op.UseSqlite("Data Source=doge.db"));
*/

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
    options.Cookie.Name = ".EmployeeApp.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});

// dependency injection
// every user gets their own database connection, destroyed after use.
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

var app = builder.Build();

app.UseCors("AllowReactApp"); 
app.UseSession(); // Enables middleware
app.MapControllers(); // Maps api routes

app.Run();

