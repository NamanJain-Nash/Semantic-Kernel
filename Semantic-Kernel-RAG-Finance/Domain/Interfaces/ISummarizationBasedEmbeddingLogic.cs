namespace Buisness_Logic.Interfaces
{
    public interface ISummarizationBasedEmbeddingLogic
    {
        public Task<bool> DocumentToEmbedding(string collection, params FileInfo[] textFile);
    }
}