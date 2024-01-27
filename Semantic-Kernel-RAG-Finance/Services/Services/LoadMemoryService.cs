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
using Services.IService;
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
    public async Task<string> ImportFileAsync(string collection, FileInfo textFile)
    {
        // Validate arguments.
        if (textFile.Length == 0)
        {
            _logger.LogError("No text files provided. Use '--help' for usage.");
            return "No File Found";
        }
        //HuggingFaceTextEmbeddingGenerationService embeddiingService = new HuggingFaceTextEmbeddingGenerationService(_config["Embedding:ModelName"], _config["Embedding:Endopint"]);
        //using custom sollution as package is not compatible with current json of the Hugging face api
        CustomHuggingFaceTextEmbeddingService embeddiingService = new CustomHuggingFaceTextEmbeddingService(_config["Embedding:ModelName"], _config["Embedding:Endopint"]);
        string memoryStringConnection = _config["Qdrant:memoryUrl"] ?? "";
        if (string.IsNullOrWhiteSpace(memoryStringConnection))
        {
            _logger.LogError("Please set the connection string of the memory");
            return "Keys not Found";
        }
        int vectorSize = int.Parse(_config["Qdrant:vectorSize"] ?? "1024");
        var memoryStore = new QdrantMemoryStore(memoryStringConnection, vectorSize);
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
    private async Task ImportMemoriesAsync(SemanticTextMemory kernel, string collection, FileInfo textFile)
    {
            // Read the text file.
            string text = File.ReadAllText(textFile.FullName);
            // Split the text into sentences.
            // Split the text into sentences.
            string[] sentences = BlingFireUtils.GetSentences(text).ToArray();
            int id_start = 100;
            // Save each sentence to the memory store.
            int sentenceCount = 0;
            foreach (string sentence in sentences)
            {
                ++sentenceCount;
                if (sentenceCount % 10 == 0)
                {
                    // Log progress every 10 sentences.
                    _logger.LogInformation($"{textFile.FullName}: {sentenceCount}/{sentences.Length}");
                }

                try
                {
                    string id = id_start.ToString();
                    id_start++;
                    var x = await kernel.SaveInformationAsync(collection, id: id, text: sentence);
                }
                catch (Exception e)
                {
                    var k=e.Message;
                }
            }
    }
    //public async Task<string> testKernelMmeory() {
    //    HuggingFaceTextEmbeddingGenerationService embeddiingService = new HuggingFaceTextEmbeddingGenerationService(_config["Embedding:ModelName"], _config["Embedding:Endopint"]);
    //    var memory = new KernelMemoryBuilder().WithCustomEmbeddingGenerator(embeddiingService).WithQdrantMemoryDb("").Build<MemoryServerless>(); ;
    //    memory.ImportDocumentAsync();

    //    return ""; }


}
