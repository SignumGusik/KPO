using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentsService.Contracts;
using PaymentsService.Domain;
using PaymentsService.Persistence;

namespace PaymentsService.Controllers;

[ApiController]
[Route("accounts")]
public sealed class AccountsController : ControllerBase
{
    private readonly PaymentsDbContext _db;

    public AccountsController(PaymentsDbContext db) => _db = db;

    // POST /accounts
    // вход
    // если нет — создать balance=0, если есть — вернуть существующий
    [HttpPost]
    public async Task<ActionResult<AccountResponse>> CreateOrGet([FromBody] CreateAccountRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.UserId))
            return BadRequest(new { error = "userId is required" });

        var existing = await _db.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.UserId == req.UserId, ct);
        if (existing is not null)
            return Ok(new AccountResponse(existing.UserId, existing.Balance));

        var acc = new Account
        {
            UserId = req.UserId,
            Balance = 0m,
            Version = 0
        };

        _db.Accounts.Add(acc);

        try
        {
            await _db.SaveChangesAsync(ct);
            return Ok(new AccountResponse(acc.UserId, acc.Balance));
        }
        catch (DbUpdateException)
        {
            // Если два запроса пришли одновременно — один создаст, второй получит конфликт PK.
            // Тогда возвращаем уже созданный
            var created = await _db.Accounts.AsNoTracking().FirstAsync(a => a.UserId == req.UserId, ct);
            return Ok(new AccountResponse(created.UserId, created.Balance));
        }
    }

    // GET /accounts/{userId}
    [HttpGet("{userId}")]
    public async Task<ActionResult<AccountResponse>> Get(string userId, CancellationToken ct)
    {
        var acc = await _db.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.UserId == userId, ct);
        if (acc is null) return NotFound();

        return Ok(new AccountResponse(acc.UserId, acc.Balance));
    }

    // 6.3 POST /accounts/{userId}/topup
    // вход
    // в транзакции:
    // - ledger (TOPUP)
    // - balance += amount
    // - version++
    // проверки:
    // - amount > 0
    // - если user не найден — 404
    [HttpPost("{userId}/topup")]
    public async Task<ActionResult<AccountResponse>> TopUp(string userId, [FromBody] TopUpRequest req, CancellationToken ct)
    {
        if (req.Amount <= 0)
            return BadRequest(new { error = "amount must be > 0" });

        // Транзакция: ledger + баланс + версия
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        //берём tracked entity, чтобы обновление прошло корректно
        var acc = await _db.Accounts.FirstOrDefaultAsync(a => a.UserId == userId, ct);
        if (acc is null) return NotFound();

        _db.Ledger.Add(new LedgerEntry
        {
            TxId = Guid.NewGuid(),
            OrderId = null,
            UserId = userId,
            Type = LedgerType.TOPUP,
            Amount = req.Amount,
            Status = LedgerStatus.SUCCESS,
            CreatedAt = DateTimeOffset.UtcNow
        });

        acc.Balance += req.Amount;
        acc.Version += 1;

        try
        {
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            await tx.RollbackAsync(ct);
            return Conflict(new { error = "concurrent update, retry" });
        }

        return Ok(new AccountResponse(acc.UserId, acc.Balance));
    }
}