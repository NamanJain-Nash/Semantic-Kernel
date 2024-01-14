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
        private string ChatTemplate = @"You are a chatting system that try to solve the query mentioned in a proffesional way and be precise in nature
```Query:{{$input}}```";

        public ChatService(IConfiguration config, ILogger<ChatService> logger)
        {
            _config = config;
            _apiUrl = _config["LLM:endpoint"] ??"";
            _logger = logger;
        }
        //To be implmented
        public async Task<string> ChattingWithLLM(string query)
        {
            //Intitalizing The Kernel
            IKernelBuilder builder = Kernel.CreateBuilder();
            // Add your text generation service as a singleton instance of the Kernel
            builder.Services.AddKeyedSingleton<ITextGenerationService>("myService1", new MyTextGenerationService(_apiUrl));
            // Add your text generation service as a factory method
            builder.Services.AddKeyedSingleton<ITextGenerationService>("myService2", (_, _) => new MyTextGenerationService(_apiUrl));
            //Build the Kernel
            Kernel kernel = builder.Build();
            //Function Defined
            var chatFunction = kernel.CreateFunctionFromPrompt(ChatTemplate);
            _logger.LogInformation($"Function input: {query}\n");
            //Run the Prompt
            var result = await chatFunction.InvokeAsync(kernel, new() { ["input"] = query });
            return result.ToString();

        }
        private sealed class MyTextGenerationService : ITextGenerationService
        {
            private readonly string _apiUrl;

            // Constructor to receive the API URL
            public MyTextGenerationService(string apiUrl)
            {
                _apiUrl = apiUrl;
            }

            public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();
            public async IAsyncEnumerable<StreamingTextContent> GetStreamingTextContentsAsync(string prompt, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
            {
                string LLMResultText = "Not implemented";
                foreach (string word in LLMResultText.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    await Task.Delay(50, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();

                    yield return new StreamingTextContent($"{word} ");
                }
            }
            public async Task<IReadOnlyList<TextContent>> GetTextContentsAsync(string prompt, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
            {
                // Doing the HTTP call to the Local LLM Server
                string LLMResultText;

                // The API URL of Custom/Local LLM
                string apiUrl = _apiUrl;
                // Define the JSON payload
                string jsonPayload = @$"
            {{
                ""messages"": [
                    {{ ""role"": ""user"", ""content"": ""{prompt}"" }}
                ],
                ""temperature"": 1,
                ""max_tokens"": -1,
                ""stream"": false
            }}";

                // Create an HttpClient instance
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        // Create HttpRequestMessage and set the Content-Type header
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                        request.Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

                        // Send the request
                        HttpResponseMessage response = await client.SendAsync(request, cancellationToken);

                        // Check if the request was successful (status code 200-299)
                        if (response.IsSuccessStatusCode)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();
                            dynamic responseObject = JsonConvert.DeserializeObject(responseBody);
                            LLMResultText = responseObject.choices[0].message.content; ;
                        }
                        else
                        {
                            LLMResultText = "Failed to make the request. Status code: " + response.StatusCode;
                        }
                    }
                    catch (HttpRequestException e)
                    {
                        LLMResultText = "Error: " + e.Message;
                    }
                }

                return new List<TextContent>
            {
                new TextContent(LLMResultText)
            };
            }
        }
    }
}
