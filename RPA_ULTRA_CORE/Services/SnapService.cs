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
    /// Resultado de snap para forma
    /// </summary>
    public class ShapeSnapResult
    {
        public bool CanSnap { get; set; }
        public IAnchorProvider? TargetShape { get; set; }
        public ShapeAnchor? Anchor { get; set; }
        public SKPoint SnapPoint { get; set; }
    }

    /// <summary>
    /// Serviço para snap-to-grid, snap-to-endpoint, snap-to-line (mid-span) e snap-to-shape
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
        public bool EnableShapeSnap { get; set; } = true;

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

        /// <summary>
        /// Detecta snap em forma (perímetro ou centro)
        /// </summary>
        public ShapeSnapResult? DetectShapeSnap(SKPoint point, IEnumerable<BaseShape> shapes, BaseShape? excludeShape = null)
        {
            if (!EnableShapeSnap)
                return null;

            ShapeSnapResult? bestSnap = null;
            float minDistance = float.MaxValue;

            foreach (var shape in shapes)
            {
                if (shape == excludeShape)
                    continue;

                // Verifica se a forma implementa IAnchorProvider
                if (shape is not IAnchorProvider anchorProvider)
                    continue;

                // Tenta obter a âncora mais próxima
                var anchor = anchorProvider.GetNearestAnchor(point, SnapTolerance);

                if (anchor != null)
                {
                    var distance = Math2D.Distance(point, anchor.World);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        bestSnap = new ShapeSnapResult
                        {
                            CanSnap = true,
                            TargetShape = anchorProvider,
                            Anchor = anchor,
                            SnapPoint = anchor.World
                        };
                    }
                }
            }

            return bestSnap;
        }

        /// <summary>
        /// Snap combinado: tenta primeiro shapes, depois endpoints, depois linhas, e por fim grid
        /// </summary>
        public SKPoint SnapWithPriority(SKPoint point, IEnumerable<BaseShape> shapes, IEnumerable<Node>? nodes = null, BaseShape? excludeShape = null)
        {
            SKPoint result = point;

            // Prioridade 1: Snap em shapes (perímetro/centro)
            if (EnableShapeSnap && shapes != null)
            {
                var shapeSnap = DetectShapeSnap(point, shapes, excludeShape);
                if (shapeSnap?.CanSnap == true)
                {
                    return shapeSnap.SnapPoint;
                }
            }

            // Prioridade 2: Snap em endpoints
            if (EnableEndpointSnap && nodes != null)
            {
                var snappedNode = SnapToNode(point, nodes);
                if (snappedNode != null)
                {
                    return new SKPoint((float)snappedNode.X, (float)snappedNode.Y);
                }
            }

            // Prioridade 3: Snap em linhas (mid-span)
            if (EnableLineSnap && shapes != null)
            {
                var lineSnap = DetectLineSnap(point, shapes, excludeShape);
                if (lineSnap?.CanSnap == true && !lineSnap.IsEndpoint)
                {
                    return lineSnap.SnapPoint;
                }
            }

            // Prioridade 4: Grid
            if (EnableGridSnap)
            {
                result = Math2D.SnapToGrid(point, GridSize);
            }

            return result;
        }
    }
}