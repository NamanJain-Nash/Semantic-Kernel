using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Chat
{
    public class ChatOutput
    {
        public string ChatId { get; set; }
        public string UserQuery { get; set; }
        public string AiAnswer { get; set; }
    }
}
