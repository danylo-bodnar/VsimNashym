using api.DTOs.Users;

namespace api.Utils
{
    public static class TelegramMessageBuilder
    {
        public static string BuildUserIntro(
            UserDto user,
            double? distanceKm)
        {
            var lines = new List<string>
            {
                $"👋 <b>{user.DisplayName}</b>, {user.Age}"
            };

            if (!string.IsNullOrWhiteSpace(user.Bio))
                lines.Add($"📝 {user.Bio}");

            if (user.Interests.Any())
                lines.Add($"✨ <b>Interests:</b> {string.Join(", ", user.Interests.Take(5))}");

            if (distanceKm.HasValue)
                lines.Add($"📍 <b>{Math.Round(distanceKm.Value)} km</b> away");

            lines.Add("");
            lines.Add("says hi 👀");

            return string.Join("\n", lines);
        }
    }
}
