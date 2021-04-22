using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NFluent;
using NUnit.Framework;
using PaymentGateway.API.Handlers;
using PaymentGateway.API.Models;
using PaymentGateway.API.Queries;
using PaymentGateway.API.Repositories;
using PaymentGateway.API.Responses;

namespace PaymentGateway.Tests.Handlers
{
    public class GetPaymentByIdHandlerTest
    {
        private Mock<IPaymentsQueryRepository> _mockPaymentsQueryRepository;

        [SetUp]
        public void SetUp()
        {
            _mockPaymentsQueryRepository = new Mock<IPaymentsQueryRepository>();
        }

        [Test]
        public async Task GivenAValidRequestReturnsPendingPaymentResponse()
        {
            //arrange
            var getPaymentByIdHandler = new GetPaymentByIdHandler(_mockPaymentsQueryRepository.Object);
            var id = Guid.NewGuid();
            var getPaymentByIdQuery = new GetPaymentByIdQuery(id);
            var paymentEvent = new PaymentEvent(Guid.NewGuid(), Guid.NewGuid(), PaymentStatus.Pending, DateTime.Now, "{\"Status\":\"Pending\",\"PaymentId\":\"3f652873-9af3-4308-b257-c8d4ea8b6826\",\"Merchant\":{\"Id\":\"2f81036d-96a1-452b-9fa8-7262d53c6898\",\"Name\":\"Amazon\"},\"Bank\":{\"Id\":\"8fcd7a25-47cb-4841-ba97-cce19a52afdd\",\"Name\":\"Santander\"},\"Card\":{\"Id\":\"8409e2a5-ab07-4c90-8647-84cdf1cf1d56\",\"CardNumber\":\"4444555566661234\",\"Expiry\":\"03/23\",\"Cvv\":\"123\"},\"Amount\":{\"Currency\":\"GBP\",\"Value\":1000.12345},\"CreatedDateTimeUTC\":\"2021-04-22T00:03:33.2624873+00:00\"}");

            var paymentEvents = new List<PaymentEvent>() { paymentEvent };
            _mockPaymentsQueryRepository.Setup(x => x.GetPaymentEventsByIdAsync(It.IsAny<Guid>())).ReturnsAsync(paymentEvents).Verifiable();
            
            //act
            var result = await getPaymentByIdHandler.Handle(getPaymentByIdQuery, new System.Threading.CancellationToken());
            
            //assert
            _mockPaymentsQueryRepository.Verify();
            Check.That(result).IsInstanceOf<PaymentResponse>();
            Check.That(result.Amount.Value).IsEqualTo(1000.12345M);
            Check.That(result.Amount.Currency).IsEqualTo("GBP");
            Check.That(result.Card.CardNumber).IsEqualTo("XXXXXXXXXXXX1234");
            Check.That(result.Card.Cvv).IsEqualTo("123");
            Check.That(result.Card.Expiry).IsEqualTo("03/23");
            Check.That(result.Status).IsEqualTo(PaymentStatus.Pending);
        }

        [Test]
        public async Task GivenAValidRequestReturnsBankValidatedPaymentResponse()
        {
            //arrange
            var getPaymentByIdHandler = new GetPaymentByIdHandler(_mockPaymentsQueryRepository.Object);
            var getPaymentByIdQuery = new GetPaymentByIdQuery(Guid.NewGuid());
            var paymentEvent1 = new PaymentEvent(Guid.NewGuid(), Guid.NewGuid(), PaymentStatus.Pending, DateTime.UtcNow, "{\"Status\":\"Pending\",\"PaymentId\":\"3f652873-9af3-4308-b257-c8d4ea8b6826\",\"Merchant\":{\"Id\":\"2f81036d-96a1-452b-9fa8-7262d53c6898\",\"Name\":\"Amazon\"},\"Bank\":{\"Id\":\"8fcd7a25-47cb-4841-ba97-cce19a52afdd\",\"Name\":\"Santander\"},\"Card\":{\"Id\":\"8409e2a5-ab07-4c90-8647-84cdf1cf1d56\",\"CardNumber\":\"4444555566661234\",\"Expiry\":\"03/23\",\"Cvv\":\"123\"},\"Amount\":{\"Currency\":\"GBP\",\"Value\":1000.12345},\"CreatedDateTimeUTC\":\"2021-04-22T00:03:33.2624873+00:00\"}");
            var paymentEvent2 = new PaymentEvent(Guid.NewGuid(), Guid.NewGuid(), PaymentStatus.BankValidated, DateTime.UtcNow, "{\"Response\":{\"Id\":\"a8566371-befa-47c0-a0ab-249df0044889\",\"Status\":1,\"BankResponse\":\"{\\\"id\\\":\\\"2c798f19-4f5e-4efc-a935-48498e98b623\\\",\\\"statusCode\\\": \\\"1\\\"}\",\"CreatedDateTimeUTC\":\"2021-04-22T00:03:46.464346+00:00\"}}");
            var paymentEvents = new List<PaymentEvent>() { paymentEvent1, paymentEvent2 };
            _mockPaymentsQueryRepository.Setup(x => x.GetPaymentEventsByIdAsync(It.IsAny<Guid>())).ReturnsAsync(paymentEvents).Verifiable();

            //act
            var result = await getPaymentByIdHandler.Handle(getPaymentByIdQuery, new System.Threading.CancellationToken());

            //assert
            _mockPaymentsQueryRepository.Verify();
            Check.That(result).IsInstanceOf<PaymentResponse>();
            Check.That(result.Amount.Value).IsEqualTo(1000.12345M);
            Check.That(result.Amount.Currency).IsEqualTo("GBP");
            Check.That(result.Card.CardNumber).IsEqualTo("XXXXXXXXXXXX1234");
            Check.That(result.Card.Cvv).IsEqualTo("123");
            Check.That(result.Card.Expiry).IsEqualTo("03/23");
            Check.That(result.Status).IsEqualTo(PaymentStatus.BankValidated);
        }

        [Test]
        public async Task GivenPaymentDoesNotExistReturnsNull()
        {
            //arrange
            var getPaymentByIdHandler = new GetPaymentByIdHandler(_mockPaymentsQueryRepository.Object);
            var id = Guid.NewGuid();
            var getPaymentByIdQuery = new GetPaymentByIdQuery(id);
            var paymentEvents = new List<PaymentEvent>();
            _mockPaymentsQueryRepository.Setup(x => x.GetPaymentEventsByIdAsync(It.IsAny<Guid>())).ReturnsAsync(paymentEvents).Verifiable();

            //act
            var result = await getPaymentByIdHandler.Handle(getPaymentByIdQuery, new System.Threading.CancellationToken());

            //assert
            _mockPaymentsQueryRepository.Verify();
            Check.That(result).IsEqualTo(null);
        }

    }
}
