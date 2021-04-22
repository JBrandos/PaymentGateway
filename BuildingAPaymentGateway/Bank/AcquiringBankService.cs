using AutoMapper;
using PaymentGateway.API.Models;
using PaymentGateway.API.Request;
using PaymentGateway.API.Responses;
using Serilog;
using System.Threading.Tasks;

namespace PaymentGateway.API
{
    public interface IAcquiringBankService
    {
        Task<AcquiringBankResponse> SendPayment(PaymentCreated payment);
    }

    public class AcquiringBankService : IAcquiringBankService
    {
        private readonly IBankResponseNormaliser _bankResponseNormaliser;
        private readonly IMapper _mapper;
        private readonly int MaxRetries = 3;

        public AcquiringBankService(IBankResponseNormaliser bankResponseNormaliser, IMapper mapper)
        {
            _bankResponseNormaliser = bankResponseNormaliser;
            _mapper = mapper;
        }
        public async Task<AcquiringBankResponse> SendPayment(PaymentCreated payment)
        {
            var request = _mapper.Map<AcquiringBankRequest>(payment);
            var responseFromBank = await SendPaymentApiRequest(request);             
            var acquiredBankResponse =  _bankResponseNormaliser.Normalise(responseFromBank);
            
            if (acquiredBankResponse.Status == PaymentStatus.BankUnavailable) 
            {
                await RetrySendingPaymentToBank(request, acquiredBankResponse);
            }
            
            return acquiredBankResponse;
        }

        private async Task<string> SendPaymentApiRequest(AcquiringBankRequest request)
        {
            //simulate the delay in waiting for a web request
            await Task.Delay(10000);
            var result = @"{""id"":""2c798f19-4f5e-4efc-a935-48498e98b623"",""statusCode"": ""1""}";
            Log.Information($"Payment with id: {request.PaymentId} has succesfully been sent to the bank");
            return result;
        }

        // this could absolutely be improved, something like Polly Policies / different behaviour like storing it to be retried significantly later
        private async Task<AcquiringBankResponse> RetrySendingPaymentToBank(AcquiringBankRequest request, AcquiringBankResponse normalisedResponse)
        {
            int increment = 0;
            while (increment < MaxRetries && normalisedResponse.Status == PaymentStatus.BankUnavailable)
            {
                increment++;
                Log.Information($"Payment with id: {request.PaymentId} did not receive a response from the bank. Retrying {increment} of {MaxRetries}");
                var responseFromBank = await SendPaymentApiRequest(request);
                normalisedResponse = _bankResponseNormaliser.Normalise(responseFromBank);
            }
            if (increment == MaxRetries)
            {
                Log.Error($"Payment with id: {request.PaymentId} cannot be sent to the bank. Max retries reached");
            }            
            return normalisedResponse;
        }
    }
}
