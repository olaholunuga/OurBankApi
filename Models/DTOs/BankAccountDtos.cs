using System.ComponentModel.DataAnnotations;

namespace OurBankApi.Models.DTOs;

public class DepositRequestDto
{
    [Required]
    public decimal amount { get; set; }
}

public class TransferRequestDto
{
    [Required]
    public string destId { get; set; }
    [Required]
    public decimal amount { get; set; }
}

public class BankAccountDto
{
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; }

    public List<TransactionDto> Transactions { get; set; } = new();
    // public List<TransactionDto> ReceivedTransactions { get; set; } = new();
}

public class AccountDetailsDto
{
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; }
}

public class TransactionDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
    public string SourceAccountId { get; set; }
    public string DestinationAccountId { get; set; }
    public string Type { get; set; }
}