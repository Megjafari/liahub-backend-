using System.Text.Json.Serialization;

namespace LIAhub.Core.Models;
public class UserTechStack
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Tech { get; set; } = string.Empty;

    [JsonIgnore]
    public User User { get; set; } = null!;
}