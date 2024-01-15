using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Text;
using Services.IService;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Connectors.Qdrant;

namespace Services.Service
{
    public class SearchService : ISearchService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<LoadMemoryService> _logger;
        private readonly IKernelBuilder _kernel;

        public SearchService(IConfiguration config, ILogger<LoadMemoryService> logger)
        {
            _config = config;
            _logger = logger;
        }
        public async Task<string> SearchMemoriesAsync(string query, string collenctionName)
        {
            //building the search engine for the store
            HuggingFaceTextEmbeddingGenerationService embeddingService = new HuggingFaceTextEmbeddingGenerationService("BAAI/bge-large-en-v1.5", "http://0.0.0.0:8080");
            string memoryStringConnection = _config["Quadrant:memoryUrl"] ?? "";
            int VectorSize = int.Parse(_config["Quadrant:vectorSize"]??"1024");
            if (string.IsNullOrWhiteSpace(memoryStringConnection))
            {
                _logger.LogError("Please set the connection string of the memory");
                return "Keys not Found";
            }
            var memoryStore = new QdrantMemoryStore(memoryStringConnection, VectorSize);
            SemanticTextMemory textMemory = new(
                memoryStore,
                embeddingService);
            return await SearchInVectorAsync(textMemory, query, collenctionName);
        }
        private async Task<string> SearchInVectorAsync(SemanticTextMemory textMemory, string query, string collenctionName)
        {
            int searchLimit = int.Parse(_config["Search:Limit"]??"5");
            double MinRelevace = double.Parse(_config["Search:Relevance"]??"0.77");
            IAsyncEnumerable<MemoryQueryResult> queryResults =
            textMemory.SearchAsync(collenctionName, query, limit: searchLimit, minRelevanceScore: MinRelevace);
            StringBuilder result = new StringBuilder();
            result.Append("The below is relevant information.\n[START INFO]");
            // For each memory found, get previous and next memories.
            await foreach (MemoryQueryResult r in queryResults)
            {
                int id = int.Parse(r.Metadata.Id);
                MemoryQueryResult? rb2 = await textMemory.GetAsync(collenctionName, (id - 2).ToString());
                MemoryQueryResult? rb = await textMemory.GetAsync(collenctionName, (id - 1).ToString());
                MemoryQueryResult? ra = await textMemory.GetAsync(collenctionName, (id + 1).ToString());
                MemoryQueryResult? ra2 = await textMemory.GetAsync(collenctionName, (id + 2).ToString());

                if (rb2 != null) result.Append("\n " + rb2.Metadata.Id + ": " + rb2.Metadata.Description + "\n");
                if (rb != null) result.Append("\n " + rb.Metadata.Description + "\n");
                if (r != null) result.Append("\n " + r.Metadata.Description + "\n");
                if (ra != null) result.Append("\n " + ra.Metadata.Description + "\n");
                if (ra2 != null) result.Append("\n " + ra2.Metadata.Id + ": " + ra2.Metadata.Description + "\n");
            }
            if(result.ToString()== "The below is relevant information.\n[START INFO]")
            {
                return null;
            }
            result.Append("\n[END INFO]");
            result.Append($"\n{query}");

            _logger.LogInformation($"The Search for {query} Result is : \n" + result.ToString());
            return result.ToString();
        }

    }
}
