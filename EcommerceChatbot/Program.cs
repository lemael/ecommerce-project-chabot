using EcommerceChatbot.Data;
using EcommerceChatbot.Services;
using Microsoft.EntityFrameworkCore;
using EcommerceChatbot.Models;
using DotNetEnv;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Charger les variables d'environnement
DotNetEnv.Env.Load();

// Configuration des services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Configuration CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://ecommerce-project-2kvd.onrender.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddHttpClient<OpenRouterService>();

// Configuration de la base de données
builder.Configuration.AddEnvironmentVariables();

var connectionString = Environment.GetEnvironmentVariable("DefaultConnection") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("ERREUR: Aucune chaîne de connexion trouvée");
    throw new Exception("Configuration de base de données manquante");
}

try 
{
    var safeConnectionString = new NpgsqlConnectionStringBuilder(connectionString) 
    {
        Password = "*****"
    }.ToString();
    Console.WriteLine($"Configuration de connexion utilisée : {safeConnectionString}");

    builder.Services.AddDbContext<ApplicationDbContext>(options => 
    {
        options.UseNpgsql(connectionString);
        options.EnableSensitiveDataLogging();
        options.LogTo(Console.WriteLine, LogLevel.Information);
    });
}
catch (Exception ex) 
{
    Console.WriteLine($"ERREUR DE CONFIGURATION DB: {ex}");
    throw;
}

var app = builder.Build();

// Application des migrations
using (var scope = app.Services.CreateScope())
{
    try 
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (db.Database.GetPendingMigrations().Any())
        {
            Console.WriteLine("Application des migrations...");
            db.Database.Migrate();
        }
        
        if (!db.Database.CanConnect())
            throw new Exception("Échec de connexion à la base de données");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERREUR MIGRATION: {ex}");
        throw;
    }
}

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.UseAuthorization();

// Endpoints
app.MapControllers();
app.MapGet("/api/test", () => "API fonctionnelle");
app.MapGet("/debug", async (ApplicationDbContext db) => 
{
    try {
        return Results.Ok(new {
            dbStatus = await db.Database.CanConnectAsync(),
            tables = db.Model.GetEntityTypes().Select(e => e.GetTableName())
        });
    }
    catch (Exception ex) {
        return Results.Problem(ex.ToString());
    }
});

// Gestion des produits
app.MapGet("/products", async (ApplicationDbContext db) => 
    await db.Products.ToListAsync());

app.MapPost("/add-product", async (HttpRequest request, ApplicationDbContext db) =>
{
    var form = await request.ReadFormAsync();
    var product = new Product
    {
        Name = form["name"],
        Price = decimal.Parse(form["price"]),
        Quantity = int.Parse(form["quantity"])
    };
    
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Ok("Produit ajouté !");
});

// Gestion dynamique du port
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
try
{
    app.Run($"http://0.0.0.0:{port}");
    Console.WriteLine($"Application démarrée sur le port {port}");
}
catch (IOException ex) when (ex.InnerException is System.Net.Sockets.SocketException se && se.ErrorCode == 98)
{
    Console.WriteLine($"Le port {port} est occupé, tentative avec port aléatoire...");
    app.Run($"http://0.0.0.0:0"); // 0 = port aléatoire
}