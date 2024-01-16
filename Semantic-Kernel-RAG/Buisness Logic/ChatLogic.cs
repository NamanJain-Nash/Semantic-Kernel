﻿using System.Net.Http.Headers;
using Models.Chat;
using Services;
using Services.IService;

namespace Buisness_Logic;

public class ChatLogic:IChatLogic
{
    private readonly ISearchService _searchService;
    private readonly IChatService _chatService;
    public ChatLogic(ISearchService searchService, IChatService chatService)
    {
        _searchService = searchService;
        _chatService = chatService;
    }
    public async Task<ChatOutput> ChatResultWithMemory(ChatInput chatInput)
    {
        ChatOutput result = new ChatOutput() { ChatId = chatInput.ChatId, UserQuery = chatInput.UserQuery, AiAnswer = "" };
        string chatQuery = chatInput.UserQuery;
        //Getting Query With Memory
        string ragSystemMemory = await _searchService.SearchMemoriesAsync(chatInput.UserQuery,chatInput.CollectionName);
        Console.WriteLine("test:"+ragSystemMemory);
        if (ragSystemMemory != "" && ragSystemMemory!="Keys not Found") {
        chatQuery = $@"Question:{chatQuery}

Context: {ragSystemMemory}
";       
        }
        result.AiAnswer = await _chatService.ChattingWithLLM(chatQuery);
        return result;

    }

}
