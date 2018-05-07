using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace blogs_mysql
{
    public partial class blogsContext : DbContext
    {
        public virtual DbSet<Blogposts> Blogposts { get; set; }
        public virtual DbSet<Blogs> Blogs { get; set; }
        public virtual DbSet<People> People { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseMySql("Server=localhost;User Id=root;Password=podsaveamerica;Database=blogs");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blogposts>(entity =>
            {
                entity.ToTable("blogposts");

                entity.HasIndex(e => e.AuthorId)
                    .HasName("author_id");

                entity.HasIndex(e => e.BlogId)
                    .HasName("blog_id");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("binary(16)")
                    .HasDefaultValueSql("'0x'");

                entity.Property(e => e.AuthorId)
                    .IsRequired()
                    .HasColumnName("author_id")
                    .HasColumnType("binary(16)")
                    .HasDefaultValueSql("'0x'");

                entity.Property(e => e.BlogId)
                    .IsRequired()
                    .HasColumnName("blog_id")
                    .HasColumnType("binary(16)")
                    .HasDefaultValueSql("'0x'");

                entity.Property(e => e.Content)
                    .HasColumnName("content")
                    .HasMaxLength(1080);

                entity.Property(e => e.DatePosted)
                    .HasColumnName("date_posted")
                    .HasColumnType("datetime");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnName("title")
                    .HasMaxLength(50)
                    .HasDefaultValueSql("''");

                entity.HasOne(d => d.Author)
                    .WithMany(p => p.Blogposts)
                    .HasForeignKey(d => d.AuthorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("blogposts_ibfk_1");

                entity.HasOne(d => d.Blog)
                    .WithMany(p => p.Blogposts)
                    .HasForeignKey(d => d.BlogId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("blogposts_ibfk_2");
            });

            modelBuilder.Entity<Blogs>(entity =>
            {
                entity.ToTable("blogs");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("binary(16)")
                    .HasDefaultValueSql("'0x'");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(50)
                    .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<People>(entity =>
            {
                entity.ToTable("people");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("binary(16)")
                    .HasDefaultValueSql("'0x'");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasColumnName("first_name")
                    .HasMaxLength(50)
                    .HasDefaultValueSql("''");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasColumnName("last_name")
                    .HasMaxLength(75)
                    .HasDefaultValueSql("''");
            });
        }
    }
}
