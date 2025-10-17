using SkiaSharp;

namespace RPA_ULTRA_CORE.Models.Geometry
{
    /// <summary>
    /// Modo de anexação do Node
    /// </summary>
    public enum AttachmentMode
    {
        MidSpan,  // Anexado no meio do segmento
        Endpoint  // Anexado em um endpoint
    }

    /// <summary>
    /// Gerencia a conexão de um Node a uma linha (mid-span ou endpoint)
    /// </summary>
    public sealed class SegmentAttachment
    {
        private readonly Node _owner;

        public AttachmentMode Mode { get; }
        public LineShape? ParentLine { get; }
        public Node? EndpointNode { get; }
        public double T { get; private set; } // Posição paramétrica [0,1] quando MidSpan

        /// <summary>
        /// Construtor para anexação mid-span (no meio do segmento)
        /// </summary>
        public SegmentAttachment(LineShape parent, double t, Node owner)
        {
            Mode = AttachmentMode.MidSpan;
            ParentLine = parent;
            T = Math.Clamp(t, 0, 1);
            _owner = owner;
        }

        /// <summary>
        /// Construtor para anexação em endpoint
        /// </summary>
        public SegmentAttachment(Node endpointNode, Node owner)
        {
            Mode = AttachmentMode.Endpoint;
            EndpointNode = endpointNode;
            _owner = owner;
        }

        /// <summary>
        /// Atualiza a posição paramétrica (apenas para MidSpan)
        /// </summary>
        public void SetT(double t)
        {
            if (Mode == AttachmentMode.MidSpan)
            {
                T = Math.Clamp(t, 0, 1);
                UpdateOwnerPosition();
            }
        }

        /// <summary>
        /// Calcula a posição mundial baseada na anexação
        /// </summary>
        public SKPoint GetWorld()
        {
            if (Mode == AttachmentMode.Endpoint && EndpointNode != null)
            {
                return new SKPoint((float)EndpointNode.X, (float)EndpointNode.Y);
            }

            if (Mode == AttachmentMode.MidSpan && ParentLine != null)
            {
                var a = new SKPoint((float)ParentLine.Start.X, (float)ParentLine.Start.Y);
                var b = new SKPoint((float)ParentLine.End.X, (float)ParentLine.End.Y);

                // Interpolação linear entre Start e End usando T
                return new SKPoint(
                    a.X + (b.X - a.X) * (float)T,
                    a.Y + (b.Y - a.Y) * (float)T
                );
            }

            return new SKPoint((float)_owner.X, (float)_owner.Y);
        }

        /// <summary>
        /// Vincula aos eventos para atualização automática
        /// </summary>
        public void Bind()
        {
            if (Mode == AttachmentMode.Endpoint && EndpointNode != null)
            {
                // Observa mudanças no endpoint
                EndpointNode.PositionChanged += OnParentPositionChanged;
            }
            else if (Mode == AttachmentMode.MidSpan && ParentLine != null)
            {
                // Observa mudanças nos endpoints da linha pai
                ParentLine.Start.PositionChanged += OnParentPositionChanged;
                ParentLine.End.PositionChanged += OnParentPositionChanged;
            }

            // Atualiza posição inicial
            UpdateOwnerPosition();
        }

        /// <summary>
        /// Desvincula dos eventos
        /// </summary>
        public void Unbind()
        {
            if (Mode == AttachmentMode.Endpoint && EndpointNode != null)
            {
                EndpointNode.PositionChanged -= OnParentPositionChanged;
            }
            else if (Mode == AttachmentMode.MidSpan && ParentLine != null)
            {
                ParentLine.Start.PositionChanged -= OnParentPositionChanged;
                ParentLine.End.PositionChanged -= OnParentPositionChanged;
            }
        }

        /// <summary>
        /// Callback quando o pai muda de posição
        /// </summary>
        private void OnParentPositionChanged(object? sender, EventArgs e)
        {
            UpdateOwnerPosition();
        }

        /// <summary>
        /// Atualiza a posição do Node owner baseado na anexação
        /// </summary>
        private void UpdateOwnerPosition()
        {
            var worldPos = GetWorld();
            _owner.SetInternal(worldPos.X, worldPos.Y);
        }
    }
}