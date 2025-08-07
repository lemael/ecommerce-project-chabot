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
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=products.db"));
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


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.UseHttpsRedirection();

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
app.MapGet("/form", async () =>
{
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "www", "add-product.html");
    var html = await File.ReadAllTextAsync(filePath);
    return Results.Content(html, "text/html");
});

app.MapPost("/add-product", async (HttpRequest request, ApplicationDbContext db) =>
{
    var form = await request.ReadFormAsync();
    var name = form["name"];
    var price = decimal.Parse(form["price"]);
    var quantity = int.Parse(form["quantity"]);

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

app.UseCors("AllowFrontend");
app.UseCors();
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

