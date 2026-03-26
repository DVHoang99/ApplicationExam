using Microsoft.EntityFrameworkCore;
using WebAppExam.Infrastructure.Persistence.AppicationDbContext;
using WebAppExam.Infrastructure.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

// Configure Npgsql with the connection string from appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register the Unit of Work to manage repositories and transactions
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();