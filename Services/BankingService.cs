using Microsoft.EntityFrameworkCore;
using OurBankApi.Data;
using OurBankApi.Models;

public class BankingService(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<bool> TransferFundsAsync(string sourceId, string destId, decimal amount)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        

        try
        {

            //get the source and destination account
            var source = await _context.BankAccounts.FirstOrDefaultAsync(a => a.AccountNumber == sourceId);
            var dest = await _context.BankAccounts.FirstOrDefaultAsync(a => a.AccountNumber == destId);

            if (source == null)
            {
                Console.WriteLine($"source account null");
                return false;
            }
            if (dest == null)
            {
                Console.WriteLine($"destination account: {destId}");
                return false;
            }
            Console.WriteLine($"i reached here");
            if (amount <= 0 || source.Balance < amount) return false;

            source.Balance -= amount;
            dest.Balance += amount;


            var transactionReceipt = new Transaction
            {
                Id = Guid.NewGuid(),
                SourceAccountId = sourceId,
                DestinationAccountId = destId,
                Amount = amount,
                Type = TransactionType.Transfer
            };

            await _context.Transactions.AddAsync(transactionReceipt);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch(DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();

            return false;
        }
        catch(Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}