namespace MojeApiMariaDB.Models;

public class Location
{
    public int Id { get; set; }
    public string ShortName { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
}