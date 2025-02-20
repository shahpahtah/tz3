using exchangesms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public interface IMessageRepository
    {
        void AddMessage(Message message);
        IEnumerable<Message> GetMessages(DateTime start, DateTime end);
    }
}
