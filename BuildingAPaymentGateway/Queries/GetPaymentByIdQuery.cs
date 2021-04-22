using MediatR;
using PaymentGateway.API.Responses;
using System;

namespace PaymentGateway.API.Queries
{
    public class GetPaymentByIdQuery : IRequest<PaymentResponse>
    {
        public Guid Id { get; }
        public GetPaymentByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
