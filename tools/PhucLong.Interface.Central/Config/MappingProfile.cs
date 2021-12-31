using AutoMapper;
using PhucLong.Interface.Central.Models.Inbound;
using PhucLong.Interface.Central.Models.Staging;

namespace PhucLong.Interface.Central.Config
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<INB_SALE_MASTER, E1BPTRANSACTION>();
            CreateMap<INB_SALE_DETAIL, E1BPRETAILLINEITEM>();
            CreateMap<INB_SALE_TENDER, E1BPTENDER>();
            CreateMap<INB_SALE_DISCOUNT, E1BPLINEITEMDISCOUNT>();
        }
    }
}
