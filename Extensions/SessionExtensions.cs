using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace ClipperCoffeeCorner.Extensions
{
    public static class SessionExtensions
    {
        // Store an object as JSON in session
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            if (session == null) return;
            var json = JsonSerializer.Serialize(value);
            session.SetString(key, json);
        }

        // Retrieve an object from JSON stored in session; returns null if not present or on failure
        public static T? GetObject<T>(this ISession session, string key)
        {
            if (session == null) return default;
            var json = session.GetString(key);
            if (string.IsNullOrEmpty(json)) return default;
            try
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            catch
            {
                return default;
            }
        }
    }
}
