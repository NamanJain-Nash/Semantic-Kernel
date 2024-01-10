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
    public class SearchService:ISearchService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<LoadMemoryService> _logger;
        private readonly IKernelBuilder _kernel;

        public SearchService(IConfiguration config, ILogger<LoadMemoryService> logger, IKernelBuilder kernel)
        {
            _config = config;
            _logger = logger;
            _kernel = kernel;
        }
        public async Task<string> SearchMemoriesAsync(string query,string collenctionName)
        {

            StringBuilder result = new StringBuilder();
            result.Append("The below is relevant information.\n[START INFO]");
            //building the search engine for the store
#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            HuggingFaceTextEmbeddingGenerationService embeddingService = new HuggingFaceTextEmbeddingGenerationService("BAAI/bge-large-en-v1.5", "http://0.0.0.0:8080");
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

#pragma warning disable SKEXP0003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            SemanticTextMemory textMemory = new(
                memoryStore,
                embeddingService);
#pragma warning restore SKEXP0003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

#pragma warning disable SKEXP0003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            IAsyncEnumerable<MemoryQueryResult> queryResults =
                textMemory.SearchAsync(collenctionName, query, limit: 5, minRelevanceScore: 0.77);
#pragma warning restore SKEXP0003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

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

            result.Append("\n[END INFO]");
            result.Append($"\n{query}");

            _logger.LogInformation($"The Search for {query} Result is : \n"+result.ToString());
            return result.ToString();
        }

    }
}
