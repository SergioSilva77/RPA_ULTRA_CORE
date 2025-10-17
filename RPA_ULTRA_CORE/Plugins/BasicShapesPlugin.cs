using System.Composition;
using RPA_ULTRA_CORE.Plugins.Abstractions;

namespace RPA_ULTRA_CORE.Plugins
{
    /// <summary>
    /// Plugin básico com formas geométricas
    /// </summary>
    [Export(typeof(IPlugin))]
    [Shared]
    public class BasicShapesPlugin : IPlugin
    {
        public string Id => "core.shapes";
        public string Name => "Basic Shapes";
        public string Version => "1.0.0";

        public IEnumerable<IInventorySection> GetSections()
        {
            yield return new BasicShapesSection();
        }
    }

    public class BasicShapesSection : IInventorySection
    {
        public string Id => "shapes.basic";
        public string Name => "Shapes";
        public string IconResource => "embedded:shapes_icon.png";
        public int DisplayOrder => 10;

        public IEnumerable<IInventoryItem> GetItems()
        {
            yield return new ShapeItem
            {
                ItemId = "shape.line",
                ItemName = "Line",
                ItemDescription = "Draw a line between two points",
                ItemIcon = "embedded:line_icon.png",
                ItemTags = new[] { "line", "shape", "draw" },
                ItemType = InventoryItemType.ShapeBlueprint,
                ShapeDefaults = new { Width = 2, Color = "White" }
            };

            yield return new ShapeItem
            {
                ItemId = "shape.rectangle",
                ItemName = "Rectangle",
                ItemDescription = "Draw a rectangle",
                ItemIcon = "embedded:rect_icon.png",
                ItemTags = new[] { "rectangle", "rect", "shape", "draw" },
                ItemType = InventoryItemType.ShapeBlueprint,
                ShapeDefaults = new { Width = 100, Height = 50, Color = "White" }
            };

            yield return new ShapeItem
            {
                ItemId = "shape.circle",
                ItemName = "Circle",
                ItemDescription = "Draw a circle",
                ItemIcon = "embedded:circle_icon.png",
                ItemTags = new[] { "circle", "ellipse", "shape", "draw" },
                ItemType = InventoryItemType.ShapeBlueprint,
                ShapeDefaults = new { Radius = 30, Color = "White" }
            };
        }

        private class ShapeItem : IInventoryItem
        {
            public string ItemId { get; init; } = "";
            public string ItemName { get; init; } = "";
            public string ItemDescription { get; init; } = "";
            public string ItemIcon { get; init; } = "";
            public string[] ItemTags { get; init; } = Array.Empty<string>();
            public InventoryItemType ItemType { get; init; }
            public object? ShapeDefaults { get; init; }

            public string Id => ItemId;
            public string Name => ItemName;
            public string Description => ItemDescription;
            public string IconResource => ItemIcon;
            public string[] Tags => ItemTags;
            public InventoryItemType Type => ItemType;
            public object? Defaults => ShapeDefaults;
            public Action<CanvasContext>? Action => null;
        }
    }
}