using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClipperCoffeeCorner.Extensions
{
    public static class SessionExtensions
    {
        // Store an object as JSON in session (ignores reference cycles)
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            if (session == null) return;
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            var json = JsonSerializer.Serialize(value, options);
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
                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                return JsonSerializer.Deserialize<T>(json, options);
            }
            catch
            {
                return default;
            }
        }
    }
}
