using BlingFire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Quadrant;
namespace Services;

public class LoadMemoryService:ILoadMemoryService
{
    private readonly IConfiguration _config;
        private readonly ILogger<LoadMemoryService> _logger;

        public LoadMemoryService(IConfiguration config, ILogger<LoadMemoryService> logger)
        {
            _config = config;
            _logger = logger;
        }
    public async Task<string> ImportFile ( string collection, params FileInfo[] textFile)
    {
        // Validate arguments.
        if (textFile.Length == 0)
        {
            _logger.LogError("No text files provided. Use '--help' for usage.");
            return "No File Found";
        }
        IKernel kernel;
        // Get the OpenAI API key from the configuration.
        string openAiApiKey = _config["OPENAI_APIKEY"]??"";
        string memoryString=_config["Quadrant:memory"]??"";
        if (string.IsNullOrWhiteSpace(openAiApiKey) || string.IsNullOrWhiteSpace(memoryString))
        {
        _logger.LogError("Please set the 'OPENAI_APIKEY' user secret with your OpenAI API key.");
            return "Keys not Found";
        }
        // Create a new memory store that will store the embeddings in Qdrant.
        Uri qdrantUri = new Uri(_config["Quadrant:Uri"]);
        QdrantMemoryStore memoryStore = new QdrantMemoryStore(
            host: $"{qdrantUri.Scheme}://{qdrantUri.Host}",
            port: qdrantUri.Port,
            vectorSize: 1536);

//             kernel = sk.Kernel()

// # Configure LLM service
// kernel.config.add_text_completion_service(
//     "gpt2", sk_hf.HuggingFaceTextCompletion("gpt2", task="text-generation")
// )
// kernel.config.add_text_embedding_generation_service(
//     "sentence-transformers/all-MiniLM-L6-v2",
//     sk_hf.HuggingFaceTextEmbedding("sentence-transformers/all-MiniLM-L6-v2"),
// )
// kernel.register_memory_store(memory_store=sk.memory.VolatileMemoryStore())
// kernel.import_skill(sk.core_skills.TextMemorySkill())
        // Create a new kernel with an OpenAI Embedding Generation service.
        kernel = new KernelBuilder()
            .Configure(c => c.AddOpenAITextEmbeddingGenerationService(
                modelId: "text-embedding-ada-002",
                apiKey: openAiApiKey))
            .WithMemoryStorage(memoryStore)
            .Build();
        await ImportMemoriesAsync(kernel, collection, textFile);
        return "Import Done";
    }
    private async Task ImportMemoriesAsync(IKernel kernel, string collection, params FileInfo[] textFile)
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