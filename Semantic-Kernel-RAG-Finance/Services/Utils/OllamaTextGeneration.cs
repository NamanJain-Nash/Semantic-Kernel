using System.Runtime.CompilerServices;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextGeneration;
using Models.LLM;
using Newtonsoft.Json;

namespace Services;

public class OllamaTextGeneration : ITextGenerationService
{
    private readonly string _apiUrl;
    private readonly int _maxtoken;
    private readonly double _temprature;
    private readonly string _model;
    public OllamaTextGeneration(string apiUrl, int maxtoken, double temperature, string model)
    {
        _apiUrl = apiUrl;
        _model = model;
        _maxtoken = maxtoken;
        _temprature = temperature;
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
        Console.WriteLine(prompt);
        // Create an instance of the ChatRequest class
        var chatRequest = new OllamaRequest
        {
            model = _model,
            temperature = _temprature,
            num_predict = _maxtoken,
            prompt = prompt,
            stream = false
        };
        // Serialize the ChatRequest object to JSON
        string jsonPayload = JsonConvert.SerializeObject(chatRequest, Formatting.Indented);

        // Create an HttpClient instance
        using (HttpClient client = new HttpClient())
        {
            try
            {
                // Create HttpRequestMessage and set the Content-Type header
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _apiUrl);
                request.Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

                // Send the request
                HttpResponseMessage response = await client.SendAsync(request, cancellationToken);

                // Check if the request was successful (status code 200-299)
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic responseObject = JsonConvert.DeserializeObject(responseBody);
                    LLMResultText = responseObject.response;
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

