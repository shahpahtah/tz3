using exchangesms.data;
using System.Reflection.Metadata.Ecma335;

namespace exchangesms
{
    public class Message
    {
        MessageDto dto;
        public int Id { get => dto.Id; set => dto.Id = value; }
        public string Text { get => dto.Text; set => dto.Text = value; }
        public DateTime TimeStamp { get => dto.TimeStamp; set => dto.TimeStamp = value; }
        public int SequenceNumber { get => dto.SequenceNumber; set => dto.SequenceNumber = value; }
        internal Message(MessageDto dto)
        {
            this.dto = dto;
        }
        public static class DtoFactory
        {
            public static MessageDto Create(string text, DateTime date, int sequenceNumber)
            {
                return new MessageDto { Text = text, TimeStamp = date, SequenceNumber = sequenceNumber };
            }
        }
        public static class Mapper
        {
            public static Message Map(MessageDto dto)
            {
                return new Message(dto);
            }
            public static MessageDto Map(Message message)
            {
                return message.dto;
            }
        }

    }
}
