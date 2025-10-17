using SkiaSharp;

namespace RPA_ULTRA_CORE.Models.Geometry
{
    /// <summary>
    /// Interface para fornecer pontos de ancoragem (snap) em formas
    /// </summary>
    public interface IAnchorProvider
    {
        /// <summary>
        /// Retorna a âncora de snapping mais apropriada a partir do ponto world-space
        /// </summary>
        ShapeAnchor? GetNearestAnchor(SKPoint world, float tolerancePx);

        /// <summary>
        /// Enumera todos os anchors estáticos (cantos, centro, etc.)
        /// </summary>
        IEnumerable<ShapeAnchor> EnumerateAnchors();

        /// <summary>
        /// Evento disparado quando a forma é transformada
        /// </summary>
        event EventHandler? Transformed;
    }

    /// <summary>
    /// Representa um ponto de ancoragem em uma forma
    /// </summary>
    public sealed class ShapeAnchor
    {
        public string ShapeId { get; init; } = "";
        public AnchorKind Kind { get; init; }
        public SKPoint World { get; set; }

        // Parâmetros para recomputar quando a forma muda
        public float T { get; set; }           // Perímetro paramétrico (0..1) para linhas/polígonos
        public float Angle { get; set; }       // Para círculos (ângulo polar)
        public int EdgeIndex { get; set; }     // Para polígonos

        public ShapeAnchor Clone()
        {
            return new ShapeAnchor
            {
                ShapeId = ShapeId,
                Kind = Kind,
                World = World,
                T = T,
                Angle = Angle,
                EdgeIndex = EdgeIndex
            };
        }
    }

    /// <summary>
    /// Tipo de âncora
    /// </summary>
    public enum AnchorKind
    {
        Perimeter,  // Ponto no perímetro
        Corner,     // Vértice/canto
        EdgeMid,    // Meio de uma aresta
        Center,     // Centro da forma
        Custom      // Personalizado
    }
}