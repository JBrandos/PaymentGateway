# PaymentGateway
RESTful API that allows merchants to process a card payment

## Overview
- Uses C# ASP.NET Web API 2
- 2 requests, 1 to let a merchant send a payment for processing and the other to check that payment 
- CQRS architecture
- EventSourcing
- Swagger
- Serilog logging
- ApiKey authentication
- Encryption (https)
- Data storage (localdb Microsoft SQL Server)

## How to use
- Clone repository and open solution in IDE
- Check appsettings.Development.json ConnectionStrings:PaymentsDb is pointing to your localdb
- Run the PaymentGateway.API as startup project (Not in IIS)
- Swagger doc should load, server should start 
- **To authenticate requests**, ensure the header contains key:value ApiKey:SuperSecretKey 
- Requests are:
  - POST https://localhost:5001/payments example body below
  `
  {
  "card": {
    "cardNumber": "4444555566661234",
    "expiry": "03/23",
    "cvv": "123"
  },
  "amount": {
    "currency": "GBP",
    "value": 1000.1234
  }
}
  `
  - GET https://localhost:5001/payments/{id}

## Implementation notes
### Interpretation of requirements and assumptions
The document gives little detail on domain specific questions like:
- What is the expected rate of requests coming from merchants?
  - I've assumed and coded for a higher rate
- What delay do merchants expect when posting a payment
  - I assumed they would expect minimal delay, meaning they will get a 201 response with _pending_ status before the acquiring bank has validated their request. A consequence of this is that if they need bank validation to be complete before processing a shopper's payment then they would have to query their payment repeatedly until the bank had responded to the gateways request and hence changed that payment's status to _BankValidated_. The alternative would be to wait for the bank's validation to come through but then there would be a larger delay
- Can the acquiring bank be derived from card details?
  - When creating the AcquiringBankService I thought about this - how do we know where to forward the payment onto, are those details derived from the card information? For now the AcquiringBankService acts as a mock service that doesn't have to know where to send it's post request (Task.Delay(1000))
- How do we detect duplicate payment requests if at all - do they send a paymentId or does the api attribute one?
  - I assumed that the gateway is attributing payments id's rather than the merchants. This means that if the merchant accidentally double posted, both payments would go through. If I've assumed wrongly then the fix would be to have a duplicate paymentId check that returns some validation error before the _PaymentCreated_ event is stored
- Does the acquiring bank need to know merchant details?
  - I've assumed not so no merchant details are included in AcquiringBankRequest
- Should card details beside card number be masked?
  - There are likely security standards online to find out but I've assumed just the card number needs masking
- Should the bank's response be stored in the event data?
  - I think so, just so that its logged and can be checked if something unexpected gets sent which breaks the system

### CQRS 
I decided to use the CQRS pattern to separate read and writes. This means if there was a requirement for more throughput - which is likely for a growing number of merchants - then we'd be more easily able to scale and optimise seperate read and write models.

### EventSourcing
The api is structured so that data is stored as events. This means that when a merchant queries a payment, it's constructed from a list of PaymentEvents that give the client - and payment gateway - all the information they need. This has advantages like self-logging data and improved write performance because its only additive.
## What could be improved
If I had more knowledge and time...
### Security
Currently the api is not ready for production because it has security concerns like:
 - Weak authentication: ApiKey authentication is not ideal because if the secret was revealed - which it has been by me checking in my _appsettings.Development.json_ - then everyone could access the apis functionality. The next step would be to use something like JWT tokens or an identity provider solution.
 - Poor storage and handling of sensitive information: The card details are currently stored as plain-text in a database. Im sure there is plenty of regulation on this topic and at a minimum that information should be hashed and salted. Whilst looking into it I also found "SecureString" which is a safer way of handling strings that contain sensitive information in code and would be beneficial in the case of an attack  
### Messaging System
Resiliency is particularly important when dealing with fast important and sensitive data. This aspect could be improved with a messaging system so that when the api receives a new payment, its added to a queue so its no longer reliant on the api maintaining its availability - it could have downtime and the payment be read by another api.
### Testing & Performance
There are no integration tests and the unit tests could definitely be improved in terms of the cases they cover and the code itself made cleaner. Minimal performance testing has been done using Postmans "runner" tool. Running 100 tests one after the other it maintains a low delay 10-15ms for both requests, but results may be very different if many requests came at once.
### Cache recently made payments
In this use case it seems like caching recent orders for a few seconds (or however long its expected to take for the bank to respond) would greatly improve performance because clients are expected to keep querying until they get BankValidated and so the same payment is reconstructed from the payment events repeatedly. Caching would fix this because on query we could first check the cache for recent payments rather than connecting to the db, reading, constructing and then sending the response back.
