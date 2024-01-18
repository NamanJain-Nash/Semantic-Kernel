namespace Services;

public interface ILoadMemoryService
{
    public Task<string> ImportFileAsync(string collection, params FileInfo[] textFile);
}
