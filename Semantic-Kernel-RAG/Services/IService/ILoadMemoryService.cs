namespace Services;

public interface ILoadMemoryService
{
    public Task<string> ImportFile(string collection, params FileInfo[] textFile);
}
