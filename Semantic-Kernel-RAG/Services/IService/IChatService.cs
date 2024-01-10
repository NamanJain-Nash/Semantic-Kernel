namespace Services.IService
{
    public interface IChatService
    {
        public Task<string> ChattingWithLLM(string query);
    }
}