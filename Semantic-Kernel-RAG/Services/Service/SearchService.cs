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
            testHuggingFace embeddingService = new testHuggingFace(_config["Embedding:ModelName"], _config["Embedding:Endopint"]);
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
            //Initialize the Search engine with Parameters
            int searchLimit = int.Parse(_config["Search:Limit"]??"5");
            double MinRelevace = double.Parse(_config["Search:Relevance"]??"0.77");
            IAsyncEnumerable<MemoryQueryResult> queryResults =
            textMemory.SearchAsync(collenctionName, query, limit: searchLimit, minRelevanceScore: MinRelevace);
            
            //Building The Searched Result with Releveant Info.
            StringBuilder result = new StringBuilder();
            result.Append("[START INFO] \n ");
            StringBuilder SummarizeText=new StringBuilder();
            // For each memory found, get previous and next memories.
            await foreach (MemoryQueryResult r in queryResults)
            {
                StringBuilder paraText=new StringBuilder();
                int id = int.Parse(r.Metadata.Id);
                MemoryQueryResult? rb2 = await textMemory.GetAsync(collenctionName, (id - 2).ToString());
                MemoryQueryResult? rb = await textMemory.GetAsync(collenctionName, (id - 1).ToString());
                MemoryQueryResult? ra = await textMemory.GetAsync(collenctionName, (id + 1).ToString());
                MemoryQueryResult? ra2 = await textMemory.GetAsync(collenctionName, (id + 2).ToString());

                if (rb2 != null) paraText.Append("\n " + rb2.Metadata.Text + "\t");
                if (rb != null) paraText.Append(rb.Metadata.Text + "\t");
                if (r != null) paraText.Append(r.Metadata.Text + "\t");
                if (ra != null) paraText.Append(ra.Metadata.Text + "\t");
                if (ra2 != null) paraText.Append(ra2.Metadata.Text + "\t");
                SummarizeText.Append(paraText+"\n");
            }
            //We have to Shorterner Up the Text to fit to the model too if the Text Length is Falling
            result.Append(SummarizeText);
            if(result.ToString()=="[START INFO] \n ")
            {
                return "";
            }
            result.Append("\n[END INFO]");

            _logger.LogInformation($"The Search for {query} Result is : \n" + result.ToString());
            return result.ToString();
        }
        private async Task<string> SummarizeParagraphText(StringBuilder builder){
            return "";
        }
    }
}
