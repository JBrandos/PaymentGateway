using NFluent;
using NUnit.Framework;
using PaymentGateway.API.Request;
using PaymentGateway.API.Validators;
using PaymentGateway.API.Models;

namespace PaymentGateway.Tests.Validators
{
    public class PaymentRequestValidatorTest
    {        
        [TestCase("GBP", 100.0, "4444222233336666", "123", "03/23")]
        [TestCase("zzz", 100.1234567, "1234567891011121", "4321", "12/25")]
        [TestCase("uSd", 1000000.111, "4444222233336669", "8313", "12/30")]
        [TestCase("sml", 0100.0, "4444222233336666", "123", "03/23")]
        [TestCase("asd", 000100.0133, "0000222233336666", "1234", "1/27")]
        public void GivenValidPaymentRequestReturnsIsValid(string amountCurrency, decimal amountValue, string cardNumber, string cardCvv, string cardExpiry)
        {
            //arrange
            var paymentRequestValidator = new PaymentRequestValidator();
            var paymentRequest = new PaymentRequest()
            {
                Amount = new Amount()
                {
                    Currency = amountCurrency,
                    Value = amountValue
                },
                Card = new Card()
                {
                    CardNumber = cardNumber,
                    Cvv = cardCvv,
                    Expiry = cardExpiry
                }
            };
            //act
            var result = paymentRequestValidator.Validate(paymentRequest);
            //assert
            Check.That(result.IsValid);
        }

        [TestCase("GBPGBPGBPGBPG", 0, "thisisntacardnumber", "secretCode132", "april23")]
        [TestCase("", 0000.0000, "", "", "")]
        [TestCase("!*-", -100, "4444222233", "8313", "13/30")]

        public void GivenValidPaymentRequestReturnsIsInvalid(string amountCurrency, decimal amountValue, string cardNumber, string cardCvv, string cardExpiry)
        {
            //arrange
            var paymentRequestValidator = new PaymentRequestValidator();
            var paymentRequest = new PaymentRequest()
            {
                Amount = new Amount()
                {
                    Currency = amountCurrency,
                    Value = amountValue
                },
                Card = new Card()
                {
                    CardNumber = cardNumber,
                    Cvv = cardCvv,
                    Expiry = cardExpiry
                }
            };
            //act
            var result = paymentRequestValidator.Validate(paymentRequest);
            //assert
            Check.That(!result.IsValid); //TODO: this should be extended to check each error more precisely 
        }
    }
}
