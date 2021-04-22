using AutoMapper;
using MediatR;
using Newtonsoft.Json;
using PaymentGateway.API.Commands;
using PaymentGateway.API.Models;
using PaymentGateway.API.Repositories;
using PaymentGateway.API.Responses;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentGateway.API.Handlers
{
    public class CreatePaymentHandler : IRequestHandler<CreatePaymentCommand, PaymentResponse>
    {
        private readonly IPaymentsCommandRepository _paymentsCommandRepository;
        private readonly IAcquiringBankService _acquiringBankService;

        public CreatePaymentHandler(IPaymentsCommandRepository paymentRepository, IAcquiringBankService acquiringBankService, IMapper mapper)
        {
            _paymentsCommandRepository = paymentRepository;
            _acquiringBankService = acquiringBankService;
        }
        public async Task<PaymentResponse> Handle(CreatePaymentCommand command, CancellationToken cancellationToken)
        {
            //TODO: validation
            var paymentCreated = ConstructPaymentCreatedEvent(command);
            var paymentCreatedEvent = new PaymentEvent(Guid.NewGuid(), paymentCreated.PaymentId, paymentCreated.Status, paymentCreated.CreatedDateTimeUTC, JsonConvert.SerializeObject(paymentCreated));
            await _paymentsCommandRepository.CreatePaymentEvent(paymentCreatedEvent);
            _ = Task.Run(async () =>
                  {
                      var aquiringBankResponse = await _acquiringBankService.SendPayment(paymentCreated);
                      var paymentEvent = CreateBankResponseEvent(paymentCreated, aquiringBankResponse);
                      await _paymentsCommandRepository.CreatePaymentEvent(paymentEvent);
                  });

            return new PaymentResponse(new List<PaymentEvent>() { paymentCreatedEvent});
        }

        private PaymentCreated ConstructPaymentCreatedEvent(CreatePaymentCommand command)
        {
            
            var payment = new PaymentCreated(Guid.NewGuid(),
                new Merchant()
                {
                    Id = Guid.NewGuid(),
                    Name = "Amazon" //TODO: hardcoded, authentication needed 
                },
                new Bank()
                {
                    Id = Guid.NewGuid(),
                    Name = "Santander" //TODO: hardcoded, another service needed to workout what bank we need to post to? can this be found from card details?
                },
                new Card()
                {
                    Id = Guid.NewGuid(),
                    CardNumber = command.Card.CardNumber,
                    Expiry = command.Card.Expiry,
                    Cvv = command.Card.Cvv
                },
                new Amount()
                {
                    Currency = command.Amount.Currency,
                    Value = command.Amount.Value
                });            
            return payment;
        }

        private PaymentEvent CreateBankResponseEvent(PaymentCreated paymentCreated, AcquiringBankResponse response) 
        {

            switch (response.Status)
            {
                case PaymentStatus.BankValidated:
                    var paymentSuccess = new PaymentSuccessful(response);
                    return new PaymentEvent(Guid.NewGuid(), paymentCreated.PaymentId, response.Status, response.CreatedDateTimeUTC, Newtonsoft.Json.JsonConvert.SerializeObject(paymentSuccess));
                case PaymentStatus.BankRejected:
                    var paymentRejected = new PaymentRejected(response);
                    return new PaymentEvent(Guid.NewGuid(), paymentCreated.PaymentId, response.Status, response.CreatedDateTimeUTC, Newtonsoft.Json.JsonConvert.SerializeObject(paymentRejected));
                case PaymentStatus.BankUnavailable:
                    var paymentUnavailable = new PaymentUnavailable(response);
                    return new PaymentEvent(Guid.NewGuid(), paymentCreated.PaymentId, response.Status, response.CreatedDateTimeUTC, Newtonsoft.Json.JsonConvert.SerializeObject(paymentUnavailable));
                default:
                    Log.Logger.Error($"Payment with id: {paymentCreated.PaymentId} recieved an unexpected response from bank");
                    throw new InvalidEnumArgumentException($"Bank response should not have a payment status of: {response.Status}");

            }            
        }
    }
}
