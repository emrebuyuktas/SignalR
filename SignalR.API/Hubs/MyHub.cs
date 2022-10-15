using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalR.API.Models;

namespace SignalR.API.Hubs;

public class MyHub: Hub
{
    private static List<string> Names { get; set; } = new List<string>();
    private static int clienttCount = 0;
    public static int teamCount = 3;
    private readonly AppDbContext _context;

    public MyHub(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task SendProduct(Product p)
    {
        await Clients.All.SendAsync("ReceiveProduct", p);
    }

    public async Task SendName(string name)
    {
        if (Names.Count >= teamCount)
        {
            await Clients.Caller.SendAsync("Error", $"Team count is too big, {teamCount} is limit");
        }
        else
        {
            Names.Add(name);
            await Clients.All.SendAsync("ReceiveName",name);
        }
        
        
    }

    public async Task GetNames()
    {
        await Clients.All.SendAsync("ReceiveNames", Names);
    }

    public async Task AddToGroup(string teamName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, teamName);
    }

    public async Task SendNameByGroup(string name, string teamName)
    {
        var team = await _context.Teams.Where(x => x.Name == teamName).FirstOrDefaultAsync();
        if (team is not null)
        {
            team.Users.Add(new User{Name = name});
        }
        else
        {
            var newTeam = new Team
            {
                Name = teamName
            };
            newTeam.Users.Add(new User{Name = name});
            _context.Teams.Add(newTeam);
        }

        await _context.SaveChangesAsync();

        await Clients.Group(teamName).SendAsync("ReceiveMessageByGroup", name,team.Id);
    }

    public async Task GetNamesByGroup()
    {
        var teams = _context.Teams.Include(x => x.Users)
            .Select(x=>new
            {
                TeamId=x.Id,
                Users=x.Users.ToList()
                
            }).ToList();

        await Clients.All.SendAsync("ReceiveNamesByGroup", teams);
    }

    public async Task RemoveFromGroup(string teamName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, teamName);
    }

    public override async Task OnConnectedAsync()
    {
        clienttCount++;
        await Clients.All.SendAsync("ReceiveClientCount", clienttCount);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        clienttCount--;
        await Clients.All.SendAsync("ReceiveClientCount", clienttCount);
        await base.OnDisconnectedAsync(exception);
    }
}