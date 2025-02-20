using Dapper;
using exchangesms;
using exchangesms.data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<MessageRepository> _logger;

        public MessageRepository(string connectionString, ILogger<MessageRepository> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }
        public void AddMessage(Message message)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var ms = Message.Mapper.Map(message);
                    string sqlquery = "INSERT INTO Messages(Text,Timestamp,SequenceNumber) Values (@Text,@TimeStamp,@SequenceNumber); SELECT SCOPE_IDENTITY(); ";
                    connection.Open();
                    ms.Id = connection.ExecuteScalar<int>(sqlquery, ms);
                    _logger.LogInformation("Message added to database: SequenceNumber = {SequenceNumber}", ms.SequenceNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding message to database.");
                throw; // Перебрасываем исключение для обработки выше
            }
        }

        public IEnumerable<Message> GetMessages(DateTime start, DateTime end)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT * from Messages WHERE Timestamp>=@start AND Timestamp<=@end Order by Timestamp;";
                    return connection.Query<MessageDto>(query, new { start = start, end = end }).Select(i => Message.Mapper.Map(i));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages from database.");
                return Enumerable.Empty<Message>(); // Или другой подходящий способ обработки ошибки
            }
        }
    }
}