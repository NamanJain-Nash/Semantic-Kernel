using System.Net.Http.Headers;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Models.Chat;
using Services;
using Services.IService;

namespace Domain;

public class ChatLogic : IChatLogic
{
    private readonly ISearchService _searchService;
    private readonly IChatService _chatService;
    private readonly ILogger<ChatLogic> _logger;

    public ChatLogic(ISearchService searchService, IChatService chatService,ILogger<ChatLogic> logger)
    {
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
        _logger = logger ?? throw new ArgumentNullException();
    }

    public async Task<ChatOutput> ChatResultWithMemory(ChatInput chatInput)
    {
        ChatOutput result = new ChatOutput
        {
            ChatId = chatInput.ChatId,
            UserQuery = chatInput.UserQuery,
            AiAnswer = ""
        };

        string chatQuery = chatInput.UserQuery;

        try
        {
            // Getting Query With Memory
            string systemMemory = await _searchService.SearchMemoriesAsync(chatInput.UserQuery, chatInput.CollectionName);
            _logger.LogError("test:" + systemMemory);

            if (string.IsNullOrEmpty(systemMemory) || systemMemory == "Keys not Found")
            {
                systemMemory = "Use General Information Only";
            }
            //Requesting the LLM
            string aiResponse = await _chatService.ChatWithLLMAsync(chatQuery, systemMemory);

            // Fitting into the output
            result.AiAnswer = aiResponse;
        }
        catch (Exception ex)
        {
            // Log or handle the exception as per your application's needs
            _logger.LogError($"An error occurred: {ex.Message}");
            result.AiAnswer = "An error occurred while processing the request.";
        }

        return result;
    }
}
