using FluentValidation;
using TradingService.Models.DTOs;

namespace TradingService.Validators
{
    /// <summary>
    /// Validator for CreateTradeDto
    /// </summary>
    public class CreateTradeDtoValidator : AbstractValidator<CreateTradeDto>
    {
        public CreateTradeDtoValidator()
        {
            RuleFor(x => x.Symbol)
                .NotEmpty().WithMessage("Symbol is required")
                .Length(1, 10).WithMessage("Symbol must be between 1 and 10 characters")
                .Matches("^[A-Z]+$").WithMessage("Symbol must contain only uppercase letters");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0")
                .LessThanOrEqualTo(1000000).WithMessage("Quantity cannot exceed 1,000,000 shares");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0")
                .LessThanOrEqualTo(100000).WithMessage("Price cannot exceed $100,000 per share")
                .ScalePrecision(2, 10).WithMessage("Price can have maximum 2 decimal places");

            RuleFor(x => x.TradeType)
                .IsInEnum().WithMessage("TradeType must be either Buy (1) or Sell (2)");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required")
                .Length(1, 50).WithMessage("UserId must be between 1 and 50 characters")
                .Matches("^[a-zA-Z0-9_-]+$").WithMessage("UserId can only contain letters, numbers, underscores, and hyphens");

            // Business rule validation (I assumed that rule only for test)
            RuleFor(x => x)
                .Must(BeValidTradeValue).WithMessage("Total trade value cannot exceed $1,000,000");
        }

        private bool BeValidTradeValue(CreateTradeDto dto)
        {
            var totalValue = dto.Quantity * dto.Price;
            return totalValue <= 1000000;
        }
    }
}