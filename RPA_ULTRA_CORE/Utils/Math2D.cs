using SkiaSharp;

namespace RPA_ULTRA_CORE.Utils
{
    /// <summary>
    /// Utilitários matemáticos para cálculos 2D
    /// </summary>
    public static class Math2D
    {
        /// <summary>
        /// Calcula a distância entre dois pontos
        /// </summary>
        public static float Distance(SKPoint p1, SKPoint p2)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Calcula a distância de um ponto para um segmento de linha
        /// </summary>
        public static float DistancePointToSegment(SKPoint point, SKPoint lineStart, SKPoint lineEnd)
        {
            float A = point.X - lineStart.X;
            float B = point.Y - lineStart.Y;
            float C = lineEnd.X - lineStart.X;
            float D = lineEnd.Y - lineStart.Y;

            float dot = A * C + B * D;
            float lenSq = C * C + D * D;

            float param = -1;
            if (lenSq != 0) // linha não é um ponto
                param = dot / lenSq;

            float xx, yy;

            if (param < 0)
            {
                xx = lineStart.X;
                yy = lineStart.Y;
            }
            else if (param > 1)
            {
                xx = lineEnd.X;
                yy = lineEnd.Y;
            }
            else
            {
                xx = lineStart.X + param * C;
                yy = lineStart.Y + param * D;
            }

            float dx = point.X - xx;
            float dy = point.Y - yy;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Clamp de valor entre min e max
        /// </summary>
        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// Snap para grid
        /// </summary>
        public static SKPoint SnapToGrid(SKPoint point, float gridSize)
        {
            float x = (float)Math.Round(point.X / gridSize) * gridSize;
            float y = (float)Math.Round(point.Y / gridSize) * gridSize;
            return new SKPoint(x, y);
        }

        /// <summary>
        /// Verifica se ponto está dentro de retângulo
        /// </summary>
        public static bool IsPointInRect(SKPoint point, SKRect rect)
        {
            return point.X >= rect.Left && point.X <= rect.Right &&
                   point.Y >= rect.Top && point.Y <= rect.Bottom;
        }

        /// <summary>
        /// Verifica se ponto está dentro de círculo
        /// </summary>
        public static bool IsPointInCircle(SKPoint point, SKPoint center, float radius)
        {
            return Distance(point, center) <= radius;
        }

        /// <summary>
        /// Projeta um ponto em um segmento de linha
        /// </summary>
        public static SKPoint ProjectPointOnSegment(SKPoint point, SKPoint lineStart, SKPoint lineEnd)
        {
            float A = point.X - lineStart.X;
            float B = point.Y - lineStart.Y;
            float C = lineEnd.X - lineStart.X;
            float D = lineEnd.Y - lineStart.Y;

            float dot = A * C + B * D;
            float lenSq = C * C + D * D;

            float param = -1;
            if (lenSq != 0) // linha não é um ponto
                param = dot / lenSq;

            float xx, yy;

            if (param < 0)
            {
                xx = lineStart.X;
                yy = lineStart.Y;
            }
            else if (param > 1)
            {
                xx = lineEnd.X;
                yy = lineEnd.Y;
            }
            else
            {
                xx = lineStart.X + param * C;
                yy = lineStart.Y + param * D;
            }

            return new SKPoint(xx, yy);
        }
    }
}