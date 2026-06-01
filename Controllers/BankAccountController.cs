using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OurBankApi.Data;
using OurBankApi.Models;
using Microsoft.EntityFrameworkCore;
using OurBankApi.Models.DTOs;

namespace OurBankApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BankAccountsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private BankingService _bankingService;
    public BankAccountsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, BankingService bankingService)
    {
        _context = context;
        _userManager = userManager;
        _bankingService = bankingService;
    }
    // public async Task<IActionResult> Index()
    // {
    //     var userId = _userManager.GetUserId(User);

    //     var account = await _context.BankAccounts
    //         .Include(a => a.SentTransactions)
    //         .Include(a => a.ReceivedTransactions)
    //         .AsSplitQuery()
    //         .FirstOrDefaultAsync(a => a.UserId == userId);

    //     return View(account);
    // }
    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> Create()
    {
        var userId = _userManager.GetUserId(User);

        if (await _context.BankAccounts.AnyAsync(a => a.UserId == userId))
            return BadRequest(new { Message = "User Bank account already created." });

        string accountId;
        do
        {
            accountId = new Random().Next(1000000000, 2000000000).ToString();
        } while (_context.BankAccounts.Any(a => a.AccountNumber == accountId));
        var account = new BankAccount
        {
            AccountNumber = accountId,
            UserId = userId,
            Balance = 500m,
            Currency = CurrencyDenomination.Dollar
        };

        await _context.BankAccounts.AddAsync(account);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Account created successfully",
            AccountDetails = new
            {
                account.AccountNumber,
                account.Balance,
                Currency = account.Currency.ToString()
            }
        });
    }

    [Authorize]
    [HttpPost("transactions")]
    public async Task<IActionResult> Transactions()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { Message = "User Identity not found" });
        }
        var account = await _context.BankAccounts
            .Include(a => a.SentTransactions)
            .Include(a => a.ReceivedTransactions)
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.UserId == userId);

        if (account == null) return NotFound("Account not found");

        var accountDto = new BankAccountDto
        {
            AccountNumber = account.AccountNumber,
            Balance = account.Balance,
            Currency = account.Currency.ToString(),
            Transactions = account.Transactions.Select(t => new TransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                Timestamp = t.TimeStamp,
                SourceAccountId = t.SourceAccountId,
                DestinationAccountId = t.DestinationAccountId,
                Type = t.Type.ToString()
            }).ToList()

            // ReceivedTransactions = account.ReceivedTransactions.Select(t => new TransactionDto
            // {
            //     Id = t.Id,
            //     Amount = t.Amount,
            //     Timestamp = t.TimeStamp,
            //     SourceAccountId = t.SourceAccountId,
            //     DestinationAccountId = t.DestinationAccountId
            // }).ToList(),
            // /
            // .Concat(account.ReceivedTransactions.Select(t => new TransactionDto
            // {
            //     Id = t.Id,
            //     Amount = t.Amount,
            //     Timestamp = t.TimeStamp,
            //     SourceAccountId = t.SourceAccountId,
            //     DestinationAccountId = t.DestinationAccountId,
            //     Type = t.Type
            // }))
        };

        return Ok(accountDto);
    }

    [Authorize]
    [HttpPost("account_details")]
    public async Task<IActionResult> AccountDetails()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { Message = "User Identity not found" });
        }
        var account = await _context.BankAccounts
            .FirstOrDefaultAsync(a => a.UserId == userId);

        if (account == null) return NotFound("Account not found");

        var accountDetailsDto = new AccountDetailsDto
        {
            AccountNumber = account.AccountNumber,
            Balance = account.Balance,
            Currency = account.Currency.ToString()
        };
        return Ok(accountDetailsDto);
    }


    [Authorize]
    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] DepositRequestDto model)
    {
        var userId = _userManager.GetUserId(User);
        var account = await _context.BankAccounts.FirstOrDefaultAsync(a => a.UserId == userId);

        if (account == null || model.amount < 0) return BadRequest("Account does not exist or amount less that zero.");

        account.Balance += model.amount;
        var tran = new Transaction
        {
            Id = Guid.NewGuid(),
            DestinationAccountId = account.AccountNumber,
            SourceAccountId = account.AccountNumber,
            Amount = model.amount,
            Type = TransactionType.Deposit
        };
        await _context.Transactions.AddAsync(tran);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { Error = "Your account state was modified during process. Please try again." });
        }

        return Ok(
            new
            {
                Message = $"{tran.Amount} deposited successfully",
                Transaction = new
                {
                    tran.Id,
                    tran.DestinationAccountId,
                    tran.SourceAccountId,
                    tran.Amount,
                    Type = tran.Type.ToString()
                }
            }
        );
    }

    [Authorize]
    [HttpPost("Transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequestDto model)
    {
        var userId = _userManager.GetUserId(User);
        var account = await _context.BankAccounts.FirstOrDefaultAsync(a => a.UserId == userId);

        if (account == null) return NotFound("Account not found");

        bool success = await _bankingService.TransferFundsAsync(account.AccountNumber, model.destId, model.amount);

        if (success)
            return Ok(new
            {
                Message = "Transfer successful!"
            });
        else
            return BadRequest(new
            {
                Message = "Transfer failed. Check your balance of network."
            });
    }

    [Authorize]
    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] DepositRequestDto model)
    {
        var userId = _userManager.GetUserId(User);
        var account = await _context.BankAccounts.FirstOrDefaultAsync(a => a.UserId == userId);

        if (account == null || model.amount < 0)
        {
            // TempData["ErrorMessage"] = $"The specified account does not exist or the amount is less than zero";
            return BadRequest(new
            {
                Message = "The specified account does not exist or the amount is less than zero"
            });
        }

        if (account.Balance < model.amount)
        {
            // TempData["ErrorMessage"] = $"amount specified is more than account balance";
            return BadRequest(new
            {
                Message = "The amount specified is more than account balance"
            });
        }
        else
        {
            account.Balance -= model.amount;
        }
        var tran = new Transaction
        {
            Id = Guid.NewGuid(),
            DestinationAccountId = account.AccountNumber,
            SourceAccountId = account.AccountNumber,
            Amount = model.amount,
            Type = TransactionType.Withdrawal
        };
        await _context.Transactions.AddAsync(
            tran
        );

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { Error = "Your account state was modified during process. Please try again." });
        }

        return Ok(
            new
            {
                Message = $"{tran.Amount} withdrawn successfully",
                Transaction = new
                {
                    tran.Id,
                    tran.DestinationAccountId,
                    tran.SourceAccountId,
                    tran.Amount,
                    Type = tran.Type.ToString()
                }
            }
        );


    }
}