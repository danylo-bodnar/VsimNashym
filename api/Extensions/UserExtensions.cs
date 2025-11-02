
public static class UserExtensions
{
    public static string GetLastActiveLabel(this DateTime lastActive)
    {
        var now = DateTime.UtcNow;
        var difference = now - lastActive;

        if (difference.TotalSeconds < 0) return "In the future";

        var days = (int)Math.Floor(difference.TotalDays);

        return days switch
        {
            0 => "Today",
            1 => "Yesterday",
            <= 6 => $"{days} days ago",
            <= 30 => $"{days / 7} week{(days / 7 > 1 ? "s" : "")} ago",
            _ => "Long time ago"
        };
    }
}
