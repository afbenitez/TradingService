using FluentValidation.TestHelper;
using TradingService.Models.DTOs;
using TradingService.Models;
using TradingService.Validators;
using Xunit;
using FluentAssertions;

namespace TradingService.Tests.Validators
{
    /// <summary>
    /// Unit tests for CreateTradeDtoValidator
    /// </summary>
    public class CreateTradeDtoValidatorTests
    {
        private readonly CreateTradeDtoValidator _validator;

        public CreateTradeDtoValidatorTests()
        {
            _validator = new CreateTradeDtoValidator();
        }

        #region Symbol Validation Tests

        [Fact]
        public void Symbol_WhenValid_ShouldNotHaveValidationError()
        {
            // Arrange
            var model = CreateValidTradeDto();

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Symbol);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Symbol_WhenEmpty_ShouldHaveValidationError(string invalidSymbol)
        {
            // Arrange
            var model = CreateValidTradeDto();
            model.Symbol = invalidSymbol;

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Symbol);
        }

        [Fact]
        public void Symbol_WhenTooLong_ShouldHaveValidationError()
        {
            // Arrange
            var model = CreateValidTradeDto();
            model.Symbol = "VERYLONGSYMBOL"; // More than 10 characters

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Symbol);
        }

        #endregion

        #region Quantity Validation Tests

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(1000000)]
        public void Quantity_WhenPositive_ShouldNotHaveValidationError(int validQuantity)
        {
            // Arrange
            var model = CreateValidTradeDto();
            model.Quantity = validQuantity;

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Quantity);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Quantity_WhenZeroOrNegative_ShouldHaveValidationError(int invalidQuantity)
        {
            // Arrange
            var model = CreateValidTradeDto();
            model.Quantity = invalidQuantity;

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Quantity);
        }

        #endregion

        #region Price Validation Tests

        [Theory]
        [InlineData(0.01)]
        [InlineData(100.50)]
        [InlineData(50000.99)] // Adjusted to be within $100,000 limit
        public void Price_WhenPositive_ShouldNotHaveValidationError(decimal validPrice)
        {
            // Arrange
            var model = CreateValidTradeDto();
            model.Price = validPrice;

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Price);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-0.01)]
        [InlineData(-100)]
        public void Price_WhenZeroOrNegative_ShouldHaveValidationError(decimal invalidPrice)
        {
            // Arrange
            var model = CreateValidTradeDto();
            model.Price = invalidPrice;

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Price);
        }

        [Fact]
        public void Price_WhenExceedsLimit_ShouldHaveValidationError()
        {
            // Arrange
            var model = CreateValidTradeDto();
            model.Price = 100001m; // Exceeds $100,000 limit

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Price);
        }

        #endregion

        #region TradeType Validation Tests

        [Theory]
        [InlineData(TradeType.Buy)]
        [InlineData(TradeType.Sell)]
        public void TradeType_WhenValid_ShouldNotHaveValidationError(TradeType validTradeType)
        {
            // Arrange
            var model = CreateValidTradeDto();
            model.TradeType = validTradeType;

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.TradeType);
        }

        [Fact]
        public void TradeType_WhenInvalid_ShouldHaveValidationError()
        {
            // Arrange
            var model = CreateValidTradeDto();
            model.TradeType = (TradeType)99; // Invalid enum value

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TradeType);
        }

        #endregion

        #region UserId Validation Tests

        [Fact]
        public void UserId_WhenValid_ShouldNotHaveValidationError()
        {
            // Arrange
            var model = CreateValidTradeDto();

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void UserId_WhenEmpty_ShouldHaveValidationError(string invalidUserId)
        {
            // Arrange
            var model = CreateValidTradeDto();
            model.UserId = invalidUserId;

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [Fact]
        public void UserId_WhenTooLong_ShouldHaveValidationError()
        {
            // Arrange
            var model = CreateValidTradeDto();
            model.UserId = new string('a', 51); // More than 50 characters

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }

        #endregion

        #region Complete Validation Tests

        [Fact]
        public void ValidTradeDto_ShouldPassAllValidations()
        {
            // Arrange
            var model = CreateValidTradeDto();

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void InvalidTradeDto_ShouldFailMultipleValidations()
        {
            // Arrange
            var model = new CreateTradeDto
            {
                Symbol = "", // Invalid
                Quantity = -1, // Invalid
                Price = 0, // Invalid
                TradeType = (TradeType)99, // Invalid
                UserId = "" // Invalid
            };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Symbol);
            result.ShouldHaveValidationErrorFor(x => x.Quantity);
            result.ShouldHaveValidationErrorFor(x => x.Price);
            result.ShouldHaveValidationErrorFor(x => x.TradeType);
            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }

        #endregion

        #region Helper Methods

        private static CreateTradeDto CreateValidTradeDto()
        {
            return new CreateTradeDto
            {
                Symbol = "AAPL",
                Quantity = 100,
                Price = 150.50m,
                TradeType = TradeType.Buy,
                UserId = "testuser"
            };
        }

        #endregion
    }
}