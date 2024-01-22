namespace Services.IService
{
    public interface ISearchService
    {
        public Task<string> SearchMemoriesAsync(string query, string collenctionName);
    }
}