using PaymentGateway.API.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace PaymentGateway.API.Repositories
{
    public interface IPaymentsQueryRepository
    {
        Task<List<PaymentEvent>> GetPaymentEventsByIdAsync(Guid id);
    }
    public class PaymentsQueryRepository : IPaymentsQueryRepository
    {

        private readonly string _connectionString;

        public PaymentsQueryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<PaymentEvent>> GetPaymentEventsByIdAsync(Guid id)
        {
            var sql =
                @"
SELECT EventId, PaymentId, EventType, EventData, CreatedDateTimeUtc
  FROM PaymentsGateway.dbo.PaymentEvents
  WHERE PaymentId = @PaymentId";

            List<PaymentEvent> paymentEvents = new List<PaymentEvent>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@PaymentId", id);
                using (var reader = await command.ExecuteReaderAsync())
                {                    
                    while (await reader.ReadAsync())
                    {
                        paymentEvents.Add(new PaymentEvent((Guid)reader["EventId"],
                                (Guid)reader["PaymentId"],
                                Enum.Parse<PaymentStatus>(reader["EventType"].ToString()),
                                (DateTimeOffset)reader["CreatedDateTimeUtc"],
                                (string)reader["EventData"])
                        );
                    }
                }
                return paymentEvents;
            }
        }
    }
}