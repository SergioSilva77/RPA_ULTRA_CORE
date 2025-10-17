namespace RPA_ULTRA_CORE.Models.Geometry
{
    /// <summary>
    /// Ponto de conexão compartilhado entre formas.
    /// Implementa padrão Observer para notificar mudanças de posição.
    /// Agora suporta anexação a segmentos de linha (mid-span).
    /// </summary>
    public sealed class Node
    {
        private double _x;
        private double _y;
        private SegmentAttachment? _attachment;
        private bool _isUserMoving;

        public Guid Id { get; }
        public double X => _x;
        public double Y => _y;

        /// <summary>
        /// Evento disparado quando a posição do Node muda
        /// </summary>
        public event EventHandler? PositionChanged;

        /// <summary>
        /// Indica se o Node está anexado no meio de um segmento
        /// </summary>
        public bool IsAttachedMidSpan => _attachment?.Mode == AttachmentMode.MidSpan;

        /// <summary>
        /// Indica se o Node está anexado a um endpoint
        /// </summary>
        public bool IsAttachedEndpoint => _attachment?.Mode == AttachmentMode.Endpoint;

        /// <summary>
        /// Indica se o Node está livre (não anexado)
        /// </summary>
        public bool IsFree => _attachment == null;

        public Node(double x, double y)
        {
            Id = Guid.NewGuid();
            _x = x;
            _y = y;
        }

        /// <summary>
        /// Define nova posição e dispara evento se houver mudança
        /// Se anexado mid-span e chamado pelo usuário, desanexa automaticamente
        /// </summary>
        public void Set(double x, double y)
        {
            // Se está sendo movido pelo usuário e está anexado mid-span, desanexa
            if (_isUserMoving && IsAttachedMidSpan)
            {
                Detach();
            }

            if (Math.Abs(_x - x) > 0.001 || Math.Abs(_y - y) > 0.001)
            {
                _x = x;
                _y = y;
                PositionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Método interno para atualização de posição sem desanexar
        /// Usado pelo SegmentAttachment
        /// </summary>
        internal void SetInternal(double x, double y)
        {
            if (Math.Abs(_x - x) > 0.001 || Math.Abs(_y - y) > 0.001)
            {
                _x = x;
                _y = y;
                PositionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Move o Node por deslocamento relativo
        /// </summary>
        public void Move(double dx, double dy)
        {
            _isUserMoving = true;
            Set(_x + dx, _y + dy);
            _isUserMoving = false;
        }

        /// <summary>
        /// Anexa o Node a uma linha em posição paramétrica
        /// </summary>
        public void AttachTo(LineShape parent, double t)
        {
            Detach(); // Remove anexação anterior se houver
            _attachment = new SegmentAttachment(parent, t, this);
            _attachment.Bind();
        }

        /// <summary>
        /// Anexa o Node a outro Node (endpoint)
        /// </summary>
        public void AttachToEndpoint(Node endpointNode)
        {
            Detach(); // Remove anexação anterior se houver
            _attachment = new SegmentAttachment(endpointNode, this);
            _attachment.Bind();
        }

        /// <summary>
        /// Desanexa o Node, tornando-o livre
        /// </summary>
        public void Detach()
        {
            _attachment?.Unbind();
            _attachment = null;
        }

        /// <summary>
        /// Marca o início de movimento pelo usuário
        /// </summary>
        public void BeginUserMove()
        {
            _isUserMoving = true;
        }

        /// <summary>
        /// Marca o fim de movimento pelo usuário
        /// </summary>
        public void EndUserMove()
        {
            _isUserMoving = false;
        }

        /// <summary>
        /// Cria cópia do Node com nova ID
        /// </summary>
        public Node Clone()
        {
            return new Node(_x, _y);
        }

        public override bool Equals(object? obj)
        {
            return obj is Node node && Id.Equals(node.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}