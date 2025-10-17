using SkiaSharp;
using RPA_ULTRA_CORE.Utils;

namespace RPA_ULTRA_CORE.Models.Geometry
{
    /// <summary>
    /// Forma de retângulo
    /// </summary>
    public sealed class RectShape : BaseShape
    {
        private readonly Node _topLeft;
        private readonly Node _bottomRight;

        public double X => _topLeft.X;
        public double Y => _topLeft.Y;
        public double Width => _bottomRight.X - _topLeft.X;
        public double Height => _bottomRight.Y - _topLeft.Y;

        public float StrokeWidth { get; set; } = 2f;
        public SKColor Color { get; set; } = SKColors.White;
        public float CornerRadius { get; set; } = 0f;

        public RectShape(double x, double y, double width, double height)
        {
            _topLeft = new Node(x, y);
            _bottomRight = new Node(x + width, y + height);
        }

        public override void Draw(SKCanvas canvas, float dpiScale)
        {
            using var paint = new SKPaint
            {
                Color = IsSelected ? SKColors.Cyan : (IsHovered ? SKColors.LightGray : Color),
                StrokeWidth = StrokeWidth * dpiScale,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            var rect = new SKRect(
                (float)X, (float)Y,
                (float)(_topLeft.X + Width),
                (float)(_topLeft.Y + Height));

            if (CornerRadius > 0)
            {
                canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, paint);
            }
            else
            {
                canvas.DrawRect(rect, paint);
            }

            // Desenha handles se selecionado
            if (IsSelected)
            {
                DrawHandles(canvas, dpiScale);
            }
        }

        private void DrawHandles(SKCanvas canvas, float dpiScale)
        {
            using var handlePaint = new SKPaint
            {
                Color = SKColors.Yellow,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            using var borderPaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 1f * dpiScale,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            float radius = 4f * dpiScale;

            // Cantos
            DrawHandle(canvas, (float)X, (float)Y, radius, handlePaint, borderPaint);
            DrawHandle(canvas, (float)(X + Width), (float)Y, radius, handlePaint, borderPaint);
            DrawHandle(canvas, (float)X, (float)(Y + Height), radius, handlePaint, borderPaint);
            DrawHandle(canvas, (float)(X + Width), (float)(Y + Height), radius, handlePaint, borderPaint);
        }

        private void DrawHandle(SKCanvas canvas, float x, float y, float radius,
            SKPaint fillPaint, SKPaint borderPaint)
        {
            canvas.DrawCircle(x, y, radius, fillPaint);
            canvas.DrawCircle(x, y, radius, borderPaint);
        }

        public override bool HitTestPoint(SKPoint point, float tolerance)
        {
            var rect = GetBounds();
            rect.Inflate(tolerance, tolerance);
            return Math2D.IsPointInRect(point, rect);
        }

        public override void Move(double dx, double dy)
        {
            _topLeft.Move(dx, dy);
            _bottomRight.Move(dx, dy);
        }

        public override SKRect GetBounds()
        {
            return new SKRect(
                (float)X, (float)Y,
                (float)(X + Width),
                (float)(Y + Height));
        }

        public override IEnumerable<Node> GetNodes()
        {
            yield return _topLeft;
            yield return _bottomRight;
        }

        /// <summary>
        /// Redimensiona o retângulo
        /// </summary>
        public void Resize(double newWidth, double newHeight)
        {
            _bottomRight.Set(_topLeft.X + newWidth, _topLeft.Y + newHeight);
        }
    }
}