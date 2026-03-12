namespace AgenticAISql.Models;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string City { get; set; } = default!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}