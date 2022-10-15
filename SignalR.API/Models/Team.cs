namespace SignalR.API.Models;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; }
    public virtual ICollection<User> Users { get; set; }

    public Team()
    {
        Users = new List<User>();
    }
}