using SkiaSharp;

namespace RPA_ULTRA_CORE.Models.Geometry
{
    /// <summary>
    /// Classe base abstrata para todas as formas no canvas
    /// </summary>
    public abstract class BaseShape
    {
        public Guid Id { get; }
        public bool IsSelected { get; set; }
        public bool IsHovered { get; set; }

        protected BaseShape()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Desenha a forma no canvas
        /// </summary>
        public abstract void Draw(SKCanvas canvas, float dpiScale);

        /// <summary>
        /// Testa se um ponto está sobre a forma
        /// </summary>
        public abstract bool HitTestPoint(SKPoint point, float tolerance);

        /// <summary>
        /// Move a forma por deslocamento
        /// </summary>
        public abstract void Move(double dx, double dy);

        /// <summary>
        /// Retorna os bounds da forma
        /// </summary>
        public abstract SKRect GetBounds();

        /// <summary>
        /// Retorna todos os Nodes da forma para gerenciamento de conexões
        /// </summary>
        public abstract IEnumerable<Node> GetNodes();

        /// <summary>
        /// Verifica se a forma contém um Node específico
        /// </summary>
        public virtual bool ContainsNode(Node node)
        {
            return GetNodes().Contains(node);
        }

        public override bool Equals(object? obj)
        {
            return obj is BaseShape shape && Id.Equals(shape.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}