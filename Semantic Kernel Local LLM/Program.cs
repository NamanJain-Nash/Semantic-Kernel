using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextGeneration;
namespace Semantic_Kernel;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Testing Tiny Lama");
        Console.WriteLine("\n======== Custom LLM - Text Completion - KernelFunction ========");
        //Intitalizing The Kernel
        IKernelBuilder builder = Kernel.CreateBuilder();
        // Add your text generation service as a singleton instance of the Kernel
        builder.Services.AddKeyedSingleton<ITextGenerationService>("myService1", new MyTextGenerationService());
        // Add your text generation service as a factory method
        builder.Services.AddKeyedSingleton<ITextGenerationService>("myService2", (_, _) => new MyTextGenerationService());
        //Build the Kernel
        Kernel kernel = builder.Build();
        //Define The Prompt
        const string FunctionDefinition = "Write Step By Step Answer on {{$input}}";
        //Function Defined
        var paragraphWritingFunction = kernel.CreateFunctionFromPrompt(FunctionDefinition);
        //Question Asked
        const string Input = "What is the Capital of France";
        Console.WriteLine($"Function input: {Input}\n");
        //Run the Prompt
        try{
        var result =  await paragraphWritingFunction.InvokeAsync(kernel, new() { ["input"] = Input });
        Console.WriteLine(result);}
        catch(Exception e){
            Console.WriteLine(e.Message);}
        
    }
    private sealed class MyTextGenerationService : ITextGenerationService
    {
        public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();
        public async IAsyncEnumerable<StreamingTextContent> GetStreamingTextContentsAsync(string prompt, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            string LLMResultText="Not implemented";
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
            string apiUrl = "http://localhost:1234/v1/chat/completions";
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
                        LLMResultText = responseObject.choices[0].message.content;;
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
