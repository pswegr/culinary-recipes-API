using CulinaryRecipes.API.Extensions.Claims;
using CulinaryRecipes.API.Models.Finance;
using CulinaryRecipes.API.Services.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CulinaryRecipes.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FinanceController : ControllerBase
{
    private readonly IFinanceService _finance;

    public FinanceController(IFinanceService finance)
    {
        _finance = finance;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<FinanceDashboard>> GetDashboard([FromQuery] int year, [FromQuery] int month) =>
        Ok(await _finance.GetDashboardAsync(GetUserId(), year, month));

    [HttpGet("calendar/{year:int}")]
    public async Task<ActionResult<IReadOnlyList<MonthSummary>>> GetYearSummary(int year) =>
        Ok(await _finance.GetYearSummaryAsync(GetUserId(), year));

    [HttpGet("expenses/{id:length(24)}")]
    public async Task<ActionResult<Expense>> GetExpense(string id)
    {
        var expense = await _finance.GetExpenseAsync(GetUserId(), id);
        return expense is null ? NotFound() : Ok(expense);
    }

    [HttpPost("expenses")]
    public async Task<ActionResult<Expense>> CreateExpense(TransactionRequest request)
    {
        try
        {
            var expense = await _finance.CreateExpenseAsync(GetUserId(), request);
            return CreatedAtAction(nameof(GetExpense), new { id = expense.id }, expense);
        }
        catch (KeyNotFoundException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPut("expenses/{id:length(24)}")]
    public async Task<ActionResult<Expense>> UpdateExpense(string id, TransactionRequest request)
    {
        try
        {
            var expense = await _finance.UpdateExpenseAsync(GetUserId(), id, request);
            return expense is null ? NotFound() : Ok(expense);
        }
        catch (KeyNotFoundException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpDelete("expenses/{id:length(24)}")]
    public async Task<IActionResult> DeleteExpense(string id) =>
        await _finance.DeleteExpenseAsync(GetUserId(), id) ? NoContent() : NotFound();

    [HttpGet("incomes/{id:length(24)}")]
    public async Task<ActionResult<Income>> GetIncome(string id)
    {
        var income = await _finance.GetIncomeAsync(GetUserId(), id);
        return income is null ? NotFound() : Ok(income);
    }

    [HttpPost("incomes")]
    public async Task<ActionResult<Income>> CreateIncome(TransactionRequest request)
    {
        try
        {
            var income = await _finance.CreateIncomeAsync(GetUserId(), request);
            return CreatedAtAction(nameof(GetIncome), new { id = income.id }, income);
        }
        catch (KeyNotFoundException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPut("incomes/{id:length(24)}")]
    public async Task<ActionResult<Income>> UpdateIncome(string id, TransactionRequest request)
    {
        try
        {
            var income = await _finance.UpdateIncomeAsync(GetUserId(), id, request);
            return income is null ? NotFound() : Ok(income);
        }
        catch (KeyNotFoundException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpDelete("incomes/{id:length(24)}")]
    public async Task<IActionResult> DeleteIncome(string id) =>
        await _finance.DeleteIncomeAsync(GetUserId(), id) ? NoContent() : NotFound();

    [HttpPost("expense-types")]
    public async Task<ActionResult<ExpenseType>> CreateExpenseType(FinanceTypeRequest request) =>
        Ok(await _finance.CreateExpenseTypeAsync(GetUserId(), request));

    [HttpPost("income-types")]
    public async Task<ActionResult<IncomeType>> CreateIncomeType(FinanceTypeRequest request) =>
        Ok(await _finance.CreateIncomeTypeAsync(GetUserId(), request));

    [HttpPut("plans/{year:int}/{month:int}")]
    public async Task<ActionResult<MonthlyPlan>> UpsertMonthlyPlan(int year, int month, MonthlyPlanRequest request) =>
        Ok(await _finance.UpsertMonthlyPlanAsync(GetUserId(), year, month, request));

    private string GetUserId() =>
        User.GetUserId() ?? throw new UnauthorizedAccessException("Authenticated user identifier is missing.");
}
