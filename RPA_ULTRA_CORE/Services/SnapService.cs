using SkiaSharp;
using RPA_ULTRA_CORE.Models.Geometry;
using RPA_ULTRA_CORE.Utils;
using System.Collections.Generic;

namespace RPA_ULTRA_CORE.Services
{
    /// <summary>
    /// Resultado de snap para linha
    /// </summary>
    public class LineSnapResult
    {
        public bool CanSnap { get; set; }
        public LineShape? TargetLine { get; set; }
        public double T { get; set; }
        public bool IsEndpoint { get; set; }
        public Node? EndpointNode { get; set; }
        public SKPoint SnapPoint { get; set; }
    }

    /// <summary>
    /// Serviço para snap-to-grid, snap-to-endpoint e snap-to-line (mid-span)
    /// </summary>
    public class SnapService
    {
        private const float DefaultGridSize = 8f;
        private const float DefaultSnapTolerance = 10f;

        public float GridSize { get; set; } = DefaultGridSize;
        public float SnapTolerance { get; set; } = DefaultSnapTolerance;
        public bool EnableGridSnap { get; set; } = true;
        public bool EnableEndpointSnap { get; set; } = true;
        public bool EnableLineSnap { get; set; } = true;

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

        /// <summary>
        /// Detecta snap em linha (mid-span ou endpoint)
        /// </summary>
        public LineSnapResult? DetectLineSnap(SKPoint point, IEnumerable<BaseShape> shapes, BaseShape? excludeShape = null)
        {
            if (!EnableLineSnap)
                return null;

            LineSnapResult? bestSnap = null;
            float minDistance = float.MaxValue;

            foreach (var shape in shapes)
            {
                if (shape == excludeShape || shape is not LineShape line)
                    continue;

                var (canConnect, t, isEndpoint) = line.CheckConnectionPoint(point, SnapTolerance);

                if (canConnect)
                {
                    var snapPoint = line.GetPointAtParameter(t);
                    var distance = Math2D.Distance(point, snapPoint);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        bestSnap = new LineSnapResult
                        {
                            CanSnap = true,
                            TargetLine = line,
                            T = t,
                            IsEndpoint = isEndpoint,
                            EndpointNode = isEndpoint ? (t < 0.5 ? line.Start : line.End) : null,
                            SnapPoint = snapPoint
                        };
                    }
                }
            }

            return bestSnap;
        }
    }
}