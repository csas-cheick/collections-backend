using System.Text.Json.Serialization;
using System.Text.Json;
using backend.Data;
using backend.Services;
using backend.Services.IServices;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;

public partial class Program
{
    public static void Main(string[] args)
    {
        // Charger les variables d'environnement depuis le fichier .env
        Env.Load();

        var builder = WebApplication.CreateBuilder(args);

        // Construire la chaîne de connexion à partir des variables d'environnement
        var connectionString = $"Server={Environment.GetEnvironmentVariable("DB_SERVER")};Port={Environment.GetEnvironmentVariable("DB_PORT")};Database={Environment.GetEnvironmentVariable("DB_DATABASE")};User={Environment.GetEnvironmentVariable("DB_USER")};Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};SslMode={Environment.GetEnvironmentVariable("DB_SSL_MODE")};";

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("La chaîne de connexion à la base de données est introuvable.");
        }

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)
            ));

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend",
                policy => policy.WithOrigins("http://localhost:5173","https://collections-f6e2.onrender.com","https://collections-f6e2.onrender.com")
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials());
        });

        builder.Services.AddScoped<EmailService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
        builder.Services.AddScoped<IModeleService, ModeleService>();
        builder.Services.AddScoped<ITransactionService, TransactionService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ICustomerService, CustomerService>();
        builder.Services.AddScoped<IOrderService, OrderService>();

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();
        app.Urls.Add("http://0.0.0.0:5120");

        // Configuration middleware dans le bon ordre
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("AllowFrontend");
        app.MapControllers();

        app.Run();
    }
}
