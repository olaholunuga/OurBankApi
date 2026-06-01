using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OurBankApi.Models;

namespace OurBankApi.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<BankAccount> BankAccounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // builder.Entity<ApplicationUser>()
        //     .Property(u => u.CustomProperty)
        //     .IsRequired();

        builder.Entity<Transaction>()
            .HasOne(t => t.SourceAccount)
            .WithMany(a => a.SentTransactions)
            .HasForeignKey(t => t.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<Transaction>()
            .HasOne(t => t.DestinationAccount)
            .WithMany(a => a.ReceivedTransactions)
            .HasForeignKey(t => t.DestinationAccountId)
            .OnDelete(DeleteBehavior.Restrict);
        
    }
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        if (Database.ProviderName == "MicrosoftEntityFramewortCore.Sqlite")
        {
            configurationBuilder.Properties<DateTime>()
                .HaveConversion<string>();
            
            configurationBuilder.Properties<DateTime>()
                .HaveConversion<string?>();
        }
    }
}