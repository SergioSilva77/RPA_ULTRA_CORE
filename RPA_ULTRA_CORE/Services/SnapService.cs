using SkiaSharp;
using RPA_ULTRA_CORE.Models.Geometry;
using RPA_ULTRA_CORE.Utils;

namespace RPA_ULTRA_CORE.Services
{
    /// <summary>
    /// Serviço para snap-to-grid e snap-to-endpoint
    /// </summary>
    public class SnapService
    {
        private const float DefaultGridSize = 8f;
        private const float DefaultSnapTolerance = 10f;

        public float GridSize { get; set; } = DefaultGridSize;
        public float SnapTolerance { get; set; } = DefaultSnapTolerance;
        public bool EnableGridSnap { get; set; } = true;
        public bool EnableEndpointSnap { get; set; } = true;

        /// <summary>
        /// Aplica snap em um ponto
        /// </summary>
        public SKPoint Snap(SKPoint point, IEnumerable<Node>? existingNodes = null)
        {
            SKPoint result = point;

            // Primeiro tenta snap em endpoints
            if (EnableEndpointSnap && existingNodes != null)
            {
                var snappedNode = SnapToNode(point, existingNodes);
                if (snappedNode != null)
                {
                    result = new SKPoint((float)snappedNode.X, (float)snappedNode.Y);
                    return result;
                }
            }

            // Se não snapou em endpoint, tenta grid
            if (EnableGridSnap)
            {
                result = Math2D.SnapToGrid(point, GridSize);
            }

            return result;
        }

        /// <summary>
        /// Tenta fazer snap com um Node existente
        /// </summary>
        public Node? SnapToNode(SKPoint point, IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
            {
                var nodePoint = new SKPoint((float)node.X, (float)node.Y);
                if (Math2D.Distance(point, nodePoint) <= SnapTolerance)
                {
                    return node;
                }
            }

            return null;
        }

        /// <summary>
        /// Snap ou conecta um Node com candidatos
        /// </summary>
        public Node SnapOrConnect(Node node, IEnumerable<Node> candidates)
        {
            var point = new SKPoint((float)node.X, (float)node.Y);
            var snappedNode = SnapToNode(point, candidates);

            if (snappedNode != null && !snappedNode.Equals(node))
            {
                // Retorna o node existente para conexão
                return snappedNode;
            }

            // Aplica snap de grid no próprio node
            if (EnableGridSnap)
            {
                var snappedPoint = Math2D.SnapToGrid(point, GridSize);
                node.Set(snappedPoint.X, snappedPoint.Y);
            }

            return node;
        }

        /// <summary>
        /// Calcula indicador visual de snap
        /// </summary>
        public bool IsSnapping(SKPoint point, IEnumerable<Node>? nodes, out SKPoint snapPoint)
        {
            snapPoint = point;

            if (EnableEndpointSnap && nodes != null)
            {
                var snappedNode = SnapToNode(point, nodes);
                if (snappedNode != null)
                {
                    snapPoint = new SKPoint((float)snappedNode.X, (float)snappedNode.Y);
                    return true;
                }
            }

            if (EnableGridSnap)
            {
                var gridPoint = Math2D.SnapToGrid(point, GridSize);
                if (Math2D.Distance(point, gridPoint) < GridSize / 2)
                {
                    snapPoint = gridPoint;
                    return true;
                }
            }

            return false;
        }
    }
}