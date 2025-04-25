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

    public virtual DbSet<History> Histories { get; set; }

    public virtual DbSet<Link> Links { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Directory>(entity =>
        {
            entity.HasOne(d => d.OldDir).WithMany(p => p.InverseOldDir)
                .HasForeignKey(d => d.OldDirId)
                .HasConstraintName("FK_Directories_Directories1");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_Directories_Directories");
        });

        modelBuilder.Entity<File>(entity =>
        {
            entity.Property(e => e.Size).HasColumnType("decimal(18, 6)");

            entity.HasOne(d => d.Directory).WithMany(p => p.FileDirectories)
                .HasForeignKey(d => d.DirectoryId)
                .HasConstraintName("FK_Files_Directories");

            entity.HasOne(d => d.OldDir).WithMany(p => p.FileOldDirs)
                .HasForeignKey(d => d.OldDirId)
                .HasConstraintName("FK_Files_Directories1");
        });

        modelBuilder.Entity<History>(entity =>
        {
            entity.ToTable("History");

            entity.Property(e => e.Date).HasColumnType("datetime");

            entity.HasOne(d => d.Directory).WithMany(p => p.Histories)
                .HasForeignKey(d => d.DirectoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_History_Directories");

            entity.HasOne(d => d.User).WithMany(p => p.Histories)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_History_Users");
        });

        modelBuilder.Entity<Link>(entity =>
        {
            entity.HasOne(d => d.Dir).WithMany(p => p.Links)
                .HasForeignKey(d => d.DirId)
                .HasConstraintName("FK_Links_Directories");

            entity.HasOne(d => d.File).WithMany(p => p.Links)
                .HasForeignKey(d => d.FileId)
                .HasConstraintName("FK_Links_Files");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasOne(d => d.RootDir).WithMany(p => p.UserRootDirs)
                .HasForeignKey(d => d.RootDirId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Directories");

            entity.HasOne(d => d.TrashDir).WithMany(p => p.UserTrashDirs)
                .HasForeignKey(d => d.TrashDirId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Directories1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
