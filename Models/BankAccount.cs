using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OurBankApi.Models;

public enum CurrencyDenomination
{
    Dollar,
    Naira
}

public class BankAccount
{
    // public Guid Id { get; set; }
    [Key]
    public string AccountNumber { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    // public string Name { get; set; }
    public decimal Balance { get; set; }
    public CurrencyDenomination Currency { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public uint Version { get; set; }


    public ICollection<Transaction> SentTransactions { get; set; } = new HashSet<Transaction>();
    public ICollection<Transaction> ReceivedTransactions { get; set; } = new HashSet<Transaction>();

    [NotMapped]
    public IEnumerable<Transaction> Transactions =>
        SentTransactions.Concat(ReceivedTransactions)
        .Distinct()
        .OrderByDescending(t => t.TimeStamp);
}