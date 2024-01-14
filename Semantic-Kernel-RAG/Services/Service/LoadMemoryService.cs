using BlingFire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Memory;
using Microsoft.KernelMemory;
using Azure.Core;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Embeddings;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Services;
using Microsoft.AspNetCore.Http;
using System.Text;
namespace Services;

public class LoadMemoryService : ILoadMemoryService
{
    private readonly IConfiguration _config;
    private readonly ILogger<LoadMemoryService> _logger;
    private SemanticTextMemory textMemory;
    public LoadMemoryService(IConfiguration config, ILogger<LoadMemoryService> logger)
    {
        _config = config;
        _logger = logger;
    }
    public async Task<string> ImportFile(string collection, params FileInfo[] textFile)
    {
        // Validate arguments.
        if (textFile.Length == 0)
        {
            _logger.LogError("No text files provided. Use '--help' for usage.");
            return "No File Found";
        }
        //HuggingFaceTextEmbeddingGenerationService embeddiingService = new HuggingFaceTextEmbeddingGenerationService(_config["Embedding:ModelName"], _config["Embedding:Endopint"]);
        //using custom sollution as package is not compatible with current json of the Hugging face api
        testHuggingFace embeddiingService = new testHuggingFace(_config["Embedding:ModelName"], _config["Embedding:Endopint"]);
        string memoryStringConnection = _config["Quadrant:memoryUrl"] ?? "";
        if (string.IsNullOrWhiteSpace(memoryStringConnection))
        {
            _logger.LogError("Please set the connection string of the memory");
            return "Keys not Found";
        }
        int vectorSize = int.Parse(_config["Quadrant:vectorSize"] ?? "1024");
        var memoryStore = new QdrantMemoryStore("http://semantickbot.centralindia.cloudapp.azure.com:6333", vectorSize);
        //Savety to make the Collection
        try
        {
            await memoryStore.CreateCollectionAsync(collection);
        }
        catch(Exception ex)
        {
            return ex.Message;
        }
        //test code for the Hugging face embeddings
        ////Store Kernel
        //List<string> test=new List<string>();
        //test.Add("test");
        //try { 
        //    var k = await embeddiingService.GenerateEmbeddingsAsync(test); }
        //catch(Exception e)
        //{
        //    return e.Message;
        //}
       
        textMemory = new (memoryStore,embeddiingService);
        await ImportMemoriesAsync(textMemory, collection, textFile);

        return "Import Done";
    }
    private async Task ImportMemoriesAsync(SemanticTextMemory kernel, string collection, params FileInfo[] textFile)
    {
        // Import the text files.
        int fileCount = 0;
        //Load Into the Memory
        foreach (FileInfo fileInfo in textFile)
        {
            // Read the text file.
            string text = File.ReadAllText(fileInfo.FullName);
            // Split the text into sentences.
            // Split the text into sentences.
            string[] sentences = BlingFireUtils.GetSentences(text).ToArray();

            // Save each sentence to the memory store.
            int sentenceCount = 0;
            foreach (string sentence in sentences)
            {
                ++sentenceCount;
                if (sentenceCount % 10 == 0)
                {
                    // Log progress every 10 sentences.
                    _logger.LogInformation($"[{fileCount}/{fileInfo.Length}] {fileInfo.FullName}: {sentenceCount}/{sentences.Length}");
                }

                try
                {
                    string id = Guid.NewGuid().ToString();
                    var x = await kernel.SaveInformationAsync(collection, id: id, text: sentence);
                }
                catch (Exception e)
                {
                    var k=e.Message;
                }
            }

        }
    }
    //public async Task<string> testKernelMmeory() {
    //    HuggingFaceTextEmbeddingGenerationService embeddiingService = new HuggingFaceTextEmbeddingGenerationService(_config["Embedding:ModelName"], _config["Embedding:Endopint"]);
    //    var memory = new KernelMemoryBuilder().WithCustomEmbeddingGenerator(embeddiingService).WithQdrantMemoryDb("").Build<MemoryServerless>(); ;
    //    memory.ImportDocumentAsync();

    //    return ""; }


}
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public class testHuggingFace : ITextEmbeddingGenerationService
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
{
    private readonly string _model;
    private readonly string? _endpoint;
    
    private readonly Dictionary<string, object?> _attributes = new();

    public testHuggingFace(Uri endpoint, string model)
    {

        this._model = model;
        this._endpoint = endpoint.AbsoluteUri;
        this._attributes.Add(AIServiceExtensions.ModelIdKey, this._model);
        this._attributes.Add(AIServiceExtensions.EndpointKey, this._endpoint);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HuggingFaceTextEmbeddingGenerationService"/> class.
    /// </summary>
    /// <param name="model">Model to use for service API call.</param>
    /// <param name="endpoint">Endpoint for service API call.</param>
    public testHuggingFace(string model, string endpoint)
    {
        this._model = model;
        this._endpoint = endpoint;
        this._attributes.Add(AIServiceExtensions.ModelIdKey, this._model);
        this._attributes.Add(AIServiceExtensions.EndpointKey, this._endpoint);
    }

    public IReadOnlyDictionary<string, object?> Attributes => this._attributes;

    public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> data, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        return await this.ExecuteEmbeddingRequestAsync(data, cancellationToken).ConfigureAwait(false);
    }
    private async Task<IList<ReadOnlyMemory<float>>> ExecuteEmbeddingRequestAsync(IList<string> data, CancellationToken cancellationToken)
    {
        var embeddingRequest = new TextEmbeddingRequest
        {
            Input = data
        };
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, this._endpoint+"/"+this._model);
        var content = new StringContent(JsonSerializer.Serialize(embeddingRequest), Encoding.UTF8, "application/json");
        request.Content = content;

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var responseStream = await response.Content.ReadAsStreamAsync();
        var embeddingResponse = await JsonSerializer.DeserializeAsync<List<float[]>>(responseStream);

        return embeddingResponse?.Select(l => new ReadOnlyMemory<float>(l)).ToList()!;
    }
}
