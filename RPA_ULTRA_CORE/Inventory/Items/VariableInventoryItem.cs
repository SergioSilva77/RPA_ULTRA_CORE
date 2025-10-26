using RPA_ULTRA_CORE.Models.Geometry;
using RPA_ULTRA_CORE.Plugins.Abstractions;
using SkiaSharp;

namespace RPA_ULTRA_CORE.Inventory.Items
{
    /// <summary>
    /// Item de inventário para criar variáveis no canvas
    /// </summary>
    public class VariableInventoryItem : IInventoryItem
    {
        public string Id => "item.variable";

        public string Name => "Variable";

        public string Description => "Create a data variable that flows through connections";

        public InventoryItemType Type => InventoryItemType.ShapeBlueprint;

        public string[] Tags => new[] { "data", "variable", "storage", "value" };

        public string IconResource => "embedded:variable_icon.png";

        public object? Defaults => null;

        public Action<CanvasContext>? Action => null;

        public void RenderIcon(SKCanvas canvas, SKRect bounds)
        {
            var centerX = bounds.MidX;
            var centerY = bounds.MidY;
            var size = System.Math.Min(bounds.Width, bounds.Height);

            // Desenha o círculo de fundo
            using (var bgPaint = new SKPaint
            {
                Color = new SKColor(30, 144, 255, 100),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            })
            {
                canvas.DrawCircle(centerX, centerY, size / 2 - 4, bgPaint);
            }

            // Desenha a borda
            using (var borderPaint = new SKPaint
            {
                Color = SKColors.DodgerBlue,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2,
                IsAntialias = true
            })
            {
                canvas.DrawCircle(centerX, centerY, size / 2 - 4, borderPaint);
            }

            // Desenha o ícone "V" no centro
            using (var iconPaint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2.5f,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round,
                IsAntialias = true
            })
            {
                var iconSize = size * 0.4f;

                var path = new SKPath();
                path.MoveTo(centerX - iconSize / 2, centerY - iconSize / 3);
                path.LineTo(centerX, centerY + iconSize / 2);
                path.LineTo(centerX + iconSize / 2, centerY - iconSize / 3);
                canvas.DrawPath(path, iconPaint);
            }
        }

        public BaseShape CreateShape(SKPoint position)
        {
            return new VariableShape(position);
        }
    }
}