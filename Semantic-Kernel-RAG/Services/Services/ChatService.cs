using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextGeneration;
using Newtonsoft.Json;
using Services.IService;

namespace Services.Service
{
    public class ChatService : IChatService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<ChatService> _logger;
        private readonly IKernelBuilder _kernel;
        public readonly string _apiUrl;
        public readonly double _temprature;
        public readonly int _maxtoken;
        public readonly string _model;
        private string ChatTemplate = @"You are a chatting system that try to solve the Query and Information mentioned in a proffesional way and be precise in nature \r\n \r\n ```Query:{{$query}}  \r\n Information:{{$information}}```";

        public ChatService(IConfiguration config, ILogger<ChatService> logger)
        {
            _config = config;
            if (_config["LLMUsed"] == "LMStudio")
            {
                _apiUrl = _config["LLM_LMStudio:endpoint"] ?? "";
                _maxtoken = int.Parse(_config["LLM_LMStudio:maxtoken"] ?? "-1");
                _temprature = double.Parse(_config["LLM_LMStudio:temprature"] ?? "0.1");
            }
            if(_config["LLMUsed"] == "Ollama"){
                _apiUrl = _config["LLM_Ollama:endpoint"] ?? "";
                _maxtoken = int.Parse(_config["LLM_Ollama:maxtoken"] ?? "-1");
                _temprature = double.Parse(_config["LLM_Ollama:temprature"] ?? "0.1");
                _model=_config["LLM_Ollama:model"]??"tinyllama";
            }
            _logger = logger;
        }
        //To be implmented
        public async Task<string> ChattingWithLLM(string query, string information)
        {
            //Intitalizing The Kernel
            IKernelBuilder builder = Kernel.CreateBuilder();
            // Add your text generation service as a singleton instance of the Kernel
            if (_config["LLMUsed"] == "LMStudio")
            {
                builder.Services.AddKeyedSingleton<ITextGenerationService>("myService1", new LMStudioTextGenerationService(_apiUrl, _maxtoken, _temprature));
            // Add your text generation service as a factory method
            builder.Services.AddKeyedSingleton<ITextGenerationService>("myService2", (_, _) => new LMStudioTextGenerationService(_apiUrl, _maxtoken, _temprature));
            }
            if(_config["LLMUsed"] == "Ollama"){
                builder.Services.AddKeyedSingleton<ITextGenerationService>("myService1", new OllamaTextGeneration(_apiUrl, _maxtoken, _temprature,_model));
            // Add your text generation service as a factory method
            builder.Services.AddKeyedSingleton<ITextGenerationService>("myService2", (_, _) => new OllamaTextGeneration(_apiUrl, _maxtoken, _temprature,_model));
            }
            
            
            //Build the Kernel
            Kernel kernel = builder.Build();
            //Function Defined
            var chatFunction = kernel.CreateFunctionFromPrompt(ChatTemplate);
            _logger.LogInformation($"Function input: {query}\n");
            //Run the Prompt
            var result = await chatFunction.InvokeAsync(kernel, new() { ["query"] = query, ["information"] = information });
            return result.ToString();

        }
    }

}
