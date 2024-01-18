namespace Services.IService
{
    public interface IChatService
    {
        public Task<string> ChatWithLLMAsync(string query,string information);
    }
}