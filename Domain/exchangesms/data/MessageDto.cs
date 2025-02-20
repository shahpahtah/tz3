using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace exchangesms.data
{
    public class MessageDto
    {
        [Key]
        public int Id { get; set; }
        public string Text { get; set; }
        public int SequenceNumber { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
