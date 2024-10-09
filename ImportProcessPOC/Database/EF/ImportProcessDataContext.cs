using ImportProcessPOC.Database.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace ImportProcessPOC.Database.EF;

public class ImportProcessDataContext : DbContext
{
    public DbSet<ItemDataModel> Items { get; set; }
    public DbSet<ImportJobDataModel> ImportJobs { get; set; }
    public DbSet<ImportJobSpanModel> ImportJobSpans { get; set; }
    public DbSet<ImportJobHeaderDataModel> ImportJobsHeaders { get; set; }
    public DbSet<ImportJobLineDataModel> ImportJobsLines { get; set; }
    public DbSet<ImportJobLineQueueDataModel> ImportJobsLineQueue { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = $"USER ID = postgres; " +
                               $"Password = admin; " +
                               $"Server = localhost; " +
                               $"Port = 5432; " +
                               $"Database = postgres; " +
                               $"SearchPath = importProcessPoc;";

        optionsBuilder.UseNpgsql(connectionString, x => { x.MigrationsHistoryTable("_migrationHistory", "importProcessPoc"); });

        //var consoleLoggerFactory = LoggerFactory.Create(builder =>
        //{
        //    builder.AddFilter((category, level) => category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information)
        //        .AddConsole();
        //});
        //optionsBuilder.UseLoggerFactory(consoleLoggerFactory);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ImportJobDataModel>(
            entity =>
            {
                entity.ToTable("ImportJob", "importProcessPoc");
                entity.HasKey(x => x.Id);

                entity.HasMany(x => x.Spans)
                      .WithOne(x => x.Job)
                      .HasForeignKey(x => x.JobId)
                      .HasPrincipalKey(x => x.Id);

                entity.HasMany(x => x.Headers)
                      .WithOne(x => x.Job)
                      .HasForeignKey(x => x.JobId)
                      .HasPrincipalKey(x => x.Id);

                entity.HasMany(x => x.Lines)
                      .WithOne(x => x.Job)
                      .HasForeignKey(x => x.JobId)
                      .HasPrincipalKey(x => x.Id);

                entity.HasMany(x => x.LineQueue)
                      .WithOne(x => x.Job)
                      .HasForeignKey(x => x.JobId)
                      .HasPrincipalKey(x => x.Id);
            });

        modelBuilder.Entity<ImportJobSpanModel>(
            entity =>
            {
                entity.ToTable("ImportJobSpan", "importProcessPoc");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id).ValueGeneratedOnAdd();
            });

        modelBuilder.Entity<ImportJobHeaderDataModel>(
            entity =>
            {
                entity.ToTable("ImportJobHeader", "importProcessPoc");
                entity.HasKey(x => new { x.JobId, x.Index });
            });

        modelBuilder.Entity<ImportJobLineDataModel>(
            entity =>
            {
                entity.ToTable("ImportJobLine", "importProcessPoc");
                entity.HasKey(x => new { x.JobId, x.Index });
            });

        modelBuilder.Entity<ImportJobLineQueueDataModel>(
            entity =>
            {
                entity.ToTable("ImportJobLineQueue", "importProcessPoc");
                entity.HasKey(x => new { x.JobId, x.Index });
                entity.HasIndex(x => new { x.ParentId, x.IsProcessed });
            });

        modelBuilder.Entity<ItemDataModel>(
            entity =>
            {
                entity.ToTable("Item", "importProcessPoc");
                entity.HasKey(x => new { x.Id, x.TenantId });

                entity.Property(x => x.Code).HasMaxLength(500);
                entity.Property(x => x.Name).HasMaxLength(500);

                entity.HasMany(x => x.Children)
                      .WithOne(x => x.Parent)
                      .HasForeignKey(x => new { x.ParentId, x.TenantId })
                      .HasPrincipalKey(x => new { x.Id, x.TenantId });
            });
    }
}