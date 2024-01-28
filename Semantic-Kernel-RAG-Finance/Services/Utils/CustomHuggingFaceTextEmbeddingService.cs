using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Embeddings;
using System.Text.Json;
using Microsoft.SemanticKernel.Services;
using System.Text;

namespace Services;

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public class CustomHuggingFaceTextEmbeddingService : ITextEmbeddingGenerationService
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
{
    private readonly string _model;
    private readonly string? _endpoint;
    
    private readonly Dictionary<string, object?> _attributes = new();

    public CustomHuggingFaceTextEmbeddingService(Uri endpoint, string model)
    {

        this._model = model;
        this._endpoint = endpoint.AbsoluteUri;
        this._attributes.Add(AIServiceExtensions.ModelIdKey, this._model);
        this._attributes.Add(AIServiceExtensions.EndpointKey, this._endpoint);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HuggingFaceTextEmbeddingGenerationService"/> class.
    /// </summary>
    /// <param name="model">Model to use for service API call.</param>
    /// <param name="endpoint">Endpoint for service API call.</param>
    public CustomHuggingFaceTextEmbeddingService(string model, string endpoint)
    {
        this._model = model;
        this._endpoint = endpoint;
        this._attributes.Add(AIServiceExtensions.ModelIdKey, this._model);
        this._attributes.Add(AIServiceExtensions.EndpointKey, this._endpoint);
    }

    public IReadOnlyDictionary<string, object?> Attributes => this._attributes;

    public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> data, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        return await this.ExecuteEmbeddingRequestAsync(data, cancellationToken).ConfigureAwait(false);
    }
    private async Task<IList<ReadOnlyMemory<float>>> ExecuteEmbeddingRequestAsync(IList<string> data, CancellationToken cancellationToken)
    {
        var embeddingRequest = new TextEmbeddingRequest
        {
            Input = data
        };
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, this._endpoint+"/"+this._model);
        var content = new StringContent(JsonSerializer.Serialize(embeddingRequest), Encoding.UTF8, "application/json");
        request.Content = content;

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var responseStream = await response.Content.ReadAsStreamAsync();
        var embeddingResponse = await JsonSerializer.DeserializeAsync<List<float[]>>(responseStream);

        return embeddingResponse?.Select(l => new ReadOnlyMemory<float>(l)).ToList()!;
    }
}
