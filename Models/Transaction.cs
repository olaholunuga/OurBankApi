namespace OurBankApi.Models;

public enum TransactionType
{
    Deposit,
    Withdrawal,
    Transfer
}

public class Transaction
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    public TransactionType Type { get; set; }

    public string SourceAccountId { get; set; }
    public BankAccount SourceAccount { get; set; }

    public string DestinationAccountId { get; set; }
    public BankAccount DestinationAccount { get; set; }
}