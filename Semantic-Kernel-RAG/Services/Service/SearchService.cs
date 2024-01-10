using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Text;
using Services.IService;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;

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

            IAsyncEnumerable<MemoryQueryResult> queryResults =
                _kernel.Memory.SearchAsync(collenctionName, query, limit: 5, minRelevanceScore: 0.77);

            // For each memory found, get previous and next memories.
            await foreach (MemoryQueryResult r in queryResults)
            {
                int id = int.Parse(r.Metadata.Id);
                MemoryQueryResult? rb2 = await _kernel.Memory.GetAsync(memoryCollectionName, (id - 2).ToString());
                MemoryQueryResult? rb = await _kernel.Memory.GetAsync(memoryCollectionName, (id - 1).ToString());
                MemoryQueryResult? ra = await _kernel.Memory.GetAsync(memoryCollectionName, (id + 1).ToString());
                MemoryQueryResult? ra2 = await _kernel.Memory.GetAsync(memoryCollectionName, (id + 2).ToString());

                if (rb2 != null) result.Append("\n " + rb2.Metadata.Id + ": " + rb2.Metadata.Description + "\n");
                if (rb != null) result.Append("\n " + rb.Metadata.Description + "\n");
                if (r != null) result.Append("\n " + r.Metadata.Description + "\n");
                if (ra != null) result.Append("\n " + ra.Metadata.Description + "\n");
                if (ra2 != null) result.Append("\n " + ra2.Metadata.Id + ": " + ra2.Metadata.Description + "\n");
            }

            result.Append("\n[END INFO]");
            result.Append($"\n{query}");

            _logger.LogInfo($"The Search for {query} Result is : \n"+result.ToString());
            return result.ToString();
        }

    }
}
