using Microsoft.AspNetCore.SignalR;

namespace SignalR.API.Hubs;

public class MyHub: Hub
{
    private static List<string> Names { get; set; } = new List<string>();
    public async Task SendName(string name)
    {
        Names.Add(name);
        await Clients.All.SendAsync("ReceiveName",name);
    }

    public async Task GetNames()
    {
        await Clients.All.SendAsync("ReceiveName", Names);
    }
}