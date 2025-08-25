using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TradingService.Models.DTOs;
using TradingService.Services.Interfaces;

namespace TradingService.Controllers
{
    /// <summary>
    /// Controller for managing trade operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TradesController : ControllerBase
    {
        private readonly ITradeService _tradeService;
        private readonly IMapper _mapper;
        private readonly ILogger<TradesController> _logger;
        private readonly IValidator<CreateTradeDto> _createTradeValidator;

        public TradesController(
            ITradeService tradeService,
            IMapper mapper,
            ILogger<TradesController> logger,
            IValidator<CreateTradeDto> createTradeValidator)
        {
            _tradeService = tradeService;
            _mapper = mapper;
            _logger = logger;
            _createTradeValidator = createTradeValidator;
        }

        /// <summary>
        /// Executes a new trade
        /// </summary>
        /// <param name="createTradeDto">Trade details</param>
        /// <returns>The executed trade</returns>
        /// <response code="201">Trade executed successfully</response>
        /// <response code="400">Invalid trade data</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(TradeDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TradeDto>> ExecuteTrade([FromBody] CreateTradeDto createTradeDto)
        {
            _logger.LogInformation("Received trade execution request for user {UserId}", createTradeDto.UserId);

            try
            {
                // Validate the request
                var validationResult = await _createTradeValidator.ValidateAsync(createTradeDto);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Trade validation failed for user {UserId}: {Errors}", 
                        createTradeDto.UserId, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                    
                    return BadRequest(validationResult.Errors.Select(e => new { 
                        Property = e.PropertyName, 
                        Error = e.ErrorMessage 
                    }));
                }

                // Execute the trade
                var trade = await _tradeService.ExecuteTradeAsync(createTradeDto);
                var tradeDto = _mapper.Map<TradeDto>(trade);

                _logger.LogInformation("Trade executed successfully. TradeId: {TradeId}", trade.Id);

                return CreatedAtAction(nameof(GetTrade), new { id = trade.Id }, tradeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing trade for user {UserId}", createTradeDto.UserId);
                return StatusCode(500, new { message = "An error occurred while executing the trade" });
            }
        }

        /// <summary>
        /// Retrieves trades based on query parameters
        /// </summary>
        /// <param name="query">Query parameters for filtering trades</param>
        /// <returns>List of trades with pagination information</returns>
        /// <response code="200">Trades retrieved successfully</response>
        /// <response code="400">Invalid query parameters</response>
        [HttpGet]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetTrades([FromQuery] TradeQueryDto query)
        {
            _logger.LogInformation("Retrieving trades with pagination. Page: {Page}, PageSize: {PageSize}", 
                query.Page, query.PageSize);

            try
            {
                var (trades, totalCount) = await _tradeService.GetTradesAsync(query);
                var tradeDtos = _mapper.Map<IEnumerable<TradeDto>>(trades);

                var response = new
                {
                    data = tradeDtos,
                    pagination = new
                    {
                        currentPage = query.Page,
                        pageSize = query.PageSize,
                        totalCount = totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trades");
                return StatusCode(500, new { message = "An error occurred while retrieving trades" });
            }
        }

        /// <summary>
        /// Retrieves a specific trade by ID
        /// </summary>
        /// <param name="id">Trade ID</param>
        /// <returns>The trade details</returns>
        /// <response code="200">Trade found</response>
        /// <response code="404">Trade not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TradeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TradeDto>> GetTrade(int id)
        {
            _logger.LogInformation("Retrieving trade with ID: {TradeId}", id);

            try
            {
                var trade = await _tradeService.GetTradeByIdAsync(id);
                
                if (trade == null)
                {
                    _logger.LogWarning("Trade with ID {TradeId} not found", id);
                    return NotFound(new { message = $"Trade with ID {id} not found" });
                }

                var tradeDto = _mapper.Map<TradeDto>(trade);
                return Ok(tradeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trade with ID {TradeId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the trade" });
            }
        }

        /// <summary>
        /// Gets trade statistics for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Trade statistics</returns>
        /// <response code="200">Statistics retrieved successfully</response>
        /// <response code="400">Invalid user ID</response>
        [HttpGet("statistics/{userId}")]
        [ProducesResponseType(typeof(TradeStatistics), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TradeStatistics>> GetTradeStatistics(string userId)
        {
            _logger.LogInformation("Retrieving trade statistics for user: {UserId}", userId);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new { message = "User ID is required" });
            }

            try
            {
                var statistics = await _tradeService.GetTradeStatisticsAsync(userId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving statistics for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving trade statistics" });
            }
        }
    }
}