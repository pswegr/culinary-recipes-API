using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CulinaryRecipes.API.Models.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CulinaryRecipes.API.Models.Finance;

public abstract class UserFinanceEntity : IEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? id { get; set; }

    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
}

public class Expense : UserFinanceEntity
{
    public string ExpenseTypeId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Amount { get; set; }

    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ExpenseType : UserFinanceEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class Income : UserFinanceEntity
{
    public string IncomeTypeId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Amount { get; set; }

    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class IncomeType : UserFinanceEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class MonthlyPlan : UserFinanceEntity
{
    public int Year { get; set; }
    public int Month { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal ExpenseLimit { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal SavingsGoal { get; set; }
}

public class TransactionRequest
{
    [Required, StringLength(24, MinimumLength = 24)]
    public string TypeId { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 999999999999)]
    public decimal Amount { get; set; }

    public DateTime Date { get; set; }
}

public class FinanceTypeRequest
{
    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [StringLength(300)]
    public string Description { get; set; } = string.Empty;
}

public class MonthlyPlanRequest
{
    [Range(0, 999999999999)]
    public decimal ExpenseLimit { get; set; }

    [Range(0, 999999999999)]
    public decimal SavingsGoal { get; set; }
}

public record MonthSummary(
    int Year,
    int Month,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal Savings,
    decimal ExpenseLimit,
    decimal SavingsGoal,
    decimal RemainingExpenseLimit,
    decimal ExpenseLimitProgress,
    decimal SavingsGoalProgress);

public record FinanceDashboard(
    MonthSummary Summary,
    IReadOnlyList<Expense> Expenses,
    IReadOnlyList<Income> Incomes,
    IReadOnlyList<ExpenseType> ExpenseTypes,
    IReadOnlyList<IncomeType> IncomeTypes,
    MonthlyPlan? Plan);
