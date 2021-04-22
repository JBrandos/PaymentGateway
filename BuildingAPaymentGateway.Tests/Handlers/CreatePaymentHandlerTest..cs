using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using NFluent;
using NUnit.Framework;
using PaymentGateway.API;
using PaymentGateway.API.Commands;
using PaymentGateway.API.Handlers;
using PaymentGateway.API.Models;
using PaymentGateway.API.Repositories;
using PaymentGateway.API.Responses;

namespace PaymentGateway.Tests.Handlers
{
    public class CreatePaymentHandlerTest
    {
        private Mock<IPaymentsCommandRepository> _mockPaymentsCommandRepository;
        private Mock<IAcquiringBankService> _mockAcquiringBankService;
        private IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            _mockPaymentsCommandRepository = new Mock<IPaymentsCommandRepository>();
            _mockAcquiringBankService = new Mock<IAcquiringBankService>();
            var profile = new AutoMapperProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(profile));
            _mapper = new Mapper(configuration);
        }

        [Test]
        public async Task GivenAValidCreatePaymentCommandReturnsPendingPaymentResponse()
        {
            //arrange
            var createPaymentHandler = new CreatePaymentHandler(_mockPaymentsCommandRepository.Object, _mockAcquiringBankService.Object,_mapper);
            var id = Guid.NewGuid();
            var createPaymentCommand = new CreatePaymentCommand()
            {
                Amount = new Amount()
                {
                    Currency = "GBP",
                    Value = 1000.12345M
                },
                Card = new Card()
                {
                    CardNumber = "4444555566661234",
                    Cvv = "123",
                    Expiry = "03/23",
                    Id = id
                }
            };
            var paymentEventPending = new PaymentEvent(Guid.NewGuid(), id, PaymentStatus.Pending, DateTime.UtcNow, "{\"Status\":\"Pending\",\"PaymentId\":\"3f652873-9af3-4308-b257-c8d4ea8b6826\",\"Merchant\":{\"Id\":\"2f81036d-96a1-452b-9fa8-7262d53c6898\",\"Name\":\"Amazon\"},\"Bank\":{\"Id\":\"8fcd7a25-47cb-4841-ba97-cce19a52afdd\",\"Name\":\"Santander\"},\"Card\":{\"Id\":\"8409e2a5-ab07-4c90-8647-84cdf1cf1d56\",\"CardNumber\":\"4444555566661234\",\"Expiry\":\"03/23\",\"Cvv\":\"123\"},\"Amount\":{\"Currency\":\"GBP\",\"Value\":1000.12345},\"CreatedDateTimeUTC\":\"2021-04-22T00:03:33.2624873+00:00\"}");
            var paymentEventBankValidated = new PaymentEvent(Guid.NewGuid(), id, PaymentStatus.BankValidated, DateTime.UtcNow, "{\"Response\":{\"Id\":\"a8566371-befa-47c0-a0ab-249df0044889\",\"Status\":1,\"BankResponse\":\"{\\\"id\\\":\\\"2c798f19-4f5e-4efc-a935-48498e98b623\\\",\\\"statusCode\\\": \\\"1\\\"}\",\"CreatedDateTimeUTC\":\"2021-04-22T00:03:46.464346+00:00\"}}");
            var paymentEvents = new List<PaymentEvent>() { paymentEventPending };
            var acquiringBankResponse = new AcquiringBankResponse(id, PaymentStatus.BankValidated, "{\\\"id\\\":\\\"2c798f19-4f5e-4efc-a935-48498e98b623\\\",\\\"statusCode\\\": \\\"1\\\"}\\", DateTime.UtcNow);
            _mockPaymentsCommandRepository.Setup(x => x.CreatePaymentEvent(It.IsAny<PaymentEvent>())).Verifiable();
            _mockAcquiringBankService.Setup(x => x.SendPayment(It.IsAny<PaymentCreated>())).ReturnsAsync(acquiringBankResponse).Verifiable();

            //act
            var result = await createPaymentHandler.Handle(createPaymentCommand, new System.Threading.CancellationToken());

            //assert
            _mockPaymentsCommandRepository.Verify(x => x.CreatePaymentEvent(It.IsAny<PaymentEvent>()), Times.AtLeastOnce());
            Check.That(result).IsInstanceOf<PaymentResponse>();
            Check.That(result.Amount.Value).IsEqualTo(1000.12345M);
            Check.That(result.Amount.Currency).IsEqualTo("GBP");
            Check.That(result.Card.CardNumber).IsEqualTo("XXXXXXXXXXXX1234");
            Check.That(result.Card.Cvv).IsEqualTo("123"); 
            Check.That(result.Card.Expiry).IsEqualTo("03/23");
            Check.That(result.Status).IsEqualTo(PaymentStatus.Pending);
        }
    }
}
