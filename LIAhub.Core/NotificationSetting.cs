public class NotificationSetting
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Frequency { get; set; } = "daily"; // instant, daily, weekly
    public bool Active { get; set; } = true;

    public User User { get; set; } = null!;
}