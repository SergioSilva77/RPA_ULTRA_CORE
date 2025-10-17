using SkiaSharp;
using RPA_ULTRA_CORE.Utils;

namespace RPA_ULTRA_CORE.Models.Geometry
{
    /// <summary>
    /// Forma de linha conectada por dois Nodes.
    /// Observa mudanças nos Nodes e se atualiza automaticamente.
    /// </summary>
    public sealed class LineShape : BaseShape
    {
        public Node Start { get; private set; }
        public Node End { get; private set; }
        public float StrokeWidth { get; set; } = 2f;
        public SKColor Color { get; set; } = SKColors.White;

        /// <summary>
        /// Raio dos handles de redimensionamento
        /// </summary>
        public const float HandleRadius = 6f;

        public LineShape(Node start, Node end)
        {
            Start = Guard.NotNull(start, nameof(start));
            End = Guard.NotNull(end, nameof(end));

            // Inscreve-se nos eventos dos Nodes (padrão Observer)
            Start.PositionChanged += OnNodePositionChanged;
            End.PositionChanged += OnNodePositionChanged;
        }

        private void OnNodePositionChanged(object? sender, EventArgs e)
        {
            // Aqui poderia notificar o canvas para redesenhar
            // Por enquanto apenas marca como modificado
        }

        public override void Draw(SKCanvas canvas, float dpiScale)
        {
            using var paint = new SKPaint
            {
                Color = IsSelected ? SKColors.Cyan : (IsHovered ? SKColors.LightGray : Color),
                StrokeWidth = StrokeWidth * dpiScale,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };

            // Desenha a linha
            canvas.DrawLine(
                (float)Start.X, (float)Start.Y,
                (float)End.X, (float)End.Y,
                paint);

            // Desenha handles se selecionado
            if (IsSelected)
            {
                DrawHandle(canvas, Start, dpiScale);
                DrawHandle(canvas, End, dpiScale);
            }
        }

        private void DrawHandle(SKCanvas canvas, Node node, float dpiScale)
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

            float radius = HandleRadius * dpiScale;
            canvas.DrawCircle((float)node.X, (float)node.Y, radius, handlePaint);
            canvas.DrawCircle((float)node.X, (float)node.Y, radius, borderPaint);
        }

        public override bool HitTestPoint(SKPoint point, float tolerance)
        {
            // Testa hit na linha usando distância ponto-segmento
            float distance = Math2D.DistancePointToSegment(
                point,
                new SKPoint((float)Start.X, (float)Start.Y),
                new SKPoint((float)End.X, (float)End.Y));

            return distance <= tolerance;
        }

        /// <summary>
        /// Testa se o ponto está sobre um handle
        /// </summary>
        public Node? HitTestHandle(SKPoint point, float tolerance)
        {
            if (Math2D.IsPointInCircle(point, new SKPoint((float)Start.X, (float)Start.Y), tolerance))
                return Start;

            if (Math2D.IsPointInCircle(point, new SKPoint((float)End.X, (float)End.Y), tolerance))
                return End;

            return null;
        }

        /// <summary>
        /// Calcula o parâmetro T [0,1] para o ponto mais próximo na linha
        /// </summary>
        public double GetParametricPosition(SKPoint point)
        {
            var a = new SKPoint((float)Start.X, (float)Start.Y);
            var b = new SKPoint((float)End.X, (float)End.Y);

            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            float lengthSq = dx * dx + dy * dy;

            if (lengthSq < 0.001f)
                return 0; // Linha é praticamente um ponto

            // Projeção do ponto na linha
            float t = ((point.X - a.X) * dx + (point.Y - a.Y) * dy) / lengthSq;

            // Clamp entre 0 e 1
            return Math.Clamp(t, 0, 1);
        }

        /// <summary>
        /// Obtém o ponto na linha para um dado T [0,1]
        /// </summary>
        public SKPoint GetPointAtParameter(double t)
        {
            t = Math.Clamp(t, 0, 1);
            var a = new SKPoint((float)Start.X, (float)Start.Y);
            var b = new SKPoint((float)End.X, (float)End.Y);

            return new SKPoint(
                a.X + (float)(t * (b.X - a.X)),
                a.Y + (float)(t * (b.Y - a.Y))
            );
        }

        /// <summary>
        /// Verifica se um ponto deve conectar em endpoint ou mid-span
        /// </summary>
        public (bool canConnect, double t, bool isEndpoint) CheckConnectionPoint(SKPoint point, float tolerance)
        {
            float distance = Math2D.DistancePointToSegment(
                point,
                new SKPoint((float)Start.X, (float)Start.Y),
                new SKPoint((float)End.X, (float)End.Y));

            if (distance > tolerance)
                return (false, 0, false);

            double t = GetParametricPosition(point);

            // Se muito próximo dos endpoints (2% da linha), conecta no endpoint
            bool isEndpoint = t <= 0.02 || t >= 0.98;

            return (true, t, isEndpoint);
        }

        public override void Move(double dx, double dy)
        {
            // Move ambos os Nodes, propagando para outras linhas conectadas
            Start.Move(dx, dy);
            End.Move(dx, dy);
        }

        public override SKRect GetBounds()
        {
            float minX = Math.Min((float)Start.X, (float)End.X);
            float minY = Math.Min((float)Start.Y, (float)End.Y);
            float maxX = Math.Max((float)Start.X, (float)End.X);
            float maxY = Math.Max((float)Start.Y, (float)End.Y);

            // Expande bounds pelos handles
            return new SKRect(
                minX - HandleRadius,
                minY - HandleRadius,
                maxX + HandleRadius,
                maxY + HandleRadius);
        }

        public override IEnumerable<Node> GetNodes()
        {
            yield return Start;
            yield return End;
        }

        /// <summary>
        /// Substitui um Node por outro (para merge de conexões)
        /// </summary>
        public void ReplaceNode(Node oldNode, Node newNode)
        {
            if (Start.Equals(oldNode))
            {
                Start.PositionChanged -= OnNodePositionChanged;
                Start = newNode;
                Start.PositionChanged += OnNodePositionChanged;
            }

            if (End.Equals(oldNode))
            {
                End.PositionChanged -= OnNodePositionChanged;
                End = newNode;
                End.PositionChanged += OnNodePositionChanged;
            }
        }

        /// <summary>
        /// Limpa recursos e eventos
        /// </summary>
        public void Dispose()
        {
            Start.PositionChanged -= OnNodePositionChanged;
            End.PositionChanged -= OnNodePositionChanged;
        }
    }
}