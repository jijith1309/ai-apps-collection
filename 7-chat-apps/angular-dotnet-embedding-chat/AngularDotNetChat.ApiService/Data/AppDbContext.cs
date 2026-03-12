using AngularDotNetChat.ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace AngularDotNetChat.ApiService.Data;

/// <summary>EF Core database context for the application.</summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserEmail)
            .IsUnique();

        modelBuilder.Entity<Document>()
            .HasOne(d => d.UploadedBy)
            .WithMany(u => u.Documents)
            .HasForeignKey(d => d.UploadedByUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DocumentChunk>()
            .HasOne(c => c.Document)
            .WithMany(d => d.Chunks)
            .HasForeignKey(c => c.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DocumentChunk>()
            .Property(c => c.EmbeddingJson)
            .HasColumnType("nvarchar(max)");
    }
}
