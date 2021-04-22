using AutoMapper;
using PaymentGateway.API.Commands;
using PaymentGateway.API.Models;
using PaymentGateway.API.Request;
using PaymentGateway.API.Responses;

namespace PaymentGateway.API
{
    public class AutoMapperProfile : Profile 
    {        
        public AutoMapperProfile()
        {
            CreateMap<PaymentCreated, PaymentResponse>();
            CreateMap<PaymentRequest, CreatePaymentCommand>();
            CreateMap<PaymentCreated, AcquiringBankRequest>();            
        }        
    }
}
