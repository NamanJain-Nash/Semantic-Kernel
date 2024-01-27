namespace Services.IService;

public interface ILoadMemoryService
{
    public Task<string> ImportFileAsync(string collection, FileInfo textFile);
}
