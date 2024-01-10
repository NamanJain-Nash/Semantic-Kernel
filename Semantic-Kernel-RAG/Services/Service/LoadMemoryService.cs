using BlingFire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
namespace Services;

public class LoadMemoryService : ILoadMemoryService
{
    private readonly IConfiguration _config;
    private readonly ILogger<LoadMemoryService> _logger;

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
#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        HuggingFaceTextEmbeddingGenerationService embeddiingService = new HuggingFaceTextEmbeddingGenerationService("BAAI/bge-large-en-v1.5", "http://0.0.0.0:8080");
#pragma warning restore SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        string memoryStringConnection = _config["Quadrant:memory"] ?? "";
        if (string.IsNullOrWhiteSpace(memoryStringConnection))
        {
            _logger.LogError("Please set the connection string of the memory");
            return "Keys not Found";
        }
#pragma warning disable SKEXP0026 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        var memoryStore = new QdrantMemoryStore(memoryStringConnection, 1536);
#pragma warning restore SKEXP0026 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        await ImportMemoriesAsync(embeddiingService, collection, textFile);
        return "Import Done";
    }
    private async Task ImportMemoriesAsync(dynamic kernel, string collection, params FileInfo[] textFile)
    {
        // Use sequential memory IDs; this makes it easier to retrieve sentences near a given sentence.
        int memoryId = 0;
        // Import the text files.
        int fileCount = 0;
        //Load Into the Memory
        foreach (FileInfo fileInfo in textFile)
        {
            // Read the text file.
            string text = File.ReadAllText(fileInfo.FullName);
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
                }

                await kernel.Memory.SaveInformationAsync(
                    collection: collection,
                    text: sentence,
                    id: memoryId++.ToString(),
                    description: sentence);
            }
        }
    }


}