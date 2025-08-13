using EcommerceChatbot.Data;
using EcommerceChatbot.Services;
using Microsoft.EntityFrameworkCore;
using EcommerceChatbot.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod();
    });
});

builder.Services.AddHttpClient<OpenRouterService>();
/*
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=products.db"));*/
builder.Configuration
    .AddEnvironmentVariables();
// Vérifier ce que Render lit comme chaîne de connexion
Console.WriteLine("DefaultConnection: " + builder.Configuration.GetConnectionString("DefaultConnection"));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
/*
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("https://mon-frontend.onrender.com")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
*/
builder.WebHost.UseUrls("http://*:8080");

var app = builder.Build();

// --- Application des migrations automatiquement ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.MapControllers();

app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    Console.WriteLine($"Requête reçue : {context.Request.Path}");
    await next();
});
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapGet("/form", async () =>
{
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "add-product.html");
    var html = await File.ReadAllTextAsync(filePath);
    return Results.Content(html, "text/html");
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
app.MapGet("/products", async (ApplicationDbContext db) =>
{
    var products = await db.Products.ToListAsync();
    return Results.Ok(products);
});
app.MapGet("/api/products", () => new { message = "Test réussi" });
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
app.UseCors("AllowFrontend");
app.UseCors();
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

