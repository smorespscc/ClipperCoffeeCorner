using System.Text.Json.Serialization;

namespace ClipperCoffeeCorner.Dtos.Queue
{
    /// <summary>Lightweight queue position</summary>
    public sealed class QueuePositionDto
    {
        [JsonPropertyName("orderId")] public string OrderId { get; set; } = string.Empty;
        [JsonPropertyName("position")] public int Position { get; set; }
        /// <summary>ISO 8601 UTC time string</summary>
        [JsonPropertyName("estimatedReadyTime")] public string? EstimatedReadyTime { get; set; }
        [JsonPropertyName("status")] public string Status { get; set; } = "InQueue";
    }

    /// <summary>Current order in progress</summary>
    public sealed class CurrentQueueItemDto
    {
        [JsonPropertyName("orderId")] public string OrderId { get; set; } = string.Empty;
        [JsonPropertyName("position")] public int Position { get; set; }
        [JsonPropertyName("status")] public string Status { get; set; } = "InProgress";
    }

    /// <summary>Queue status used inside other responses</summary>
    public sealed class QueueStatusDto
    {
        [JsonPropertyName("orderId")] public string OrderId { get; set; } = string.Empty;
        [JsonPropertyName("position")] public int Position { get; set; }
        [JsonPropertyName("status")] public string Status { get; set; } = "InQueue";
        [JsonPropertyName("estimatedReadyTime")] public string? EstimatedReadyTime { get; set; }
    }
}
