using BlingFire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Memory;
using Microsoft.KernelMemory;
using Azure.Core;
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
        HuggingFaceTextEmbeddingGenerationService embeddiingService = new HuggingFaceTextEmbeddingGenerationService(_config["Embedding:ModelName"], _config["Embedding:Endopint"]);
        string memoryStringConnection = _config["Quadrant:memoryUrl"] ?? "";
        if (string.IsNullOrWhiteSpace(memoryStringConnection))
        {
            _logger.LogError("Please set the connection string of the memory");
            return "Keys not Found";
        }
        int vectorSize = int.Parse(_config["Quadrant:vectorSize"] ?? "0");
        var memoryStore = new QdrantMemoryStore(memoryStringConnection, vectorSize);
        //Store Kernel
        List<string> test=new List<string>();
        test.Add("test");
        var k=await embeddiingService.GenerateEmbeddingsAsync(test);
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
                catch { }
            }

    }
    }
    //public async Task<string> testKernelMmeory() {
    //    HuggingFaceTextEmbeddingGenerationService embeddiingService = new HuggingFaceTextEmbeddingGenerationService(_config["Embedding:ModelName"], _config["Embedding:Endopint"]);
    //    var memory = new KernelMemoryBuilder().WithCustomEmbeddingGenerator(embeddiingService).WithQdrantMemoryDb("").Build<MemoryServerless>(); ;
    //    memory.ImportDocumentAsync();

    //    return ""; }

}