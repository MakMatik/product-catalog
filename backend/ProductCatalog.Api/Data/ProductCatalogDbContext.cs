using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Models;

namespace ProductCatalog.Api.Data;

public class ProductCatalogDbContext : DbContext
{
    public ProductCatalogDbContext(DbContextOptions<ProductCatalogDbContext> options)
        : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Description).HasMaxLength(500);

            entity.HasOne(c => c.ParentCategory)
                  .WithMany(c => c.SubCategories)
                  .HasForeignKey(c => c.ParentCategoryId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired(false);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).HasMaxLength(2000);
            entity.Property(p => p.SKU).IsRequired().HasMaxLength(50);
            entity.Property(p => p.Price).HasPrecision(18, 2);

            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electronics", Description = "Electronic devices and gadgets" },
            new Category { Id = 2, Name = "Laptops", Description = "Portable computers", ParentCategoryId = 1 },
            new Category { Id = 3, Name = "Smartphones", Description = "Mobile phones", ParentCategoryId = 1 },
            new Category { Id = 4, Name = "Clothing", Description = "Apparel and accessories" },
            new Category { Id = 5, Name = "Men's Wear", Description = "Clothing for men", ParentCategoryId = 4 },
            new Category { Id = 6, Name = "Women's Wear", Description = "Clothing for women", ParentCategoryId = 4 },
            new Category { Id = 7, Name = "Books", Description = "Physical and digital books" },
            new Category { Id = 8, Name = "Fiction", Description = "Fiction books", ParentCategoryId = 7 },
            new Category { Id = 9, Name = "Non-Fiction", Description = "Non-fiction books", ParentCategoryId = 7 }
        );

        // Seed products
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "MacBook Pro 16\"", Description = "Apple MacBook Pro with M3 chip", SKU = "ELEC-LAP-001", Price = 2499.99m, Quantity = 25, CategoryId = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Product { Id = 2, Name = "Dell XPS 15", Description = "Dell XPS 15 with Intel i7", SKU = "ELEC-LAP-002", Price = 1799.99m, Quantity = 30, CategoryId = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Product { Id = 3, Name = "ThinkPad X1 Carbon", Description = "Lenovo ThinkPad X1 Carbon Gen 11", SKU = "ELEC-LAP-003", Price = 1649.99m, Quantity = 20, CategoryId = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Product { Id = 4, Name = "iPhone 15 Pro", Description = "Apple iPhone 15 Pro 256GB", SKU = "ELEC-PHN-001", Price = 1199.99m, Quantity = 50, CategoryId = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Product { Id = 5, Name = "Samsung Galaxy S24", Description = "Samsung Galaxy S24 Ultra", SKU = "ELEC-PHN-002", Price = 1099.99m, Quantity = 40, CategoryId = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Product { Id = 6, Name = "Classic Oxford Shirt", Description = "Men's formal Oxford cotton shirt", SKU = "CLO-MEN-001", Price = 59.99m, Quantity = 100, CategoryId = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Product { Id = 7, Name = "Slim Fit Chinos", Description = "Men's slim fit chino trousers", SKU = "CLO-MEN-002", Price = 79.99m, Quantity = 75, CategoryId = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Product { Id = 8, Name = "Floral Summer Dress", Description = "Women's floral pattern summer dress", SKU = "CLO-WOM-001", Price = 89.99m, Quantity = 60, CategoryId = 6, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Product { Id = 9, Name = "The Great Gatsby", Description = "Classic novel by F. Scott Fitzgerald", SKU = "BK-FIC-001", Price = 12.99m, Quantity = 200, CategoryId = 8, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Product { Id = 10, Name = "Sapiens", Description = "A Brief History of Humankind by Yuval Noah Harari", SKU = "BK-NF-001", Price = 18.99m, Quantity = 150, CategoryId = 9, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
    }
}