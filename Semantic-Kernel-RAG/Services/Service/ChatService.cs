using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Services.IService;

namespace Services.Service
{
    public class ChatService:IChatService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<ChatService> _logger;
        private readonly IKernelBuilder _kernel;
        private string ChatTemplate = @"You are a chatting system that try to solve the query mentioned in a proffesional way and be precise in nature
```Query:{query}```";

        public ChatService(IConfiguration config, ILogger<ChatService> logger, IKernelBuilder kernel)
        {
            _config = config;
            _logger = logger;
            _kernel = kernel;
        }
        //To be implmented
        public async Task<string> ChattingWithLLM(string query) {
            return query;
        
        }

    }
}
