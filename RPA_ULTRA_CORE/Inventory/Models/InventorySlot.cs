using RPA_ULTRA_CORE.Plugins.Abstractions;
using SkiaSharp;

namespace RPA_ULTRA_CORE.Inventory.Models
{
    /// <summary>
    /// Representa um slot no inventário ou hotbar
    /// </summary>
    public class InventorySlot
    {
        public int Index { get; set; }
        public IInventoryItem? Item { get; set; }
        public SKRect Bounds { get; set; }
        public bool IsHovered { get; set; }
        public bool IsSelected { get; set; }
        public bool IsHotbarSlot { get; set; }

        /// <summary>
        /// Tecla de atalho (1-7 para hotbar)
        /// </summary>
        public int? HotkeyNumber => IsHotbarSlot ? (Index + 1) : null;

        public bool IsEmpty => Item == null;

        public bool HitTest(SKPoint point)
        {
            return Bounds.Contains(point);
        }
    }

    /// <summary>
    /// Estado visual de um slot
    /// </summary>
    public enum SlotState
    {
        Normal,
        Hovered,
        Selected,
        Dragging
    }
}