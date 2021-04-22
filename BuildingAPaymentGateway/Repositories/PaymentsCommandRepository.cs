using PaymentGateway.API.Models;
using Serilog;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace PaymentGateway.API.Repositories
{
    public interface IPaymentsCommandRepository
    {
        Task CreatePaymentEvent(PaymentEvent paymentEvent);
    }
    public class PaymentsCommandRepository : IPaymentsCommandRepository
    {
        private readonly string _connectionString;

        public PaymentsCommandRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task CreatePaymentEvent(PaymentEvent paymentEvent)
        {
            var sql =
                @"
BEGIN TRANSACTION
    INSERT INTO PaymentsGateway.dbo.PaymentEvents (EventId,	PaymentId, EventType, EventData, CreatedDateTimeUtc)
	VALUES (@EventId, @PaymentId, @EventType, @EventData, @CreatedDateTimeUtc);
COMMIT TRANSACTION";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@EventId", Guid.NewGuid());
                    command.Parameters.AddWithValue("@PaymentId", paymentEvent.PaymentId);
                    command.Parameters.AddWithValue("@EventType", paymentEvent.Status.ToString());
                    command.Parameters.AddWithValue("@EventData", paymentEvent.EventData);
                    command.Parameters.AddWithValue("@CreatedDateTimeUtc", paymentEvent.CreatedDateTimeUTC);
                    await command.ExecuteNonQueryAsync();
                }
            }
            Log.Information($"Created payment event type: {paymentEvent.Status} for payment: {paymentEvent.PaymentId} with event id: {paymentEvent.Id}");
        }
    }
}
