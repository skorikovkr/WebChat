namespace WebChatTest.Hubs
{
    public interface IChatHub
    {
        Task Notify(string message);
        Task RecieveMessage(string username, string message);
    }
}