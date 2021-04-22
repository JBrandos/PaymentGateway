using MediatR;
using PaymentGateway.API.Queries;
using PaymentGateway.API.Repositories;
using PaymentGateway.API.Responses;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentGateway.API.Handlers
{
    public class GetPaymentByIdHandler : IRequestHandler<GetPaymentByIdQuery, PaymentResponse>
    {
        private readonly IPaymentsQueryRepository _paymentsQueryRepository;

        public GetPaymentByIdHandler(IPaymentsQueryRepository paymentsQueryRepository)
        {
            _paymentsQueryRepository = paymentsQueryRepository;
        }
       
        public async Task<PaymentResponse> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
        {
            var paymentEvents = await _paymentsQueryRepository.GetPaymentEventsByIdAsync(request.Id);
            return paymentEvents.Count == 0 ? null : new PaymentResponse(paymentEvents);
        }
    }
}
