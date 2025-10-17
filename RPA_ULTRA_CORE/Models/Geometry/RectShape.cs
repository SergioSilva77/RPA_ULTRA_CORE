using SkiaSharp;
using RPA_ULTRA_CORE.Utils;

namespace RPA_ULTRA_CORE.Models.Geometry
{
    /// <summary>
    /// Forma de retângulo com suporte a snap no perímetro
    /// </summary>
    public sealed class RectShape : BaseShape, IAnchorProvider
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

        public event EventHandler? Transformed;

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
            Transformed?.Invoke(this, EventArgs.Empty);
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
            Transformed?.Invoke(this, EventArgs.Empty);
        }

        #region IAnchorProvider Implementation

        /// <summary>
        /// Retorna a âncora mais próxima no perímetro do retângulo
        /// </summary>
        public ShapeAnchor? GetNearestAnchor(SKPoint world, float tolerancePx)
        {
            var rect = GetBounds();
            ShapeAnchor? bestAnchor = null;
            float minDistance = float.MaxValue;

            // Testa cada aresta do retângulo
            var edges = new[]
            {
                // Top edge (left to right)
                (new SKPoint(rect.Left, rect.Top), new SKPoint(rect.Right, rect.Top), 0),
                // Right edge (top to bottom)
                (new SKPoint(rect.Right, rect.Top), new SKPoint(rect.Right, rect.Bottom), 1),
                // Bottom edge (right to left)
                (new SKPoint(rect.Right, rect.Bottom), new SKPoint(rect.Left, rect.Bottom), 2),
                // Left edge (bottom to top)
                (new SKPoint(rect.Left, rect.Bottom), new SKPoint(rect.Left, rect.Top), 3)
            };

            foreach (var (start, end, edgeIndex) in edges)
            {
                // Projeta o ponto na aresta
                var projectedPoint = Math2D.ProjectPointOnSegment(world, start, end);
                var distance = Math2D.Distance(world, projectedPoint);

                if (distance < minDistance && distance <= tolerancePx)
                {
                    minDistance = distance;

                    // Calcula t (posição paramétrica na aresta)
                    float edgeLength = Math2D.Distance(start, end);
                    float t = edgeLength > 0 ? Math2D.Distance(start, projectedPoint) / edgeLength : 0;

                    // Calcula o ângulo normal à aresta
                    float angle = 0;
                    switch (edgeIndex)
                    {
                        case 0: angle = -(float)Math.PI / 2; break; // Top - normal up
                        case 1: angle = 0; break;                    // Right - normal right
                        case 2: angle = (float)Math.PI / 2; break;   // Bottom - normal down
                        case 3: angle = (float)Math.PI; break;       // Left - normal left
                    }

                    bestAnchor = new ShapeAnchor
                    {
                        ShapeId = Id.ToString(),
                        Kind = AnchorKind.Perimeter,
                        World = projectedPoint,
                        Angle = angle,
                        T = t,
                        EdgeIndex = edgeIndex
                    };
                }
            }

            return bestAnchor;
        }

        /// <summary>
        /// Enumera pontos de ancoragem (centro + cantos + meio das arestas)
        /// </summary>
        public IEnumerable<ShapeAnchor> EnumerateAnchors()
        {
            var rect = GetBounds();

            // Centro
            yield return new ShapeAnchor
            {
                ShapeId = Id.ToString(),
                Kind = AnchorKind.Center,
                World = new SKPoint(rect.MidX, rect.MidY),
                Angle = 0,
                T = 0,
                EdgeIndex = 0
            };

            // Cantos
            yield return new ShapeAnchor
            {
                ShapeId = Id.ToString(),
                Kind = AnchorKind.Corner,
                World = new SKPoint(rect.Left, rect.Top),
                Angle = -(float)(Math.PI * 3 / 4),
                T = 0,
                EdgeIndex = 0
            };

            yield return new ShapeAnchor
            {
                ShapeId = Id.ToString(),
                Kind = AnchorKind.Corner,
                World = new SKPoint(rect.Right, rect.Top),
                Angle = -(float)(Math.PI / 4),
                T = 0,
                EdgeIndex = 1
            };

            yield return new ShapeAnchor
            {
                ShapeId = Id.ToString(),
                Kind = AnchorKind.Corner,
                World = new SKPoint(rect.Right, rect.Bottom),
                Angle = (float)(Math.PI / 4),
                T = 0,
                EdgeIndex = 2
            };

            yield return new ShapeAnchor
            {
                ShapeId = Id.ToString(),
                Kind = AnchorKind.Corner,
                World = new SKPoint(rect.Left, rect.Bottom),
                Angle = (float)(Math.PI * 3 / 4),
                T = 0,
                EdgeIndex = 3
            };

            // Meio das arestas
            yield return new ShapeAnchor
            {
                ShapeId = Id.ToString(),
                Kind = AnchorKind.EdgeMid,
                World = new SKPoint(rect.MidX, rect.Top),
                Angle = -(float)Math.PI / 2,
                T = 0.5f,
                EdgeIndex = 0
            };

            yield return new ShapeAnchor
            {
                ShapeId = Id.ToString(),
                Kind = AnchorKind.EdgeMid,
                World = new SKPoint(rect.Right, rect.MidY),
                Angle = 0,
                T = 0.5f,
                EdgeIndex = 1
            };

            yield return new ShapeAnchor
            {
                ShapeId = Id.ToString(),
                Kind = AnchorKind.EdgeMid,
                World = new SKPoint(rect.MidX, rect.Bottom),
                Angle = (float)Math.PI / 2,
                T = 0.5f,
                EdgeIndex = 2
            };

            yield return new ShapeAnchor
            {
                ShapeId = Id.ToString(),
                Kind = AnchorKind.EdgeMid,
                World = new SKPoint(rect.Left, rect.MidY),
                Angle = (float)Math.PI,
                T = 0.5f,
                EdgeIndex = 3
            };
        }

        #endregion
    }
}