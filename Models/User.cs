
// Temp model just so the project builds. 
// I think these are the right names for the fields tho

namespace ClipperCoffeeCorner.Models
{
    public class User
    {
        public Guid UserId { get; set; } = Guid.NewGuid();

        public required string Username { get; set; }

        public required string UserRole { get; set; }

        public NotificationPreference NotificationPref { get; set; }  // 0 = none, 1 = email, 2 = SMS, etc.

        public string? NotificationEmail { get; set; }

        public string? PhoneNumber { get; set; }

        public DateTimeOffset? LastLogin { get; set; }

        public DateTimeOffset AccountCreated { get; set; } = DateTimeOffset.UtcNow;
    }

    [Flags]
    public enum NotificationPreference : byte
    {
        None = 0,
        Sms = 1,
        Email = 2,
    }
}
