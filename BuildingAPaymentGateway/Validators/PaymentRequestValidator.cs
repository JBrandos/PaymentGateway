using FluentValidation;
using PaymentGateway.API.Request;

namespace PaymentGateway.API.Validators
{
    public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
    {
        public PaymentRequestValidator()
        {
            RuleFor(x => x.Card.CardNumber)
                .NotEmpty()
                .Matches("^(?:4[0-9]{12}(?:[0-9]{3})?|[25][1-7][0-9]{14}|6(?:011|5[0-9][0-9])[0-9]{12}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|(?:2131|1800|35\\d{3})\\d{11})$");
            RuleFor(x => x.Card.Cvv)
                .NotEmpty()
                .Matches("^[0-9]{3,4}$");
            RuleFor(x => x.Card.Expiry)
                .NotEmpty()
                .Matches("(0[1-9]|10|11|12)/[0-9]{2}$"); //TODO: add check so that its expiry isnt in the past
            RuleFor(x => x.Amount.Currency)
                .NotEmpty()
                .Matches("[a-zA-Z]{3}");
            RuleFor(x => x.Amount.Value.ToString())
                .NotEmpty()
                .Matches("^(?!0\\d)\\d*(\\.\\d{1,5})?$");
        }
    }
}
