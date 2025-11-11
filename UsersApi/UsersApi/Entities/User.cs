namespace UsersApi.Entities;

public class User
{
    public int Id { get; set; }           // PK
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public string Email { get; set; } = "";
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
}