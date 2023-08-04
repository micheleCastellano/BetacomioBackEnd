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

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Data Source=.\\SQLEXPRESS;Initial Catalog=Betacomio;Integrated Security=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
