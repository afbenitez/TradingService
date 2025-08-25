using AutoMapper;
using TradingService.Models;
using TradingService.Models.DTOs;

namespace TradingService.Mappings
{
    /// <summary>
    /// AutoMapper profile for mapping between domain models and DTOs
    /// </summary>
    public class TradeMappingProfile : Profile
    {
        public TradeMappingProfile()
        {
            // Map from CreateTradeDto to Trade
            CreateMap<CreateTradeDto, Trade>()
                .ForMember(dest => dest.ExecutedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => TradeStatus.Pending))
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // Map from Trade to TradeDto
            CreateMap<Trade, TradeDto>()
                .ForMember(dest => dest.TradeType, opt => opt.MapFrom(src => src.TradeType.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}