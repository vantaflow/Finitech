using Finitech.Modules.Banking.Contracts;
using Finitech.Modules.Banking.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Finitech.ApiHost.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BankingController : ControllerBase
{
    private readonly IBankingService _service;

    public BankingController(IBankingService service)
    {
        _service = service;
    }

    [HttpPost("accounts")]
    public async Task<ActionResult<BankAccountDto>> CreateAccount(CreateBankAccountRequest request)
    {
        var account = await _service.CreateAccountAsync(request);
        return CreatedAtAction(nameof(GetAccount), new { accountId = account.Id }, account);
    }

    [HttpGet("accounts/{accountId:guid}")]
    public async Task<ActionResult<BankAccountDto>> GetAccount(Guid accountId)
    {
        var account = await _service.GetAccountAsync(accountId);
        return account == null ? NotFound() : Ok(account);
    }

    [HttpGet("by-party/{partyId:guid}/accounts")]
    public async Task<ActionResult<IReadOnlyList<BankAccountDto>>> GetAccountsByParty(Guid partyId)
    {
        var accounts = await _service.GetAccountsByPartyIdAsync(partyId);
        return Ok(accounts);
    }

    [HttpPost("savings")]
    public async Task<ActionResult<SavingsAccountDto>> CreateSavingsAccount(CreateSavingsAccountRequest request)
    {
        var account = await _service.CreateSavingsAccountAsync(request);
        return Ok(account);
    }

    [HttpPost("accounts/{accountId:guid}/deposit")]
    public async Task<IActionResult> Deposit(Guid accountId, [FromBody] long amountMinorUnits, [FromQuery] string? description = null)
    {
        await _service.DepositAsync(new DepositRequest { AccountId = accountId, AmountMinorUnits = amountMinorUnits, Description = description });
        return NoContent();
    }

    [HttpPost("accounts/{accountId:guid}/withdraw")]
    public async Task<IActionResult> Withdraw(Guid accountId, [FromBody] long amountMinorUnits, [FromQuery] string? description = null)
    {
        await _service.WithdrawAsync(new WithdrawRequest { AccountId = accountId, AmountMinorUnits = amountMinorUnits, Description = description });
        return NoContent();
    }

    [HttpPost("accounts/{accountId:guid}/calculate-interest")]
    public async Task<IActionResult> CalculateInterest(Guid accountId)
    {
        await _service.CalculateInterestAsync(accountId);
        return NoContent();
    }

    [HttpPost("fixed-deposits")]
    public async Task<ActionResult<FixedDepositDto>> CreateFixedDeposit(CreateFixedDepositRequest request)
    {
        var fd = await _service.CreateFixedDepositAsync(request);
        return Ok(fd);
    }

    [HttpGet("fixed-deposits/{fixedDepositId:guid}")]
    public async Task<ActionResult<FixedDepositDto>> GetFixedDeposit(Guid fixedDepositId)
    {
        var fd = await _service.GetFixedDepositAsync(fixedDepositId);
        return fd == null ? NotFound() : Ok(fd);
    }

    [HttpPost("loans")]
    public async Task<ActionResult<LoanDto>> RequestLoan(LoanApplicationRequest request)
    {
        var loan = await _service.RequestLoanAsync(request);
        return Ok(loan);
    }

    [HttpPost("loans/{loanId:guid}/approve")]
    public async Task<ActionResult<LoanDto>> ApproveLoan(Guid loanId, LoanApprovalRequest request)
    {
        var loan = await _service.ApproveLoanAsync(request with { LoanId = loanId });
        return Ok(loan);
    }

    [HttpPost("loans/{loanId:guid}/repay")]
    public async Task<IActionResult> RepayLoan(Guid loanId, [FromBody] long amountMinorUnits, [FromQuery] Guid fromAccountId)
    {
        await _service.RepayLoanAsync(new LoanRepaymentRequest { LoanId = loanId, FromAccountId = fromAccountId, AmountMinorUnits = amountMinorUnits });
        return NoContent();
    }

    [HttpGet("loans/{loanId:guid}")]
    public async Task<ActionResult<LoanDto>> GetLoan(Guid loanId)
    {
        var loan = await _service.GetLoanAsync(loanId);
        return loan == null ? NotFound() : Ok(loan);
    }

    [HttpPost("accounts/{accountId:guid}/overdraft")]
    public async Task<ActionResult<OverdraftDto>> SetOverdraft(Guid accountId, [FromBody] long limitMinorUnits, [FromQuery] decimal interestRate = 0.05m)
    {
        var overdraft = await _service.SetOverdraftAsync(new SetOverdraftRequest { AccountId = accountId, LimitMinorUnits = limitMinorUnits, InterestRate = interestRate });
        return Ok(overdraft);
    }

    [HttpPost("accounts/{accountId:guid}/cheque-books")]
    public async Task<ActionResult<ChequeBookDto>> OrderChequeBook(Guid accountId, [FromQuery] int numberOfCheques = 25)
    {
        var book = await _service.OrderChequeBookAsync(new OrderChequeBookRequest { AccountId = accountId, NumberOfCheques = numberOfCheques });
        return Ok(book);
    }

    [HttpPost("accounts/{accountId:guid}/cheque-deposit")]
    public async Task<ActionResult<ChequeDepositDto>> DepositCheque(Guid accountId, DepositChequeRequest request)
    {
        var deposit = await _service.DepositChequeAsync(request with { ToAccountId = accountId });
        return Ok(deposit);
    }

    [HttpPost("cards/{cardId:guid}/freeze")]
    public async Task<IActionResult> FreezeCard(Guid cardId)
    {
        await _service.FreezeCardAsync(cardId);
        return NoContent();
    }

    [HttpPost("cards/{cardId:guid}/unfreeze")]
    public async Task<IActionResult> UnfreezeCard(Guid cardId)
    {
        await _service.UnfreezeCardAsync(cardId);
        return NoContent();
    }
}
