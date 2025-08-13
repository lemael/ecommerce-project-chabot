using EcommerceChatbot.Data;
using EcommerceChatbot.Services;
using Microsoft.EntityFrameworkCore;
using EcommerceChatbot.Models;
using DotNetEnv;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Charger les variables d'environnement depuis .env si présent
DotNetEnv.Env.Load();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Configuration CORS unique
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",                   // Développement local
                "https://ecommerce-project-2kvd.onrender.com"  // Production Render
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddHttpClient<OpenRouterService>();

// Configuration de la base de données
builder.Configuration.AddEnvironmentVariables();

// Debug: Affiche la configuration de connexion
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
    
    Console.WriteLine("Configuration DbContext réussie");
}
catch (Exception ex) 
{
    Console.WriteLine($"ERREUR DE CONFIGURATION : {ex}");
    throw;
}

var app = builder.Build();

// --- Application des migrations automatiquement ---
using (var scope = app.Services.CreateScope())
{
    try 
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (db.Database.GetPendingMigrations().Any())
        {
            Console.WriteLine("Application des migrations...");
            db.Database.Migrate();
            Console.WriteLine("Migrations appliquées avec succès");
        }
        
        if (!db.Database.CanConnect())
            throw new Exception("Échec de connexion à la base de données");
        
        Console.WriteLine("Connexion DB vérifiée");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERREUR MIGRATION: {ex}");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middlewares dans le bon ordre
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors(); // Doit être après UseRouting et avant UseAuthorization
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
{
    var products = await db.Products.ToListAsync();
    return Results.Ok(products);
});

app.MapPost("/add-product", async (HttpRequest request, ApplicationDbContext db) =>
{
    var form = await request.ReadFormAsync();
    var name = form["name"];
    var price = decimal.Parse(form["price"]);
    var quantity = int.Parse(form["quantity"]);
    
    if (string.IsNullOrWhiteSpace(name) || price <= 0 || quantity < 0)
    {
        return Results.BadRequest("Données invalides");
    }

    var product = new Product
    {
        Name = name,
        Price = price,
        Quantity = quantity
    };

    db.Products.Add(product);
    await db.SaveChangesAsync();

    return Results.Ok("Produit ajouté !");
});

// Démarrer l'application
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
Console.WriteLine($"Démarrage de l'application sur le port {port}");
app.Run($"http://0.0.0.0:{port}");