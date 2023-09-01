using System;
using System.Collections.Generic;
using Main.Models;
using Microsoft.EntityFrameworkCore;

namespace Main.Data;

public partial class BetacomioContext : DbContext
{
    public BetacomioContext()
    {
    }

    public BetacomioContext(DbContextOptions<BetacomioContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Credential> Credentials { get; set; }

    public virtual DbSet<CreditCard> CreditCards { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //    => optionsBuilder.UseSqlServer("Data Source=.\\SQLEXPRESS;Initial Catalog=Betacomio;Integrated Security=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
