using SkiaSharp;
using RPA_ULTRA_CORE.Utils;

namespace RPA_ULTRA_CORE.Models.Geometry
{
    /// <summary>
    /// Forma de círculo com suporte a snap no perímetro
    /// </summary>
    public sealed class CircleShape : BaseShape, IAnchorProvider
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

        public event EventHandler? Transformed;

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
            Transformed?.Invoke(this, EventArgs.Empty);
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
            Transformed?.Invoke(this, EventArgs.Empty);
        }

        #region IAnchorProvider Implementation

        /// <summary>
        /// Retorna a âncora mais próxima no perímetro do círculo
        /// </summary>
        public ShapeAnchor? GetNearestAnchor(SKPoint world, float tolerancePx)
        {
            var center = new SKPoint((float)CenterX, (float)CenterY);
            var vector = world - center;

            // Se muito próximo do centro, usa ângulo do cursor
            float length = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            if (length < 0.001f)
            {
                // Default para direita
                vector = new SKPoint(1, 0);
                length = 1;
            }

            // Normaliza e projeta no perímetro
            vector.X /= length;
            vector.Y /= length;

            var anchorPoint = new SKPoint(
                center.X + vector.X * (float)Radius,
                center.Y + vector.Y * (float)Radius
            );

            // Verifica se está dentro da tolerância
            float distance = Math2D.Distance(world, anchorPoint);
            if (distance > tolerancePx)
                return null;

            return new ShapeAnchor
            {
                ShapeId = Id.ToString(),
                Kind = AnchorKind.Perimeter,
                World = anchorPoint,
                Angle = (float)Math.Atan2(vector.Y, vector.X),
                T = 0, // Não usado para círculo
                EdgeIndex = 0 // Não usado para círculo
            };
        }

        /// <summary>
        /// Enumera pontos de ancoragem (centro + pontos cardeais)
        /// </summary>
        public IEnumerable<ShapeAnchor> EnumerateAnchors()
        {
            // Centro
            yield return new ShapeAnchor
            {
                ShapeId = Id.ToString(),
                Kind = AnchorKind.Center,
                World = new SKPoint((float)CenterX, (float)CenterY),
                Angle = 0,
                T = 0,
                EdgeIndex = 0
            };

            // Pontos cardeais no perímetro
            // Direita
            yield return new ShapeAnchor
            {
                ShapeId = Id.ToString(),
                Kind = AnchorKind.Perimeter,
                World = new SKPoint((float)(CenterX + Radius), (float)CenterY),
                Angle = 0,
                T = 0,
                EdgeIndex = 0
            };

            // Topo
            yield return new ShapeAnchor
            {
                ShapeId = Id.ToString(),
                Kind = AnchorKind.Perimeter,
                World = new SKPoint((float)CenterX, (float)(CenterY - Radius)),
                Angle = -(float)Math.PI / 2,
                T = 0.25f,
                EdgeIndex = 0
            };

            // Esquerda
            yield return new ShapeAnchor
            {
                ShapeId = Id.ToString(),
                Kind = AnchorKind.Perimeter,
                World = new SKPoint((float)(CenterX - Radius), (float)CenterY),
                Angle = (float)Math.PI,
                T = 0.5f,
                EdgeIndex = 0
            };

            // Base
            yield return new ShapeAnchor
            {
                ShapeId = Id.ToString(),
                Kind = AnchorKind.Perimeter,
                World = new SKPoint((float)CenterX, (float)(CenterY + Radius)),
                Angle = (float)Math.PI / 2,
                T = 0.75f,
                EdgeIndex = 0
            };
        }

        #endregion
    }
}