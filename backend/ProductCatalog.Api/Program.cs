using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Data;
using ProductCatalog.Api.Middleware;
using ProductCatalog.Api.Repositories;
using ProductCatalog.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── Services ────────────────────────────────────────────────────────────

builder.Services.AddDbContext<ProductCatalogDbContext>(options =>
    options.UseInMemoryDatabase("ProductCatalogDb"));

// DI: Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddSingleton<ICategoryRepository, InMemoryCategoryRepository>();

// DI: ProductSearchEngine — Singleton to maintain index across requests
builder.Services.AddSingleton<ProductSearchEngine>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Product Catalog API", Version = "v1" });
});

// CORS for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ─── Middleware Pipeline ─────────────────────────────────────────────────

app.UseExceptionHandling();   // Custom — outermost to catch all errors
app.UseRequestLogging();      // Custom — logs every request

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.MapControllers();

// ─── Seed DB & Initialize Search Index ───────────────────────────────────

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();
    context.Database.EnsureCreated();

    var searchEngine = app.Services.GetRequiredService<ProductSearchEngine>();
    var products = await context.Products.Include(p => p.Category).ToListAsync();
    searchEngine.UpdateIndex(products);
}

app.Run();