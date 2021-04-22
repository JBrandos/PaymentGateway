using AutoMapper;
using BuildingAPaymentGateway.Controllers;
using MediatR;
using Moq;
using NUnit.Framework;
using PaymentGateway.API.Request;
using PaymentGateway.API.Models;
using System.Threading.Tasks;
using NFluent;
using PaymentGateway.API.Commands;
using PaymentGateway.API;
using PaymentGateway.API.Responses;
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.API.Queries;

namespace BuildingAPaymentGateway.Tests
{
    public class PaymentsControllerTest
    {
        private Mock<IMediator> _mockMediator;
        private IMapper _mapper;
        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            var profile = new AutoMapperProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(profile));
            _mapper = new Mapper(configuration);
        }

        [Test]
        public async Task GivenValidPaymentRequestSends201ResponseWithCreatedObject()
        {
            //arrange
            var controller = new PaymentsController(_mockMediator.Object, _mapper);
            var paymentRequest = new PaymentRequest()
            {
                Amount = new Amount()
                {
                    Currency = "GBP", Value = 123.345M
                },
                Card = new Card()
                {
                    CardNumber = "1111222233334444",
                    Cvv = "123",
                    Expiry = "01/23"
                }
            };
            var id = Guid.NewGuid();
            var paymentEvent = new PaymentEvent(Guid.NewGuid(), id, PaymentStatus.Pending, DateTime.UtcNow, "{\"Status\":\"Pending\",\"PaymentId\":\"3f652873-9af3-4308-b257-c8d4ea8b6826\",\"Merchant\":{\"Id\":\"2f81036d-96a1-452b-9fa8-7262d53c6898\",\"Name\":\"Amazon\"},\"Bank\":{\"Id\":\"8fcd7a25-47cb-4841-ba97-cce19a52afdd\",\"Name\":\"Santander\"},\"Card\":{\"Id\":\"8409e2a5-ab07-4c90-8647-84cdf1cf1d56\",\"CardNumber\":\"4444555566661234\",\"Expiry\":\"03/23\",\"Cvv\":\"123\"},\"Amount\":{\"Currency\":\"GBP\",\"Value\":1000.12345},\"CreatedDateTimeUTC\":\"2021-04-22T00:03:33.2624873+00:00\"}");
            
            var paymentEvents = new List<PaymentEvent>() { paymentEvent };
            Task<PaymentResponse> paymentResponse = Task.FromResult(new PaymentResponse(paymentEvents));

            _mockMediator.Setup(x => x.Send(It.IsAny<CreatePaymentCommand>(), It.IsAny<CancellationToken>())).Returns(paymentResponse).Verifiable();

            //act
            var createPaymentResponse = await controller.CreatePayment(paymentRequest, _mapper);
            //assert

            _mockMediator.Verify();
            Check.That(createPaymentResponse).IsInstanceOf<CreatedAtActionResult>();
            var result = (CreatedAtActionResult)createPaymentResponse;
            Check.That(result.Value).IsInstanceOf<PaymentResponse>();
            var value = (PaymentResponse)result.Value;
            Check.That(result.ActionName).IsEqualTo(nameof(controller.GetPayment));
            Check.That(result.StatusCode).IsEqualTo(201);
            Check.That(value.PaymentId).IsEqualTo(id);
            Check.That(value.Amount.Value).IsEqualTo(1000.12345M);
            Check.That(value.Amount.Currency).IsEqualTo("GBP");
            Check.That(value.Card.CardNumber).IsEqualTo("XXXXXXXXXXXX1234");
            Check.That(value.Card.Cvv).IsEqualTo("123"); 
            Check.That(value.Card.Expiry).IsEqualTo("03/23");
            Check.That(value.Status).IsEqualTo(PaymentStatus.Pending);
        }

        [Test]
        public async Task GivenValidGetPaymentIdPendingSends200ResponseWithCorrectObject()
        {
            //arrange
            var controller = new PaymentsController(_mockMediator.Object, _mapper);

            var id = Guid.NewGuid();
            var paymentEvent = new PaymentEvent(Guid.NewGuid(), id, PaymentStatus.Pending, DateTime.UtcNow, "{\"Status\":\"Pending\",\"PaymentId\":\"3f652873-9af3-4308-b257-c8d4ea8b6826\",\"Merchant\":{\"Id\":\"2f81036d-96a1-452b-9fa8-7262d53c6898\",\"Name\":\"Amazon\"},\"Bank\":{\"Id\":\"8fcd7a25-47cb-4841-ba97-cce19a52afdd\",\"Name\":\"Santander\"},\"Card\":{\"Id\":\"8409e2a5-ab07-4c90-8647-84cdf1cf1d56\",\"CardNumber\":\"4444555566661234\",\"Expiry\":\"03/23\",\"Cvv\":\"123\"},\"Amount\":{\"Currency\":\"GBP\",\"Value\":1000.12345},\"CreatedDateTimeUTC\":\"2021-04-22T00:03:33.2624873+00:00\"}");
            var paymentEvents = new List<PaymentEvent>() { paymentEvent };
            var paymentResponse = new PaymentResponse(paymentEvents);
            _mockMediator.Setup(x => x.Send(It.IsAny<GetPaymentByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(paymentResponse).Verifiable();

            //act
            var getPaymentResponse = await controller.GetPayment(id);
            //assert

            _mockMediator.Verify();
            Check.That(getPaymentResponse).IsInstanceOf<OkObjectResult>();
            var result = (OkObjectResult)getPaymentResponse;
            Check.That(result.Value).IsInstanceOf<PaymentResponse>();
            var value = (PaymentResponse)result.Value;
            Check.That(result.StatusCode).IsEqualTo(200);
            Check.That(value.PaymentId).IsEqualTo(id);
            Check.That(value.Amount.Value).IsEqualTo(1000.12345M);
            Check.That(value.Amount.Currency).IsEqualTo("GBP");
            Check.That(value.Card.CardNumber).IsEqualTo("XXXXXXXXXXXX1234");
            Check.That(value.Card.Cvv).IsEqualTo("123");
            Check.That(value.Card.Expiry).IsEqualTo("03/23");
            Check.That(value.Status).IsEqualTo(PaymentStatus.Pending);
        }

        [Test]
        public async Task GivenValidGetPaymentIdBankValidatedSends200ResponseWithCorrectObject()
        {
            //arrange
            var controller = new PaymentsController(_mockMediator.Object, _mapper);

            var id = Guid.NewGuid();
            var paymentEventPending = new PaymentEvent(Guid.NewGuid(), id, PaymentStatus.Pending, DateTime.UtcNow, "{\"Status\":\"Pending\",\"PaymentId\":\"3f652873-9af3-4308-b257-c8d4ea8b6826\",\"Merchant\":{\"Id\":\"2f81036d-96a1-452b-9fa8-7262d53c6898\",\"Name\":\"Amazon\"},\"Bank\":{\"Id\":\"8fcd7a25-47cb-4841-ba97-cce19a52afdd\",\"Name\":\"Santander\"},\"Card\":{\"Id\":\"8409e2a5-ab07-4c90-8647-84cdf1cf1d56\",\"CardNumber\":\"4444555566661234\",\"Expiry\":\"03/23\",\"Cvv\":\"123\"},\"Amount\":{\"Currency\":\"GBP\",\"Value\":1000.12345},\"CreatedDateTimeUTC\":\"2021-04-22T00:03:33.2624873+00:00\"}");
            var paymentEventBankValidated = new PaymentEvent(Guid.NewGuid(), id, PaymentStatus.BankValidated, DateTime.UtcNow, "{\"Response\":{\"Id\":\"a8566371-befa-47c0-a0ab-249df0044889\",\"Status\":1,\"BankResponse\":\"{\\\"id\\\":\\\"2c798f19-4f5e-4efc-a935-48498e98b623\\\",\\\"statusCode\\\": \\\"1\\\"}\",\"CreatedDateTimeUTC\":\"2021-04-22T00:03:46.464346+00:00\"}}");
            var paymentEvents = new List<PaymentEvent>() { paymentEventPending, paymentEventBankValidated};
            var paymentResponse = new PaymentResponse(paymentEvents);
            _mockMediator.Setup(x => x.Send(It.IsAny<GetPaymentByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(paymentResponse).Verifiable();

            //act
            var getPaymentResponse = await controller.GetPayment(id);
            //assert

            _mockMediator.Verify();
            Check.That(getPaymentResponse).IsInstanceOf<OkObjectResult>();
            var result = (OkObjectResult)getPaymentResponse;
            Check.That(result.Value).IsInstanceOf<PaymentResponse>();
            var value = (PaymentResponse)result.Value;
            Check.That(result.StatusCode).IsEqualTo(200);
            Check.That(value.PaymentId).IsEqualTo(id);
            Check.That(value.Amount.Value).IsEqualTo(1000.12345M);
            Check.That(value.Amount.Currency).IsEqualTo("GBP");
            Check.That(value.Card.CardNumber).IsEqualTo("XXXXXXXXXXXX1234");
            Check.That(value.Card.Cvv).IsEqualTo("123");
            Check.That(value.Card.Expiry).IsEqualTo("03/23");
            Check.That(value.Status).IsEqualTo(PaymentStatus.BankValidated);
        }
    }
}