using Buisness_Logic;
using Models.Chat;

namespace ChatAPI;

public class ChatHandler
{
    private readonly IChatLogic _chatLogic;

    public ChatHandler(IChatLogic chatLogic)
    {
        _chatLogic = chatLogic;
    }
    public async Task<ChatOutput> HandleChat(ChatInput chats){
        ChatOutput result=await _chatLogic.ChatResultWithMemory(chats);
        return result;
    }

  
}
