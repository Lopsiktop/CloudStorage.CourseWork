using CloudStorage.Data.Models;
using Microsoft.EntityFrameworkCore;
using Directory = CloudStorage.Data.Models.Directory;
using File = CloudStorage.Data.Models.File;

namespace CloudStorage.Data.Contexts;

public partial class CloudStorageContext : DbContext
{
    public CloudStorageContext()
    {
    }

    public CloudStorageContext(DbContextOptions<CloudStorageContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Directory> Directories { get; set; }

    public virtual DbSet<File> Files { get; set; }

    public virtual DbSet<Link> Links { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Directory>(entity =>
        {
            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_Directories_Directories")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<File>(entity =>
        {
            entity.Property(e => e.Size).HasColumnType("decimal(18, 6)");

            entity.HasOne(d => d.Directory).WithMany(p => p.Files)
                .HasForeignKey(d => d.DirectoryId)
                .HasConstraintName("FK_Files_Directories")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Link>(entity =>
        {
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");

            entity.HasOne(d => d.Directory).WithMany(p => p.Links)
                .HasForeignKey(d => d.DirectoryId)
                .HasConstraintName("FK_Links_Directories");

            entity.HasOne(d => d.File).WithMany(p => p.Links)
                .HasForeignKey(d => d.FileId)
                .HasConstraintName("FK_Links_Files");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasOne(d => d.RootDir).WithMany(p => p.Users)
                .HasForeignKey(d => d.RootDirId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Directories");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
