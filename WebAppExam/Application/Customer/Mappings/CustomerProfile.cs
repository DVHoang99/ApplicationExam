using AutoMapper;
using WebAppExam.Domain.ViewModels;

namespace WebAppExam.Application.Customer.Mappings
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<Domain.Customer, CustomerViewModel>();
        }
    }
}
