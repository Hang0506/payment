using AutoMapper;
using FRTTMO.PaymentCore.Options;
using FRTTMO.PaymentCore.Dto.TransactionDetailDto;
using FRTTMO.PaymentCore.Kafka.Eto.v2;
using FRTTMO.PaymentCore.Dto.v2;

namespace FRTTMO.PaymentCore.AutoMapper
{
    public class ElasticSearchMapperProfile : Profile
    {
        public ElasticSearchMapperProfile()
        {
            //CreateMap<DepositCoresAllOutputFullDtoV2, TransactionIndex>()
            // .ForMember(dts => dts.id, opts => opts.MapFrom(src => src.PaymentRequestCode));
            CreateMap<TransactionDetailTransferOutputDto, TransactionIndex>()
            .ForMember(dts => dts.id, opts => opts.MapFrom(src => src.PaymentCode));

            CreateMap<Dto.v2.DepositAllDto, DepositAllIndex>()
            .ForMember(dts => dts.id, opts => opts.MapFrom(src => src.PaymentCode));

            CreateMap<Dto.v2.DepositAllDto, DepositAllEto>();
            CreateMap<HeaderFinalIndex, DepositAllDto>();
            CreateMap<PaymentTransIndex, FRTTMO.PaymentCore.Dto.v2.Detail>();
        }
    }
}
