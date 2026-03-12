using System.ComponentModel.DataAnnotations;

namespace AngularDotNetChat.ApiService.Models;

/// <summary>Represents an application user.</summary>
public class User
{
    [Key]
    public int UserId { get; set; }

    [Required, MaxLength(256)]
    public string UserEmail { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public ICollection<Document> Documents { get; set; } = [];
}
