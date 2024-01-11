using Models;

namespace Buisness_Logic;

public interface IChatLogic
{
    public Task<ChatOutput> ChatResultWithMemory(ChatInput chatInput);

}
