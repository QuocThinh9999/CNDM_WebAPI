using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CNDM.Models;

public partial class DbDataMiningContext : DbContext
{
    public DbDataMiningContext()
    {
    }

    public DbDataMiningContext(DbContextOptions<DbDataMiningContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DanhSachPhoBien> DanhSachPhoBiens { get; set; }

    public virtual DbSet<HoaDon> HoaDons { get; set; }

    public virtual DbSet<SanPham> SanPhams { get; set; }

    public virtual DbSet<TanSuatHaiSanPham> TanSuatHaiSanPhams { get; set; }

    public virtual DbSet<TanSuatMotSanPham> TanSuatMotSanPhams { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("name=connectString");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DanhSachPhoBien>(entity =>
        {
            entity.HasKey(e => e.IdSanPham).HasName("PK__DanhSach__5FFA2D428430BFAF");
        });

        modelBuilder.Entity<HoaDon>(entity =>
        {
            entity.HasKey(e => e.IdHoaDon).HasName("PK__HoaDon__4DD461C8CF9A4F94");
        });

        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.HasKey(e => e.IdSanPham).HasName("PK__SanPham__5FFA2D429839A6B4");
        });

        modelBuilder.Entity<TanSuatHaiSanPham>(entity =>
        {
            entity.Property(e => e.ThuTu).ValueGeneratedNever();
        });

        modelBuilder.Entity<TanSuatMotSanPham>(entity =>
        {
            entity.HasKey(e => e.ThuTu).HasName("PK__TanSuatM__2E2833D0505D9125");

            entity.Property(e => e.ThuTu).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
