using SkiaSharp;
using RPA_ULTRA_CORE.Utils;

namespace RPA_ULTRA_CORE.Models.Geometry
{
    /// <summary>
    /// Forma de círculo
    /// </summary>
    public sealed class CircleShape : BaseShape
    {
        private readonly Node _center;
        private double _radius;

        public double CenterX => _center.X;
        public double CenterY => _center.Y;
        public double Radius
        {
            get => _radius;
            set => _radius = Math.Max(1, value);
        }

        public float StrokeWidth { get; set; } = 2f;
        public SKColor Color { get; set; } = SKColors.White;

        public CircleShape(double centerX, double centerY, double radius)
        {
            _center = new Node(centerX, centerY);
            _radius = Math.Max(1, radius);
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

            canvas.DrawCircle((float)CenterX, (float)CenterY, (float)Radius, paint);

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

            float handleRadius = 4f * dpiScale;

            // Handle no centro
            canvas.DrawCircle((float)CenterX, (float)CenterY, handleRadius, handlePaint);
            canvas.DrawCircle((float)CenterX, (float)CenterY, handleRadius, borderPaint);

            // Handles nos pontos cardeais para redimensionar
            DrawHandle(canvas, (float)(CenterX + Radius), (float)CenterY, handleRadius, handlePaint, borderPaint);
            DrawHandle(canvas, (float)(CenterX - Radius), (float)CenterY, handleRadius, handlePaint, borderPaint);
            DrawHandle(canvas, (float)CenterX, (float)(CenterY + Radius), handleRadius, handlePaint, borderPaint);
            DrawHandle(canvas, (float)CenterX, (float)(CenterY - Radius), handleRadius, handlePaint, borderPaint);
        }

        private void DrawHandle(SKCanvas canvas, float x, float y, float radius,
            SKPaint fillPaint, SKPaint borderPaint)
        {
            canvas.DrawCircle(x, y, radius, fillPaint);
            canvas.DrawCircle(x, y, radius, borderPaint);
        }

        public override bool HitTestPoint(SKPoint point, float tolerance)
        {
            float distance = Math2D.Distance(
                point,
                new SKPoint((float)CenterX, (float)CenterY));

            // Testa se está próximo da borda do círculo
            return Math.Abs(distance - Radius) <= tolerance;
        }

        public override void Move(double dx, double dy)
        {
            _center.Move(dx, dy);
        }

        public override SKRect GetBounds()
        {
            return new SKRect(
                (float)(CenterX - Radius),
                (float)(CenterY - Radius),
                (float)(CenterX + Radius),
                (float)(CenterY + Radius));
        }

        public override IEnumerable<Node> GetNodes()
        {
            yield return _center;
        }

        /// <summary>
        /// Testa se ponto está no handle de redimensionamento
        /// </summary>
        public bool HitTestResizeHandle(SKPoint point, float tolerance, out string handleDirection)
        {
            handleDirection = "";

            if (Math2D.IsPointInCircle(point, new SKPoint((float)(CenterX + Radius), (float)CenterY), tolerance))
            {
                handleDirection = "right";
                return true;
            }

            if (Math2D.IsPointInCircle(point, new SKPoint((float)(CenterX - Radius), (float)CenterY), tolerance))
            {
                handleDirection = "left";
                return true;
            }

            if (Math2D.IsPointInCircle(point, new SKPoint((float)CenterX, (float)(CenterY + Radius)), tolerance))
            {
                handleDirection = "bottom";
                return true;
            }

            if (Math2D.IsPointInCircle(point, new SKPoint((float)CenterX, (float)(CenterY - Radius)), tolerance))
            {
                handleDirection = "top";
                return true;
            }

            return false;
        }

        /// <summary>
        /// Redimensiona o círculo baseado na posição do mouse
        /// </summary>
        public void ResizeFromPoint(SKPoint point)
        {
            float distance = Math2D.Distance(
                point,
                new SKPoint((float)CenterX, (float)CenterY));

            Radius = Math.Max(5, distance);
        }
    }
}