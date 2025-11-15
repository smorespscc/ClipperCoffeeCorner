using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ClipperCoffeeCorner.Dtos.Menu
{
    /// <summary>Root object returned by GET /menu</summary>
    public sealed class MenuResponse
    {
        /// <summary>Current service window (e.g., Breakfast 08:00–10:30)</summary>
        [JsonPropertyName("currentServiceWindow")]
        public ServiceWindow CurrentServiceWindow { get; set; } = new();

        /// <summary>Common size labels shown in UI order</summary>
        [JsonPropertyName("sizeSchema")]
        public SizeSchema SizeSchema { get; set; } = new();

        /// <summary>Menu categories (Brewed Coffee, Espresso, etc.)</summary>
        [JsonPropertyName("categories")]
        public List<MenuCategory> Categories { get; set; } = new();

        /// <summary>Global drink modifiers (style/milk/additions/flavors)</summary>
        [JsonPropertyName("drinkModifiers")]
        public DrinkModifiers DrinkModifiers { get; set; } = new();
    }

    public sealed class ServiceWindow
    {
        /// <summary>Human label (e.g., Breakfast)</summary>
        [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;

        /// <summary>Start time HH:mm (local)</summary>
        [JsonPropertyName("start")] public string Start { get; set; } = "08:00";

        /// <summary>End time HH:mm (local)</summary>
        [JsonPropertyName("end")] public string End { get; set; } = "10:30";

        /// <summary>Optional note for UI</summary>
        [JsonPropertyName("note")] public string? Note { get; set; }
    }

    public sealed class SizeSchema
    {
        /// <summary>Keys must match item price keys (e.g., 12oz, 16oz, 20oz/24oz)</summary>
        [JsonPropertyName("drinkSizes")] public List<string> DrinkSizes { get; set; } = new();
        /// <summary>Optional size hints</summary>
        [JsonPropertyName("notes")] public string? Notes { get; set; }
    }

    public sealed class MenuCategory
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

        /// <summary>Items in this category</summary>
        [JsonPropertyName("items")] public List<MenuItem> Items { get; set; } = new();
    }

    public sealed class MenuItem
    {
        /// <summary>Stable id (e.g., mocha)</summary>
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;

        /// <summary>Display name</summary>
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

        /// <summary>Price per size key (must match SizeSchema.DrinkSizes)</summary>
        [JsonPropertyName("prices")] public Dictionary<string, decimal> Prices { get; set; } = new();

        /// <summary>Whether item is currently available</summary>
        [JsonPropertyName("isAvailable")] public bool IsAvailable { get; set; } = true;

        /// <summary>Optional style limits (e.g., hot-only, iced-only)</summary>
        [JsonPropertyName("restrictions")] public List<string>? Restrictions { get; set; }

        /// <summary>Optional note line</summary>
        [JsonPropertyName("notes")] public string? Notes { get; set; }
    }

    public sealed class DrinkModifiers
    {
        /// <summary>Style options (Hot/Iced/Blended)</summary>
        [JsonPropertyName("style")] public List<SimpleOption> Style { get; set; } = new();

        /// <summary>Milk options with optional price delta</summary>
        [JsonPropertyName("milk")] public List<PricedOption> Milk { get; set; } = new();

        /// <summary>Add-ons (extra shot, add flavor) with deltas</summary>
        [JsonPropertyName("additions")] public List<PricedOption> Additions { get; set; } = new();

        /// <summary>Flavor libraries</summary>
        [JsonPropertyName("flavors")] public FlavorGroups Flavors { get; set; } = new();
    }

    public class SimpleOption
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;
    }

    public sealed class PricedOption : SimpleOption
    {
        /// <summary>Price delta when selected (can be 0)</summary>
        [JsonPropertyName("delta")] public decimal? Delta { get; set; }
    }

    public sealed class FlavorGroups
    {
        [JsonPropertyName("regularSyrups")] public List<string> RegularSyrups { get; set; } = new();
        [JsonPropertyName("sugarFreeSyrups")] public List<string> SugarFreeSyrups { get; set; } = new();
        [JsonPropertyName("sauces")] public List<string> Sauces { get; set; } = new();
        [JsonPropertyName("sugarFreeSauces")] public List<string> SugarFreeSauces { get; set; } = new();
        [JsonPropertyName("limitedTime")] public List<string> LimitedTime { get; set; } = new();
    }
}
