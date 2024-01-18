using Models.Chat;

namespace Domain.Interfaces;

public interface IChatLogic
{
    public Task<ChatOutput> ChatResultWithMemory(ChatInput chatInput);

}
