using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1;

public class ChatHub : Hub
{
    private AppDb _appDb;

    public ChatHub(AppDb appDb)
    {
        _appDb = appDb;
    }

    public async Task NewMessage(string userId, string content)
    {
        var foundUser = await _appDb.Users.Where(u => u.Id == userId).FirstAsync();
        await Clients.All.SendAsync("chatMessageReceived", Guid.NewGuid().ToString(), foundUser.Email, content);
    }
}