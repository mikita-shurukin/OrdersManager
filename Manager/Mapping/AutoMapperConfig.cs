using AutoMapper;
using Manager.DAL.Models;
using Manager.Models.Requests;

namespace Manager.Mapping
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<UpdateCategory, Category>();
            CreateMap<CreateCategory, Category>();

            CreateMap<UpdateItem, Item>();
            CreateMap<Item, UpdateItem>();
            CreateMap<CreateItem, Item>();

            CreateMap<UpdateOrder, Order>();
            CreateMap<CreateOrder, Order>();

        }
    }
}
