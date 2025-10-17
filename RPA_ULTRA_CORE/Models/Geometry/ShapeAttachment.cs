using SkiaSharp;

namespace RPA_ULTRA_CORE.Models.Geometry
{
    /// <summary>
    /// Gerencia a conexão de um Node a uma forma (perímetro ou centro)
    /// </summary>
    public sealed class ShapeAttachment
    {
        private readonly Node _owner;
        private IAnchorProvider? _shape;

        public string ShapeId { get; }
        public AnchorKind Kind { get; }
        public float T { get; private set; }           // Para perímetro/aresta
        public float Angle { get; private set; }       // Para círculo
        public int EdgeIndex { get; private set; }     // Para polígono

        /// <summary>
        /// Construtor para attachment em forma
        /// </summary>
        public ShapeAttachment(Node owner, string shapeId, AnchorKind kind, float t = 0, float angle = 0, int edgeIndex = 0)
        {
            _owner = owner;
            ShapeId = shapeId;
            Kind = kind;
            T = t;
            Angle = angle;
            EdgeIndex = edgeIndex;
        }

        /// <summary>
        /// Vincula aos eventos da forma para atualização automática
        /// </summary>
        public void Bind(IAnchorProvider shape)
        {
            _shape = shape;
            if (_shape != null)
            {
                _shape.Transformed += OnShapeTransformed;
                UpdateOwnerPosition();
            }
        }

        /// <summary>
        /// Desvincula dos eventos
        /// </summary>
        public void Unbind()
        {
            if (_shape != null)
            {
                _shape.Transformed -= OnShapeTransformed;
                _shape = null;
            }
        }

        /// <summary>
        /// Calcula a posição mundial baseada na âncora
        /// </summary>
        public SKPoint GetWorld()
        {
            if (_shape == null)
                return new SKPoint((float)_owner.X, (float)_owner.Y);

            // Pede à forma para recalcular a posição da âncora
            var anchor = _shape.GetNearestAnchor(new SKPoint((float)_owner.X, (float)_owner.Y), float.MaxValue);
            if (anchor != null)
            {
                // Atualiza parâmetros se mudaram
                T = anchor.T;
                Angle = anchor.Angle;
                EdgeIndex = anchor.EdgeIndex;
                return anchor.World;
            }

            return new SKPoint((float)_owner.X, (float)_owner.Y);
        }

        /// <summary>
        /// Callback quando a forma é transformada
        /// </summary>
        private void OnShapeTransformed(object? sender, EventArgs e)
        {
            UpdateOwnerPosition();
        }

        /// <summary>
        /// Atualiza a posição do Node owner baseado na âncora
        /// </summary>
        private void UpdateOwnerPosition()
        {
            var worldPos = GetWorld();
            _owner.SetInternal(worldPos.X, worldPos.Y);
        }
    }
}